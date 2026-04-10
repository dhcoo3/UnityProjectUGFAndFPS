using HotAssets.Scripts.GamePlay.Logic.AI;
using HotAssets.Scripts.GamePlay.Logic.Common;
using HotAssets.Scripts.GamePlay.Logic.Unit.Component;
using HotAssets.Scripts.GamePlay.Logic.Unit.Core;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;

namespace HotAssets.Scripts.GamePlay.Logic.AI.AIStrategy
{
    public partial class AiStrategyManager
    {
        /// <summary>
        /// 使用指定技能。直接调用 UseSkill，不经过 ParseCommand 管线。
        /// 发技能前根据 LookAtTarget 更新瞄准方向，确保射击方向和动画朝向正确。
        /// 返回 Finish，由 AIClip 的 SortType=ToLast 控制再次触发频率（配合技能自身冷却）。
        /// </summary>
        private static AICommand AIUseSkill(IUnit npc, UnitAI unitAI, float deltaTime, AIClip aiClip, cfg.AI.AIAction aiAction)
        {
            if (aiAction is cfg.AI.UseSkill useSkill && npc is RoleUnit npcUnit)
            {
                // 根据目标方向更新瞄准方向，修正射击角度和 Sprite 朝向
                if (npcUnit.RoleBehaviour.LookAtTarget is RoleUnit target)
                {
                    bool targetIsRight = target.Behaviour.Position.x >= npcUnit.Behaviour.Position.x;
                    npcUnit.OrderAim(targetIsRight
                        ? GamePlayDefine.EAimDirection.Right
                        : GamePlayDefine.EAimDirection.Left);
                }

                npcUnit.UseSkill(useSkill.SkillId);
                return AICommand.Finish;
            }

            return AICommand.Null;
        }
    }
}
