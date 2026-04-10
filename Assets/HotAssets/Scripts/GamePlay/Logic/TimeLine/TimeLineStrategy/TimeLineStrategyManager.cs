using System.Collections.Generic;

namespace HotAssets.Scripts.GamePlay.Logic.TimeLine.TimeLineStrategy
{
    public partial class TimeLineStrategyManager
    {
        public delegate void TimelineEvent(TimelineObj timeline,TimelineNode timelineNode);
        
        public static Dictionary<string, TimelineEvent> functions = new Dictionary<string, TimelineEvent>()
        {
            {"FireBullet", FireBullet},
            {"SetCasterControlState", SetCasterControlState},
            {"CasterPlayAnim", CasterPlayAnim},
            {"PlaySightEffectOnCaster", PlaySightEffectOnCaster},
            {"UseAoeToPoint",UseAoeToPoint},
            {"PlaySound",PlaySound},
            {"ApplyForceToCaster", ApplyForceToCaster}
        };
    }
}