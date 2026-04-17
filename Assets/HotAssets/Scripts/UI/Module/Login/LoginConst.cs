using System;
using HotAssets.Scripts.Common.Event;

namespace HotAssets.Scripts.UI.Module.Login
{
    public class LoginConst
    {
        public class Event
        {
            public static readonly int LoginSucc = ++GameEvent.UIEventId;
        }
    }
}