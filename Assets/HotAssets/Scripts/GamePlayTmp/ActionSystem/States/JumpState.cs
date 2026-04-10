namespace GamePlay.ActionSystem.States
{
    /// <summary>
    /// 跳跃状态（银河恶魔城版，支持二段跳）
    /// 垂直速度由PlayerActionController统一管理
    /// </summary>
    public class JumpState : ActionStateBase
    {
        /// <summary>跳跃初速度</summary>
        private const float JumpVelocity = 10f;

        /// <summary>短按跳跃速度（松开跳跃键时的速度）</summary>
        private const float ShortJumpVelocity = 5f;

        /// <summary>空中水平移动速度倍率</summary>
        private const float AirControlMultiplier = 0.8f;

        /// <summary>跳跃键是否按住</summary>
        private bool _jumpKeyHeld;

        /// <summary>当前跳跃次数</summary>
        private int _jumpCount;

        /// <summary>最大跳跃次数（1=单跳，2=二段跳，需解锁）</summary>
        public int MaxJumpCount { get; set; } = 1;

        /// <summary>是否可以二段跳</summary>
        public bool CanDoubleJump => _jumpCount < MaxJumpCount;

        /// <summary>
        /// 进入跳跃状态，设置起跳速度
        /// </summary>
        public override void Enter()
        {
            base.Enter();
            _jumpKeyHeld = true;
            _jumpCount++;

            // 通过Controller设置起跳垂直速度
            StateMachine.SetVerticalVelocity?.Invoke(JumpVelocity);

            UnityGameFramework.Runtime.Log.Info($"执行跳跃（第{_jumpCount}段）");
        }

        /// <summary>
        /// 更新跳跃状态，检测是否开始下落
        /// </summary>
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            // 从Controller获取当前垂直速度，判断是否开始下落
            float velocity = StateMachine.GetVerticalVelocity?.Invoke() ?? 0f;
            if (velocity < 0f)
            {
                StateMachine.ChangeState(ActionType.Fall);
            }
        }

        /// <summary>
        /// 退出跳跃状态
        /// </summary>
        public override void Exit()
        {
            base.Exit();
        }

        /// <summary>
        /// 跳跃完成回调（落地时由Controller调用SetGrounded触发）
        /// </summary>
        protected override void OnActionComplete()
        {
            ResetJumpCount();
            StateMachine.ChangeState(ActionType.Idle);
        }

        /// <summary>
        /// 执行二段跳（由外部调用）
        /// </summary>
        public void PerformDoubleJump()
        {
            StateMachine.SetVerticalVelocity?.Invoke(JumpVelocity);
            _jumpKeyHeld = true;
            UnityGameFramework.Runtime.Log.Info("执行二段跳");
        }

        /// <summary>
        /// 设置跳跃键状态（用于控制跳跃高度）
        /// </summary>
        public void SetJumpKeyHeld(bool held)
        {
            float velocity = StateMachine.GetVerticalVelocity?.Invoke() ?? 0f;

            // 如果松开跳跃键且当前向上跳跃，降低速度实现短跳
            if (_jumpKeyHeld && !held && velocity > ShortJumpVelocity)
            {
                StateMachine.SetVerticalVelocity?.Invoke(ShortJumpVelocity);
                UnityGameFramework.Runtime.Log.Info("短跳");
            }

            _jumpKeyHeld = held;
        }

        /// <summary>
        /// 设置落地状态（由Controller调用）
        /// </summary>
        public void SetGrounded(bool grounded)
        {
            if (grounded)
            {
                ResetJumpCount();
                OnActionComplete();
            }
        }

        /// <summary>
        /// 重置跳跃次数
        /// </summary>
        public void ResetJumpCount()
        {
            _jumpCount = 0;
        }

        /// <summary>
        /// 获取空中控制倍率
        /// </summary>
        public float GetAirControlMultiplier()
        {
            return AirControlMultiplier;
        }

        /// <summary>
        /// 状态转换检查
        /// </summary>
        public override bool CanTransitionTo(ActionStateBase targetState)
        {
            ActionType target = targetState.Config.ActionType;

            return target == ActionType.Idle    // 落地/OnActionComplete 兜底
                || target == ActionType.Fall
                || target == ActionType.WallSlide
                || target == ActionType.Attack
                || target == ActionType.AirAttack
                || target == ActionType.Skill
                || target == ActionType.Dodge
                || target == ActionType.Hit
                || target == ActionType.Dead;
        }
    }
}
