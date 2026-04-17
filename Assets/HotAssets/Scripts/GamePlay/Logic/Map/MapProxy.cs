using System;
using System.Collections.Generic;
using Builtin.Scripts.Extension;
using Builtin.Scripts.Game;
using cfg.Map;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using GameFramework;
using HotAssets.Scripts.Common;
using HotAssets.Scripts.Common.Event;
using HotAssets.Scripts.Extension;
using HotAssets.Scripts.GamePlay.Logic.Common;
using HotAssets.Scripts.GamePlay.Logic.ProxyManager;
using HotAssets.Scripts.GamePlay.Logic.Unit;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GamePlay.Logic.Map
{
    public class MapProxy : GameProxy
    {
        private MapInfo _mapInfo;
        public MapInfo MapInfo => _mapInfo;

        private MapData curMapData;
        public MapData CurMapData => curMapData;

        private TbMapData _tbMapData;
        
        private TbPlatformDef _tbPlatformData;

        // ── 移动平台 ────────────────────────────────────────────────────────
        private readonly List<MapPlatformUnit> _platforms = new List<MapPlatformUnit>();

        /// <summary>只读平台列表，供渲染层或其他系统查询</summary>
        public IReadOnlyList<MapPlatformUnit> Platforms => _platforms;

        private UnitProxy _unitProxy;

        public override async void Initialize()
        {
            try
            {
                _tbMapData = await AppEntry.DataTable.GetDataTableLuBan<TbMapData>(cfg.Tables.map_tbmapdata);
                _tbPlatformData = await AppEntry.DataTable.GetDataTableLuBan<TbPlatformDef>(cfg.Tables.map_tbplatformdef);
            }
            catch (Exception e)
            {
                Log.Error(ZString.Format("MapProxy Initialize error = {0}", e.Message));
            }

            _unitProxy = GetProxy<UnitProxy>();
        }

        public override void LogicUpdate(fix deltaTime)
        {
            UpdatePlatforms(deltaTime);
        }

        public async UniTask LoadMap(int mapId)
        {
            MapData mapData = _tbMapData.GetOrDefault(mapId);

            if (mapData == null)
            {
                Log.Error(ZString.Format("不存在地图数据 {0}", mapId));
                return;
            }

            curMapData = mapData;

            byte[] bytes;
#if UNITY_EDITOR
            bytes = await AppEntry.Resource.LoadBinaryAwait(AssetPathUtil.GetSceneCollisionData(curMapData.Name, true));
#else
            bytes = AppEntry.Resource.LoadBinaryFromFileSystem(AssetPathUtil.GetSceneCollisionData(curMapData.Name, true));
#endif
            _mapInfo = MapCollisionLoader.ParseBytes(bytes);

            if (mapData.PlatformDataId.Count > 0)
            {
                CreatePlatform(mapData.PlatformDataId);
            }

#if UNITY_EDITOR
            ValidateMapInfo(_mapInfo);
#endif

            Fire(GamePlayEvent.ECreateMapRender, curMapData);
        }

        public override void Clear()
        {
            _mapInfo = null;
            for (int i = 0; i < _platforms.Count; i++)
            {
                ReferencePool.Release(_platforms[i]);
            }
            _platforms.Clear();
            base.Clear();
        }

        /// <summary>
        /// 地图中的移动平台数据
        /// </summary>
        /// <param name="idList"></param>
        private void CreatePlatform(List<int> idList)
        {
            for (int i = 0; i < idList.Count; i++)
            {
                PlatformDef def = _tbPlatformData.GetOrDefault(idList[i]);
                if (def != null)
                {
                    MapPlatformUnit testMapPlatform = ReferencePool.Acquire<MapPlatformUnit>();
                    testMapPlatform.PlatformId   = def.Id;
                    testMapPlatform.StartPos     = new fix3(MathUtils.Convert(def.StartX), MathUtils.Convert(def.StartY), 0);
                    testMapPlatform.EndPos       = new fix3(MathUtils.Convert(def.EndX), MathUtils.Convert(def.EndY), 0);
                    testMapPlatform.Position     = testMapPlatform.StartPos;
                    testMapPlatform.PrevPosition = testMapPlatform.Position;
                    testMapPlatform.Speed        = MathUtils.Convert(def.Speed);
                    testMapPlatform.HalfWidth = MathUtils.Convert(def.HalfWidth);
                    testMapPlatform.HalfHeight = MathUtils.Convert(def.HalfHeight);
                    testMapPlatform.AssetPath    = def.AssetPath;
                    _platforms.Add(testMapPlatform);
                }
            }
        }

        // ── 平台每帧逻辑 ─────────────────────────────────────────────────────
        /// <summary>
        /// 推进所有平台位置，并将站在平台上的角色随之携带移动。
        /// 在 UnitProxy（角色物理结算）之前执行，确保 IsGrounded 状态在 CalcMove 读取前已更新。
        /// </summary>
        private void UpdatePlatforms(fix deltaTime)
        {
            if (_platforms.Count == 0 || _unitProxy == null) return;

            for (int pi = 0; pi < _platforms.Count; pi++)
            {
                _platforms[pi].LogicUpdate(deltaTime);
            }

            foreach (var kvp in _unitProxy.RoleUnits)
            {
                RoleUnit roleUnit = kvp.Value as RoleUnit;
                if (roleUnit == null || roleUnit.IsDeath()) continue;

                // 帧首重置：IsGrounded 由本帧各系统重新写入
                // MapProxy（平台）在此之后写，UnitMove（地形）在 UnitProxy 中用 OR 追加，不会覆盖
                roleUnit.RoleState.IsGrounded = false;

                fix3 rPos = roleUnit.Behaviour.Position;
                fix bodyRadius = roleUnit.BodyRadius;

                for (int pi = 0; pi < _platforms.Count; pi++)
                {
                    MapPlatformUnit mapPlatform = _platforms[pi];
                    if (!mapPlatform.IsStandingOn(rPos, bodyRadius)) continue;

                    // 跳跃中（竖向速度 > 0）或从下往上碰撞时，不处理平台逻辑，让其自由穿过
                    // 只有明确从上方落下时才与平台交互
                    bool jumping = roleUnit.RoleBrian.VerticalVelocity > fix.Zero;
                    bool fallingIntoplatform = roleUnit.RoleBrian.VerticalVelocity < fix.Zero;

                    if (jumping)
                    {
                        // 跳跃中直接跳过平台处理，避免穿透和吸附
                        continue;
                    }

                    fix3 delta = mapPlatform.GetDeltaThisFrame();

                    if (fallingIntoplatform && rPos.y > mapPlatform.SurfaceY + roleUnit.BodyRadius)
                    {
                        // 从上往下落地时才吸附到平台表面
                        roleUnit.Behaviour.Position = new fix3(
                            rPos.x + delta.x,
                            mapPlatform.SurfaceY + roleUnit.BodyRadius,
                            rPos.z + delta.z
                        );
                        roleUnit.RoleState.IsGrounded = true;
                        roleUnit.RoleBrian.OnStandOnPlatform();
                    }
                    else
                    {
                        // 其他情况（如平台自身移动）：跟随平台整体移动
                        roleUnit.Behaviour.Position = new fix3(rPos.x + delta.x, rPos.y + delta.y, rPos.z + delta.z);
                    }
                    break; // 每角色同一帧只处理一块平台
                }
            }
        }

#if UNITY_EDITOR
        private void ValidateMapInfo(MapInfo info)
        {
            if (info == null) { Log.Error("[MapProxy] MapInfo 为 null"); return; }

            int w = info.MapWidth(), h = info.MapHeight();
            int blockCount = 0, passCount = 0;
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                if (info.grid[x, y].groundCanPass) passCount++; else blockCount++;

            Log.Info(ZString.Format(
                "[MapProxy] 验证: 宽={0} 高={1} 格子大小={2}x{3} 可通={4} 阻挡={5} border={6}",
                w, h, info.gridSize.x, info.gridSize.y, passCount, blockCount, info.border));
        }
#endif
    }
}