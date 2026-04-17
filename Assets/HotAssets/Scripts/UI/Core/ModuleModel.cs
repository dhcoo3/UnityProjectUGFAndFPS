using System;
using GameFramework;
using HotAssets.Scripts.Common.Event;

namespace HotAssets.Scripts.UI.Core
{
    public class ModuleModel<T> where T : class, IModel,new()
    {
        private static T _instance;
        private static readonly object _lock = new object();

        public static T Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new T();
                    }
                    return _instance;
                }
            }
        }

        protected ModuleModel()
        {
            // 防止外部实例化
            if (_instance != null)
            {
                throw new System.Exception($"Only one instance of {typeof(T)} is allowed.");
            }
        }
        
        private readonly AAAGameEventHelper m_EventHelper = ReferencePool.Acquire<AAAGameEventHelper>();
        
        public AAAGameEventHelper EventHelper => m_EventHelper;
        
        public virtual void Clear()
        {
            EventHelper.RemoveAllSubscribe();
        }
    }
}