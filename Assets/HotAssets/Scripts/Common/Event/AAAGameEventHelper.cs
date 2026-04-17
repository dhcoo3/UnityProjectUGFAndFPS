using System;
using System.Collections.Generic;
using Builtin.Scripts.Game;
using GameFramework;
using GameFramework.Event;

namespace HotAssets.Scripts.Common.Event
{
    public class AAAGameEventHelper:IReference
    {
        public class EventData:IReference
        {
            public int Id;
            public EventHandler<GameEventArgs> Listener;
            public List<EventHandler<GameEvent>> Handlers = new List<EventHandler<GameEvent>>();
            public void Clear()
            {
                Id = 0;
                Listener = null;
                Handlers.Clear();
            }
        }

        public class EventArgsData:IReference
        {
            public int Id;
            public EventHandler<GameEventArgs> Listener;
            public List<EventHandler<GameEventArgs>> Handlers = new List<EventHandler<GameEventArgs>>();

            public void Clear()
            {
                Id = 0;
                Listener = null;
                Handlers.Clear();
            }
        }
        
        private readonly Dictionary<int, EventData> _events =new Dictionary<int,EventData>();
        private readonly Dictionary<int, EventArgsData> _eventArgs = new Dictionary<int, EventArgsData>();

        public bool Init = false;
        
        /// <summary>
        /// 注册事件，让事件跟面板绑定，用于关闭面板时释放掉所有事件
        /// </summary>
        /// <param name="id"></param>
        /// <param name="handler"></param>
        public void SubscribeCommon(int id, EventHandler<GameEvent> handler)
        {
            EventData eventData;
            
            if (!_events.TryGetValue(id, out eventData))
            {
                eventData = ReferencePool.Acquire<EventData>();
                _events.Add(id, eventData);
            }
            
            eventData.Handlers.Add(handler);
            
            if (eventData.Listener == null)
            {
                eventData.Listener = (sender, args) =>
                {
                    if (!(args is GameEvent gameEvent))
                    {
                        return;
                    }

                    if (_events.TryGetValue(gameEvent.Id, out EventData _eventData))
                    {
                        for (int i = 0; i < _eventData.Handlers.Count; i++)
                        {
                            _eventData.Handlers[i].Invoke(sender,gameEvent);
                        }
                    }
                };
            }
            
            if (!AppEntry.Event.Check((short)id, eventData.Listener))
            {
                AppEntry.Event.Subscribe((short)id, eventData.Listener);
            }
        }

        public void Subscribe(int id, EventHandler<GameEventArgs> handler)
        {
            EventArgsData eventData;

            if (!_eventArgs.TryGetValue(id, out eventData))
            {
                eventData = ReferencePool.Acquire<EventArgsData>();
                _eventArgs.Add(id, eventData);
            }

            eventData.Handlers.Add(handler);

            if (eventData.Listener == null)
            {
                eventData.Listener = (sender, args) =>
                {
                    if (_eventArgs.TryGetValue(args.Id, out EventArgsData _eventData))
                    {
                        for (int i = 0; i < _eventData.Handlers.Count; i++)
                        {
                            _eventData.Handlers[i].Invoke(sender, args);
                        }
                    }
                };
            }

            if (!AppEntry.Event.Check(id, eventData.Listener))
            {
                AppEntry.Event.Subscribe(id, eventData.Listener);
            }
        }

        /// <summary>
        /// 派发事件
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        public void FireCommon(int id, params object[] args)
        {
            AppEntry.Event.Fire(this, GameEvent.Create(id, args));
        }

        public void Fire(GameEventArgs args)
        {
            AppEntry.Event.Fire(this, args);
        }
        
        /// <summary>
        /// 派发事件
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        public void FireCommonNow(int id, params object[] args)
        {
            AppEntry.Event.FireNow(this, GameEvent.Create(id, args));
        }

        public void FireNow(GameEventArgs args)
        {
            AppEntry.Event.FireNow(this, args);
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="id"></param>
        /// <param name="handler"></param>
        public void UnsubscribeCommon(int id, EventHandler<GameEvent> handler)
        {
            if (_events.TryGetValue(id, out EventData _eventData))
            {
                for (var i = _eventData.Handlers.Count - 1; i >= 0; i--)
                {
                    if (_eventData.Handlers[i] != handler) continue;
                    _eventData.Handlers.RemoveAt(i);
                    break;
                }

                if (_eventData.Handlers.Count != 0)
                {
                    return;
                }
                
                ReferencePool.Release(_eventData);
                _events.Remove(id);
            }
        }

        public void Unsubscribe(int id, EventHandler<GameEventArgs> handler)
        {
            if (_eventArgs.TryGetValue(id, out EventArgsData _eventData))
            {
                for (var i = _eventData.Handlers.Count - 1; i >= 0; i--)
                {
                    if (_eventData.Handlers[i] != handler) continue;
                    _eventData.Handlers.RemoveAt(i);
                    break;
                }

                if (_eventData.Handlers.Count != 0)
                {
                    return;
                }

                if (AppEntry.Event.Check(id, _eventData.Listener))
                {
                    AppEntry.Event.Unsubscribe(id, _eventData.Listener);
                }

                ReferencePool.Release(_eventData);
                _eventArgs.Remove(id);
            }
        }

        /// <summary>
        /// 移除所有绑定UI的事件
        /// </summary>
        public void RemoveAllSubscribe()
        {
            foreach (var (gamePlayEvent, eEventData) in _events)
            {
                AppEntry.Event.Unsubscribe((short)gamePlayEvent, eEventData.Listener);
                ReferencePool.Release(eEventData);
            }

            _events.Clear();

            foreach (var (gamePlayEvent, eEventData) in _eventArgs)
            {
                AppEntry.Event.Unsubscribe(gamePlayEvent, eEventData.Listener);
                ReferencePool.Release(eEventData);
            }

            _eventArgs.Clear();
        }

        public void Clear()
        {
            RemoveAllSubscribe();
        }
    }
}
