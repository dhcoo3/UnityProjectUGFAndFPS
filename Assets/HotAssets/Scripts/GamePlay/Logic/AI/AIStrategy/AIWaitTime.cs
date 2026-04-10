using cfg.AI;
using HotAssets.Scripts.Common;
using HotAssets.Scripts.GamePlay.Logic.Unit.Component;
using HotAssets.Scripts.GamePlay.Logic.Unit.Core;

namespace HotAssets.Scripts.GamePlay.Logic.AI.AIStrategy
{
    public partial class AiStrategyManager
    {
        private static AICommand AIWaitTime(IUnit npc, UnitAI unitAI, float deltaTime,AIClip aiClip, cfg.AI.AIAction aiAction)
        {
            if (aiAction is WaitTime waitTime)
            {
                if (unitAI.AIWaitData.TimeElapsed >= MathUtils.Convert(waitTime.Time))
                {
                    unitAI.AIWaitData.TimeElapsed = 0;
                    return AICommand.Finish;
                }
                
                unitAI.AIWaitData.TimeElapsed += deltaTime;
            }
            
            return AICommand.Null;
        }
    }
}