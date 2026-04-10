using HotAssets.Scripts.Common;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;

namespace HotAssets.Scripts.GamePlay.Logic.TimeLine.TimeLineStrategy
{
    public partial class TimeLineStrategyManager
    {
        /// <summary>
        /// 向 Caster 施加持续水平推力 和/或 瞬时竖向速度，供冲刺、二段跳等移动技能使用。
        /// 参数来自配置表 ApplyForceToCaster 节点：
        ///   Speed         - 水平推力速度（米/秒，精度1000），方向跟随 Caster 当前朝向，0=不施加水平力
        ///   Duration      - 水平推力持续时长（秒，精度1000）
        ///   CanMoveInput  - 为 false 时同时锁定移动输入（由 SetCasterControlState 节点负责）
        ///   VerticalSpeed - 竖向速度（米/秒，精度1000），非0时直接设置竖向速度，用于二段跳
        /// </summary>
        private static void ApplyForceToCaster(TimelineObj tlo, TimelineNode timelineNode)
        {
            if (timelineNode.TimelineNodeDef is cfg.Skill.ApplyForceToCaster param
                && tlo.caster is RoleUnit roleUnit
                && !tlo.caster.IsDeath())
            {
                // 竖向速度（二段跳）
                fix verticalSpeed = MathUtils.Convert(param.VerticalSpeed);
                if (verticalSpeed != fix.Zero)
                {
                    roleUnit.RoleBrian.SetVerticalVelocity(verticalSpeed);
                }

                // 水平推力（冲刺）
                fix speed = MathUtils.Convert(param.Speed);
                if (speed != fix.Zero)
                {
                    fix duration = MathUtils.Convert(param.Duration);
                    bool facingRight = roleUnit.RoleBehaviour.FacingRight;
                    fix xVelocity = facingRight ? speed : -speed;
                    roleUnit.RoleBrian.ApplyForce(
                        new fix3(xVelocity, fix.Zero, fix.Zero),
                        duration
                    );
                }
            }
        }
    }
}