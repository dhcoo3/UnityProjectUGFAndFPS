using System;
using HotAssets.Scripts.Common.Event;

namespace HotAssets.Scripts.UI.Module.FightShare
{
    #if ENABLE_OBFUZ
    [Obfuz.ObfuzIgnore]
    #endif
    /// <summary>
    /// 该模块常量定义
    /// </summary>
    public class FightShareConst
    {
        public const string OverlayFightHudUI = "OverlayFightHudUI";
        public const string OverlayFightDamageUI = "OverlayFightDamageUI";
        public class Event
        {
            public static readonly int ECreateRole = ++GameEvent.UIEventId;
            public static readonly int EUpdatePos = ++GameEvent.UIEventId;
            public static readonly int EUpdateHp = ++GameEvent.UIEventId;
            public static readonly int ERoleDie = ++GameEvent.UIEventId;
            public static readonly int EPopUpNumber = ++GameEvent.UIEventId;
        }
    }
}
