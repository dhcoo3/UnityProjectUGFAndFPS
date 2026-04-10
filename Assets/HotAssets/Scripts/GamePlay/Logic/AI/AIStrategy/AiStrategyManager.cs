using System.Collections.Generic;
using HotAssets.Scripts.GamePlay.Logic.Unit.Component;
using HotAssets.Scripts.GamePlay.Logic.Unit.Core;

namespace HotAssets.Scripts.GamePlay.Logic.AI.AIStrategy
{
    public partial class AiStrategyManager
    {
        // <summary>
        /// AIClip的Condition
        /// 返回值必须是bool，true代表可以，false代表不行
        /// 至于参数，其实是看游戏设计的，所以这里的只是范例
        /// </summary>
        public delegate bool AICondition(IUnit npc,UnitAI unitAI,cfg.AI.AICondition param);
        
        /// <summary>
        /// AIClip的Action
        /// 无论他内部执行了什么，他都得返回AIAction出来，告诉游戏这个AI想干嘛
        /// </summary>
        public delegate AICommand AIAction(IUnit npc,UnitAI unitAI,float deltaTime,AIClip aiClip,cfg.AI.AIAction aiAction);
        
        public static Dictionary<string, AICondition> AIConditions = new Dictionary<string, AICondition>(){
            {"AIResource",AiStrategyManager.CheckAIResource},
            {"CheckPosition",AiStrategyManager.CheckPosition},
            {"CheckEnemyInRange",AiStrategyManager.CheckEnemyInRange},
            {"CheckCanUseSkill",AiStrategyManager.CheckCanUseSkill},
        };

        public static Dictionary<string, AIAction> AIActions = new Dictionary<string, AIAction>(){
            {"MoveTo",AiStrategyManager.AIMoveTo},
            {"WaitTime",AiStrategyManager.AIWaitTime},
            {"Follow",AiStrategyManager.AIFollow},
            {"Patrol",AiStrategyManager.AIPatrol},
            {"UseSkill",AiStrategyManager.AIUseSkill},
        };
    }
}