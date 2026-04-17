using System;
using HotAssets.Scripts.Common.Event;

namespace HotAssets.Scripts.GamePlay.Logic.Common
{
    public class GamePlayEvent
    {
        public static readonly int ECreateRoleAll = ++GameEvent.UIEventId;
        public static readonly int ERenderAllRole = ++GameEvent.UIEventId;
        public static readonly int ECreateMapRender = ++GameEvent.UIEventId;
        public static readonly int EFightLoadingFinish = ++GameEvent.UIEventId;
        public static readonly int ECameraFocus = ++GameEvent.UIEventId;
        public static readonly int ERenderBullet = ++GameEvent.UIEventId;
        public static readonly int EStopRenderBullet = ++GameEvent.UIEventId;
        public static readonly int ERenderMonster = ++GameEvent.UIEventId;
        public static readonly int EStopRenderRole = ++GameEvent.UIEventId;
        public static readonly int EPlayEffect = ++GameEvent.UIEventId;
        public static readonly int ERenderAoe = ++GameEvent.UIEventId;
        public static readonly int EStopRenderAoe = ++GameEvent.UIEventId;
        
        public static readonly int EUpdateFrame = ++GameEvent.UIEventId;
        public static readonly int ESvrRoomPlayerInfoUpdate = ++GameEvent.UIEventId;
        
        public static readonly int ELoadSceneSuccess = ++GameEvent.UIEventId;
        public static readonly int ELoadSceneFailure = ++GameEvent.UIEventId;
        public static readonly int ELoadSceneUpdate = ++GameEvent.UIEventId;
        public static readonly int EGameOver = ++GameEvent.UIEventId;
    }
}
