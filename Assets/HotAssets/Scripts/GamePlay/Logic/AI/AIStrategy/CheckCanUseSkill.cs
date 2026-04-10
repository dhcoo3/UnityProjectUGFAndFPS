using HotAssets.Scripts.GamePlay.Logic.Unit.Component;
using HotAssets.Scripts.GamePlay.Logic.Unit.Core;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;

namespace HotAssets.Scripts.GamePlay.Logic.AI.AIStrategy
{
    public partial class AiStrategyManager
    {
        /// <summary>
        /// 自己是否使用任意一个技能
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="unitAI"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private static bool CheckCanUseSkill(IUnit npc, UnitAI unitAI,cfg.AI.AICondition param)
        {
            if (param is cfg.AI.CheckCanUseSkill checkCanUseSkill 
                && npc is RoleUnit roleUnit)
            {
                return false;
            }
            
            return false;
        }
    }
}