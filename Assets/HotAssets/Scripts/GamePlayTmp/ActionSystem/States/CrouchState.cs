namespace GamePlay.ActionSystem.States
{
    /// <summary>
    /// 下蹲状态（银河恶魔城，用于穿越低矮通道）
    /// </summary>
    public class CrouchState : ActionStateBase
    {
        /// <summary>
        /// 是否正在下蹲
        /// </summary>
        public bool IsCrouching { get; private set; }

        /// <summary>
        /// 进入下蹲状态
        /// </summary>
        public override void Enter()
        {
            base.Enter();
            IsCrouching = true;

            UnityGameFramework.Runtime.Log.Info("进入下蹲状态");

            // TODO: 播放下蹲动画
            // TODO: 缩小碰撞体高度
        }

        /// <summary>
        /// 更新下蹲状态（无Duration限制，由输入控制）
        /// </summary>
        public override void Update(float deltaTime)
        {
            // 不调用base.Update，下蹲由输入控制结束
            ElapsedTime += deltaTime;
        }

        /// <summary>
        /// 退出下蹲状态
        /// </summary>
        public override void Exit()
        {
            base.Exit();
            IsCrouching = false;

            // TODO: 恢复正常碰撞体高度
        }

        /// <summary>
        /// 下蹲完成回调
        /// </summary>
        protected override void OnActionComplete()
        {
            StateMachine.ChangeState(ActionType.Idle);
        }

        /// <summary>
        /// 状态转换检查
        /// </summary>
        public override bool CanTransitionTo(ActionStateBase targetState)
        {
            ActionType target = targetState.Config.ActionType;

            return target == ActionType.Idle
                || target == ActionType.Attack
                || target == ActionType.Hit
                || target == ActionType.Dead;
        }
    }
}
