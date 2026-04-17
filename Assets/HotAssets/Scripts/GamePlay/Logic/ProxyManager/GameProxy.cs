using System;
using GameFramework;
using HotAssets.Scripts.Common.Event;
using Unity.Burst;

namespace HotAssets.Scripts.GamePlay.Logic.ProxyManager
{
    public abstract class GameProxy:IReference
    {
        public abstract void Initialize();

        private readonly AAAGameEventHelper m_EventHelper = ReferencePool.Acquire<AAAGameEventHelper>();
        
        public AAAGameEventHelper EventHelper => m_EventHelper;
      
        public virtual void Clear()
        {
            EventHelper.RemoveAllSubscribe();
        }

        [BurstCompile]
        public virtual void LogicUpdate(fix deltaTime)
        {
            
        }

        protected T GetProxy<T>() where T : GameProxy
        {
            return GameProxyManger.Instance.GetProxy<T>();
        }
    }
}