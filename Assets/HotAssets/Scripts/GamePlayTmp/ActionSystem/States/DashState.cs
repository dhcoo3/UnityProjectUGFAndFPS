namespace GamePlay.ActionSystem.States
{
    /// <summary>
    /// 冲刺状态（银河恶魔城版）
    /// </summary>
    public class DashState : ActionStateBase
    {
        /// <summary>
        /// 冲刺方向（1为右，-1为左）
        /// </summary>
        private float _dashDirection;

        /// <summary>
        /// 冲刺速度
        /// </summary>
        private const float DashSpeed = 18f;

        /// <summary>
        /// 设置冲刺方向
        /// </summary>
        public void SetDashDirection(float direction)
        {
            _dashDirection = direction > 0 ? 1f : -1f;
        }

        /// <summary>
        /// 进入冲刺状态
        /// </summary>
        public override void Enter()
        {
            base.Enter();

            UnityGameFramework.Runtime.Log.Info($"执行冲刺 - 方向: {(_dashDirection > 0 ? "右" : "左")}");

            // TODO: 播放冲刺动画
            // TODO: 显示冲刺残影特效
        }

        /// <summary>
        /// 更新冲刺状态
        /// </summary>
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            // TODO: 应用冲刺位移
            // float displacement = DashSpeed * _dashDirection * deltaTime;
        }

        /// <summary>
        /// 退出冲刺状态
        /// </summary>
        public override void Exit()
        {
            base.Exit();
        }

        /// <summary>
        /// 冲刺完成回调
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

        /// <summary>
        /// 获取冲刺速度
        /// </summary>
        public float GetDashVelocity()
        {
            return DashSpeed * _dashDirection;
        }
    }
}
