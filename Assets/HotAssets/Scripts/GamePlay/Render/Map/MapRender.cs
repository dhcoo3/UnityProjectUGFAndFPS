using System.Collections.Generic;
using Builtin.Scripts.Event;
using Builtin.Scripts.Game;
using cfg.Map;
using HotAssets.Scripts.Common.Event;
using HotAssets.Scripts.Extension;
using HotAssets.Scripts.GamePlay.Logic.Common;
using HotAssets.Scripts.GamePlay.Logic.Fight;
using HotAssets.Scripts.GamePlay.Logic.Map;
using HotAssets.Scripts.GamePlay.Render.RenderManager;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GamePlay.Render.Map
{
    public class MapRender:GameRender
    {
        ///<summary>
        ///地图的单元格信息，对应角色坐标为[x,z]
        ///</summary>
        private GridInfo[,] _grid;
        
        private readonly Dictionary<int, MapPlatformEntity> _platformEntities =
            new Dictionary<int, MapPlatformEntity>();
        
        private FightLoadingProxy _fightLoadingProxy;
        private AsyncOperation _sceneLoader;
        private bool _loadSceneBundleFailed;
        private int _preloadTotal;
        private int _preloaded;
        private fix _bundleProgress;
        private GameTimer _loadMapTimer;
        private MapProxy _mapProxy;
        private float _gridMaxCount ;
        private int _gridCreateCount ;
        private bool _loadSceneOver;
        
        public override void Initialize()
        {
            _fightLoadingProxy = GetProxy<FightLoadingProxy>();
            _mapProxy = GetProxy<MapProxy>();
            Subscribe(GamePlayEvent.ECreateMapRender,CreateMapView);
        }
        
        public override void LogicUpdate(fix deltaTime)
        {
            foreach (var kv in _platformEntities)
            {
                kv.Value.LogicUpdate(deltaTime);
            }
        }

        public override void Clear()
        {
            _platformEntities.Clear();
            base.Clear();
        }

        public void CreateMapView(object sender, GameEvent e)
        {
            MapData mapData = e.GetParam1<MapData>();
            if (mapData == null)
            {
                Log.Error("地图信息错误");
                return;
            }
            _loadSceneOver = false;
            
            Subscribe(GamePlayEvent.ELoadSceneSuccess,OnLoadSceneSuccess);
            Subscribe(GamePlayEvent.ELoadSceneUpdate,OnLoadSceneUpdate);
            Subscribe(GamePlayEvent.ELoadSceneFailure,OnLoadSceneFailure);
            AppEntry.Event.Fire(this, ChangeSceneArgs.Create(mapData.Name));
        }
        
        private void OnLoadSceneUpdate(object sender, GameEvent e)
        {
            float progress = e.GetParam1<float>();
            string sceneAssetName = e.GetParam2<string>();
            Log.Info("场景加载进度:{0}, {1}", progress, sceneAssetName);
            //TODO 显示场景加载进度
            _fightLoadingProxy.SetSceneProgress(progress);
        }

        private void OnLoadSceneSuccess(object sender, GameEvent e)
        {
            if (_mapProxy.Platforms.Count > 0)
            {
                for (int i = 0; i < _mapProxy.Platforms.Count; i++)
                {
                    OnRenderPlatform(_mapProxy.Platforms[i]);
                }
            }
            
            LoadSceneDone();
        }
    
        //加载场景资源失败 重启游戏框架
        private void OnLoadSceneFailure(object sender, GameEvent e)
        {
            
        }

        private void LoadSceneDone()
        {
            _loadSceneOver = true;
            _fightLoadingProxy.SetSceneProgress(1);
            Unsubscribe(GamePlayEvent.ELoadSceneSuccess,OnLoadSceneSuccess);
            Unsubscribe(GamePlayEvent.ELoadSceneUpdate,OnLoadSceneUpdate);
            Unsubscribe(GamePlayEvent.ELoadSceneFailure,OnLoadSceneFailure);
            AppEntry.Sound.PlayBGM(_mapProxy.CurMapData.Music);
        }
        
        private void OnRenderPlatform(MapPlatformUnit mapPlatformUnit)
        {
            EntityParams param = EntityParams.Create(mapPlatformUnit.Position);
            param.Unit = mapPlatformUnit;
            param.OnShowCallback += LoadPlatformFinish;

            AppEntry.Entity.ShowEntity<MapPlatformEntity>(
                mapPlatformUnit.AssetPath,
                GamePlayDefine.EntityGroup.PlatformEntity,
                GamePlayDefine.LoadPriority.Scene,
                param
            );
        }

        private void LoadPlatformFinish(EntityLogic entityLogic)
        {
            if (entityLogic is MapPlatformEntity platformEntity)
            {
                _platformEntities[platformEntity.PlatformId] = platformEntity;
            }
        }
    }
}