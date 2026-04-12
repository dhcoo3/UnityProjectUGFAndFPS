using AAAGame.ScriptsHotfix.GamePlay.Logic.Map;
using cfg.Fight;
using GameFramework;
using HotAssets.Scripts.Common;
using HotAssets.Scripts.GamePlay.Logic.AI;
using HotAssets.Scripts.GamePlay.Logic.Damage;
using HotAssets.Scripts.GamePlay.Logic.Dungeon.Main;
using HotAssets.Scripts.GamePlay.Logic.Effect;
using HotAssets.Scripts.GamePlay.Logic.Fight;
using HotAssets.Scripts.GamePlay.Logic.Frame;
using HotAssets.Scripts.GamePlay.Logic.GameInput;
using HotAssets.Scripts.GamePlay.Logic.Map;
using HotAssets.Scripts.GamePlay.Logic.MonsterSpawner;
using HotAssets.Scripts.GamePlay.Logic.Player;
using HotAssets.Scripts.GamePlay.Logic.Skill;
using HotAssets.Scripts.GamePlay.Logic.TimeLine;
using HotAssets.Scripts.GamePlay.Logic.Unit;

namespace HotAssets.Scripts.GamePlay.Logic.ProxyManager
{
    public class GameProxyManger:Singleton<GameProxyManger>
    {
        private GameProxy[] _proxyList;
        
        /// <summary>
        /// 通用
        /// </summary>
        public void Register(DungeonType dungeonType)
        {
            //副本放在更新顺序最后
            GameProxy dungeonProxy = GetDungeonProxy(dungeonType);
            
            _proxyList = new[]
            {  
                ReferencePool.Acquire<FrameProxy>(),
                ReferencePool.Acquire<InputProxy>(),
                ReferencePool.Acquire<FightProxy>(),
                ReferencePool.Acquire<FightLoadingProxy>(),
                ReferencePool.Acquire<MapProxy>(),
                ReferencePool.Acquire<UnitProxy>(),
                ReferencePool.Acquire<PlayerProxy>(),
                ReferencePool.Acquire<TimelineProxy>(),
                ReferencePool.Acquire<MonsterSpawnerProxy>(),
                ReferencePool.Acquire<DamageProxy>(),
                ReferencePool.Acquire<SkillProxy>(),
                ReferencePool.Acquire<EffectProxy>(),
                ReferencePool.Acquire<AIProxy>(),
                dungeonProxy
            };
        }

        /// <summary>
        /// 各类副本
        /// </summary>
        /// <param name="dungeonType"></param>
        public GameProxy GetDungeonProxy(DungeonType dungeonType)
        {
            switch (dungeonType)
            {
                case DungeonType.Main:
                    return ReferencePool.Acquire<MainDungeonProxy>();
            }

            return null;
        }

        public void Initialize()
        {
            for(int i = 0;i< _proxyList.Length;i++)
            {
                _proxyList[i].Initialize();
            }
        }

        public T GetProxy<T>() where T : GameProxy
        {
            foreach (var data in _proxyList)
            {
                if (data is T)
                {
                    return (T)data;
                }
            }

            return null;
        }
     
        public void LogicUpdate(fix deltaTime)
        {
            for(int i = 0;i< _proxyList.Length;i++)
            {
                _proxyList[i].LogicUpdate(deltaTime);
            }
        }

        public void Clear()
        {
            for(int i = 0;i< _proxyList.Length;i++)
            {
                ReferencePool.Release(_proxyList[i]);
            }
            
            _proxyList = null;
        }
    }
}