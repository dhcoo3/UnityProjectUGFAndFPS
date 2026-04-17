using System;
using System.Collections.Generic;
using GameFramework;
using GameFramework.Event;
using HotAssets.Scripts.Common.Event;
using HotAssets.Scripts.GamePlay.Logic.ProxyManager;

namespace HotAssets.Scripts.GamePlay.Render.RenderManager
{
    public abstract class GameRender : IReference
    {
        public abstract void Initialize();
        
        private readonly AAAGameEventHelper m_EventHelper = ReferencePool.Acquire<AAAGameEventHelper>();

        public AAAGameEventHelper EventHelper => m_EventHelper;
        

        public virtual void Clear()
        {
            m_EventHelper.RemoveAllSubscribe();
        }
      
        public virtual void LogicUpdate(fix deltaTime)
        {

        }
      
        protected T GetRender<T>() where T : GameRender
        {
            return GameRenderManager.Instance.GetRender<T>();
        }
        
        protected T GetProxy<T>() where T : GameProxy
        {
            return GameProxyManger.Instance.GetProxy<T>();
        }
    }
}