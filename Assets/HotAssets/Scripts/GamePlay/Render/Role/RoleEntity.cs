using System.Collections.Generic;
using Builtin.Scripts.Game;
using cfg.Anim;
using GameFramework;
using HotAssets.Scripts.Bridge;
using HotAssets.Scripts.Common;
using HotAssets.Scripts.Extension;
using HotAssets.Scripts.GamePlay.Logic.Role;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;
using HotAssets.Scripts.GamePlay.Render.Anim;
using HotAssets.Scripts.GamePlay.Render.Entity;
using HotAssets.Scripts.GamePlay.Render.Map;
using HotAssets.Scripts.UI;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GamePlay.Render.Role
{
    public class RoleEntity:EntityRender
    {
        public int RoleId = 0;

        private RoleUnit _roleUnit;

        private RoleBehaviour _roleBehaviour;

        private RoleData _roleData;

        private UnitAnim _unitAnim;

        private Vector3 _tmpVector3 = Vector3.zero;

     public UnitAnim RoleUnitAnim
        {
            get
            {
                return _unitAnim ??= gameObject.GetOrAddComponent<UnitAnim>();
            }
        }
       
        public VarVector3 Position
        {
            get
            {
                return transform.position;
            }
            set
            {
                transform.position = value;
            }
        }
        
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
        }

        protected override void OnShow(object userData)
        {
            EntityParams entityParams = (EntityParams)userData;
            _roleUnit = entityParams.Unit as RoleUnit;
           
            if (_roleUnit == null)
            {
                Log.Error("RoleData is invalid.");
                return;
            }

            RoleId = _roleUnit.RoleId;
            
            _roleBehaviour = _roleUnit.Behaviour as RoleBehaviour;
            _roleData = _roleUnit.Data;
            
            _tmpVector3.x =  _roleUnit.Behaviour.Position.x;
            _tmpVector3.y =  _roleUnit.Behaviour.Position.y;
            _tmpVector3.z =  _roleUnit.Behaviour.Position.z;
            
            Position = _tmpVector3;
            transform.rotation = _roleUnit.Behaviour.Rotation;

            InitAnimation(_roleData.AnimId);
            
            RoleUnitAnim.RoleBehaviour = _roleBehaviour;
          
            SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer)
            {
                spriteRenderer.sortingOrder = (int)MapRenderLayer.Role;
            }
            
            _roleUnit.HasEntity = true;
            
            entityParams.OnShowCallback.Invoke(this);
            base.OnShow(userData);
        }

        public override void LogicUpdate(fix deltaTime)
        {
            //将逻辑层坐标同步到渲染层
            if(_roleUnit == null) return;

            _tmpVector3.x =  _roleUnit.Behaviour.Position.x;
            _tmpVector3.y =  _roleUnit.Behaviour.Position.y;
            _tmpVector3.z =  _roleUnit.Behaviour.Position.z;

            //更新显示位置
            transform.position = _tmpVector3;
            
            //更新显示旋转,  注释PS：只更新逻辑层方向，显示的方向 由动画决定
            //transform.rotation = _roleBehaviour.Rotation;
            
            RoleUnitAnim.LogicUpdate(deltaTime);
            
            GamePlayToUIBridge.Instance.UpdatePos(_roleUnit.Data.RoleId, transform.position);
            
            base.LogicUpdate(deltaTime);
        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            _roleUnit = null;
            base.OnHide(isShutdown, userData);
        }

        protected override void OnRecycle()
        {
            _roleUnit = null;
            base.OnRecycle();
        }

        /// <summary>
        /// 编辑器 Gizmos 可视化：绿色圆=碰撞范围（BodyRadius），红色圆=身体范围（HitRadius）
        /// 仅运行时有效（_roleUnit 需已初始化）
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            if (_roleUnit == null) return;

            float px = _roleUnit.Behaviour.Position.x;
            float py = _roleUnit.Behaviour.Position.y;
            float bodyRadius = _roleUnit.BodyRadius;
            float hitRadius = _roleUnit.Data.Prop.HitRadius;

            // 碰撞范围（绿色圆，用于移动碰撞和平台站立检测）
            Gizmos.color = new Color(0f, 1f, 0f, 0.8f);
            Gizmos.DrawWireSphere(new Vector3(px, py, 0f), bodyRadius);

            // 身体范围（红色圆，用于子弹命中判定）
            Gizmos.color = new Color(1f, 0f, 0f, 0.8f);
            Gizmos.DrawWireSphere(new Vector3(px, py, 0f), hitRadius);

            // 圆心标记（黄色小球）
            Gizmos.color = new Color(1f, 1f, 0f, 1f);
            float markerSize = Mathf.Max(bodyRadius, hitRadius) * 0.1f;
            Gizmos.DrawSphere(new Vector3(px, py, 0f), markerSize);

            // 绘制地图碰撞网格
            DrawMapGrid();
        }

        /// <summary>
        /// 绘制地图碰撞网格（灰色表示不可通过的网格）
        /// </summary>
        private void DrawMapGrid()
        {
            try
            {
                var mapProxy = HotAssets.Scripts.GamePlay.Logic.ProxyManager.GameProxyManger.Instance?
                    .GetProxy<HotAssets.Scripts.GamePlay.Logic.Map.MapProxy>();
                if (mapProxy?.MapInfo == null) return;

                var mapInfo = mapProxy.MapInfo;
                int width = mapInfo.MapWidth();
                int height = mapInfo.MapHeight();
                float gridSizeX = (float)mapInfo.gridSize.x;
                float gridSizeY = (float)mapInfo.gridSize.y;
                float originX = (float)mapInfo.origin.x;
                float originY = (float)mapInfo.origin.y;

                // 绘制不可通过的网格（灰色）
                Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (!mapInfo.grid[x, y].groundCanPass)
                        {
                            float gridX = originX + (x + 0.5f) * gridSizeX;
                            float gridY = originY + (y + 0.5f) * gridSizeY;
                            Gizmos.DrawCube(new Vector3(gridX, gridY, 0f),
                                          new Vector3(gridSizeX, gridSizeY, 0.1f));
                        }
                    }
                }

                // 绘制可通过的网格边界（绿色细线）
                Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
                for (int x = 0; x <= width; x++)
                {
                    float gridX = originX + x * gridSizeX;
                    Gizmos.DrawLine(new Vector3(gridX, originY, 0f),
                                  new Vector3(gridX, originY + height * gridSizeY, 0f));
                }
                for (int y = 0; y <= height; y++)
                {
                    float gridY = originY + y * gridSizeY;
                    Gizmos.DrawLine(new Vector3(originX, gridY, 0f),
                                  new Vector3(originX + width * gridSizeX, gridY, 0f));
                }
            }
            catch { }
        }

        /// <summary>
        /// 初始化动画组
        /// </summary>
        /// <param name="animGroup">动画组</param>
        private async void InitAnimation(int animGroup)
        {
           if (animGroup <= 0) return;
            
            Dictionary<cfg.Anim.Type, AnimInfo> animDic = new Dictionary<cfg.Anim.Type, AnimInfo>();
            TbAnimGroup tbAnimGroup = await AppEntry.DataTable.GetDataTableLuBan<TbAnimGroup>(cfg.Tables.anim_tbanimgroup);
            TbAnimDef tbAnimDef = await AppEntry.DataTable.GetDataTableLuBan<TbAnimDef>(cfg.Tables.anim_tbanimdef);
            
            AnimGroup data = tbAnimGroup.GetOrDefault(animGroup);
            if (data == null)
            {
                Log.Warning("AnimGroup is invalid. {0}",animGroup);
                return;
            }

            for (int i = 0; i < data.AnimList.Count; i++)
            {
                AnimDef anim = tbAnimDef.GetOrDefault(data.AnimList[i]);
                
                if (anim == null)
                {
                    Log.Warning("AnimDef is invalid. {0}",data.AnimList[i]);
                    continue;
                }
                
                animDic.TryAdd(anim.Type, new AnimInfo(anim.Type,anim.Priority));

                if (animDic.TryGetValue(anim.Type, out AnimInfo animInfo))
                {
                    animInfo.allAnims.TryAdd(anim.Direction,new List<SingleAnimInfo>());

                    if (animInfo.allAnims.TryGetValue(anim.Direction, out List<SingleAnimInfo> allAnimInfo))
                    {
                        for (int j = 0; j < anim.Sub.Length; j++)
                        {
                            SingleAnimInfo singleAnimInfo = new SingleAnimInfo(anim.Sub[j].AnimName,
                                j + 1,
                                MathUtils.Convert(anim.Sub[j].Duration),
                                 anim.Direction);
                            allAnimInfo.Add(singleAnimInfo);
                        }
                    }
                }
            }

            RoleUnitAnim.animInfo = animDic;
        }
    }
}
