namespace GamePlay.ActionSystem.States
{
    /// <summary>
    /// 下落状态（银河恶魔城版，支持贴墙转换）
    /// 垂直速度和重力由PlayerActionController统一管理
    /// </summary>
    public class FallState : ActionStateBase
    {
        /// <summary>空中水平移动速度倍率</summary>
        private const float AirControlMultiplier = 0.8f;

        /// <summary>是否已落地</summary>
        private bool _isGrounded;

        /// <summary>
        /// 进入下落状态
        /// </summary>
        public override void Enter()
        {
            base.Enter();
            _isGrounded = false;
        }

        /// <summary>
        /// 更新下落状态，落地后切换到Idle
        /// </summary>
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (_isGrounded)
            {
                OnActionComplete();
            }
        }

        /// <summary>
        /// 退出下落状态
        /// </summary>
        public override void Exit()
        {
            base.Exit();
        }

        /// <summary>
        /// 下落完成回调
        /// </summary>
        protected override void OnActionComplete()
        {
            StateMachine.ChangeState(ActionType.Idle);
        }

        /// <summary>
        /// 设置落地状态（由Controller调用）
        /// </summary>
        public void SetGrounded(bool grounded)
        {
            _isGrounded = grounded;
        }

        /// <summary>
        /// 获取空中控制倍率
        /// </summary>
        public float GetAirControlMultiplier()
        {
            return AirControlMultiplier;
        }

        /// <summary>
        /// 状态转换检查（支持贴墙、二段跳等银河城机制）
        /// </summary>
        public override bool CanTransitionTo(ActionStateBase targetState)
        {
            ActionType target = targetState.Config.ActionType;

            return target == ActionType.Idle        // 落地
                || target == ActionType.WallSlide   // 贴墙
                || target == ActionType.Jump        // 二段跳
                || target == ActionType.Attack
                || target == ActionType.AirAttack   // 空中下砸
                || target == ActionType.Dodge
                || target == ActionType.Hit
                || target == ActionType.Dead;
        }
    }
}
