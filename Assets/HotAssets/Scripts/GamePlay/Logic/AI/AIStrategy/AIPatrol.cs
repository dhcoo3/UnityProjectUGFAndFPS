using HotAssets.Scripts.GamePlay.Logic.AI;
using HotAssets.Scripts.GamePlay.Logic.Role;
using HotAssets.Scripts.GamePlay.Logic.Unit.Component;
using HotAssets.Scripts.GamePlay.Logic.Unit.Core;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;

namespace HotAssets.Scripts.GamePlay.Logic.AI.AIStrategy
{
    public partial class AiStrategyManager
    {
        /// <summary>
        /// 怪物巡逻行为：在 PatrolCenterX ± PatrolHalfRange 范围内左右来回移动。
        /// 巡逻参数从 RoleMonster 读取，边界到达后自动反向，持续执行不返回 Finish。
        /// </summary>
        private static AICommand AIPatrol(IUnit npc, UnitAI unitAI, float deltaTime, AIClip aiClip, cfg.AI.AIAction aiAction)
        {
            if (npc is RoleUnit npcUnit && npcUnit.Data is RoleMonster monsterData)
            {
                AIPatrolData patrolData = unitAI.AIPatrolData;

                // 首次执行时根据 RoleMonster 初始化边界和朝向
                if (!patrolData.Initialized)
                {
                    patrolData.LeftBound  = monsterData.PatrolCenterX - monsterData.PatrolHalfRange;
                    patrolData.RightBound = monsterData.PatrolCenterX + monsterData.PatrolHalfRange;
                    // 初始方向：当前位置偏左则向右，否则向左
                    patrolData.Direction  = npcUnit.Behaviour.Position.x < monsterData.PatrolCenterX ? fix.One : -fix.One;
                    patrolData.Initialized = true;
                }

                fix curX = npcUnit.Behaviour.Position.x;

                // 到达边界时反向
                if (curX >= patrolData.RightBound)
                    patrolData.Direction = -fix.One;
                else if (curX <= patrolData.LeftBound)
                    patrolData.Direction = fix.One;

                return AICommand.CreateMove(new fix3(patrolData.Direction, fix.Zero, fix.Zero));
            }

            return AICommand.Null;
        }
    }
}
