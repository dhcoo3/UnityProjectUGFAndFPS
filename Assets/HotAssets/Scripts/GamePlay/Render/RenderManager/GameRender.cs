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
        
        private readonly AAAGameEventHelper _aaaGameEventHelper = ReferencePool.Acquire<AAAGameEventHelper>();

        private readonly Dictionary<int, List<EventHandler<GameEventArgs>>> _events =
            new Dictionary<int, List<EventHandler<GameEventArgs>>>();

        public virtual void Clear()
        {
            ReferencePool.Release(_aaaGameEventHelper);
        }
      
        public virtual void LogicUpdate(fix deltaTime)
        {

        }
      
        protected void Subscribe(Int16 id, EventHandler<GameEvent> handler)
        {
            _aaaGameEventHelper.Subscribe(id,handler);
        }

        protected void Fire(Int16 id, params object[] args)
        {
            _aaaGameEventHelper.Fire(id, args);
        }

        protected void Unsubscribe(Int16 id, EventHandler<GameEvent> handler)
        {
            _aaaGameEventHelper.Unsubscribe(id,handler);
        }

        protected void RemoveAllSubscribe()
        {
            _aaaGameEventHelper.RemoveAllSubscribe();
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