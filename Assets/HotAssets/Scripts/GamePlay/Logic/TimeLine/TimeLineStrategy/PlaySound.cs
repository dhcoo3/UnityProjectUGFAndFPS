using Builtin.Scripts.Game;
using HotAssets.Scripts.Common;
using HotAssets.Scripts.Extension;

namespace HotAssets.Scripts.GamePlay.Logic.TimeLine.TimeLineStrategy
{
    public partial class TimeLineStrategyManager
    {
        private static void PlaySound(TimelineObj tlo, TimelineNode timelineNode)
        {
            if (timelineNode.TimelineNodeDef is cfg.Skill.PlaySound playSound)
            {
                if (playSound.Only)
                {
                    AppEntry.Sound.PlayEffect(playSound.SoundName,MathUtils.Convert(playSound.Interval));
                }
                else
                {
                    AppEntry.Sound.PlayEffect(playSound.SoundName);
                }
            }
        }
    }
}