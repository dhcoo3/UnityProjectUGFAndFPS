using GameFramework;
using AiStrategyManager = HotAssets.Scripts.GamePlay.Logic.AI.AIStrategy.AiStrategyManager;

namespace HotAssets.Scripts.GamePlay.Logic.AI
{
    /// <summary>
    /// AI条件方法及参数
    /// </summary>
    public class AiConditionData:IReference
    {
        public AiStrategyManager.AICondition method;
        public cfg.AI.AICondition parameter;

        public static AiConditionData Create(AiStrategyManager.AICondition method, cfg.AI.AICondition parameter)
        {
            AiConditionData aiActionData = ReferencePool.Acquire<AiConditionData>();
            aiActionData.method = method;
            aiActionData.parameter = parameter;
            return aiActionData;
        }
        
        public void Clear()
        {
            method = null;
            parameter = null;
        }
    }
}