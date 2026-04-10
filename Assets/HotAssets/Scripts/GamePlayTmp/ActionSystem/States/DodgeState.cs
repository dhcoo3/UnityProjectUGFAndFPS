namespace GamePlay.ActionSystem.States
{
    /// <summary>
    /// 翻滚状态（银河恶魔城版，保留无敌帧）
    /// </summary>
    public class DodgeState : ActionStateBase
    {
        /// <summary>
        /// 是否处于无敌帧
        /// </summary>
        public bool IsInvincible { get; private set; }

        /// <summary>
        /// 无敌帧持续时间
        /// </summary>
        private const float InvincibleDuration = 0.25f;

        /// <summary>
        /// 翻滚移动速度
        /// </summary>
        private const float DodgeSpeed = 12f;

        /// <summary>
        /// 翻滚方向
        /// </summary>
        private float _dodgeDirection;

        /// <summary>
        /// 进入翻滚状态
        /// </summary>
        public override void Enter()
        {
            base.Enter();
            IsInvincible = true;

            UnityGameFramework.Runtime.Log.Info("执行翻滚（无敌帧开始）");

            // TODO: 播放翻滚动画
            // TODO: 显示翻滚特效
        }

        /// <summary>
        /// 更新翻滚状态
        /// </summary>
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            // 检查无敌帧是否结束
            if (IsInvincible && ElapsedTime >= InvincibleDuration)
            {
                IsInvincible = false;
            }

            // TODO: 更新翻滚位移
        }

        /// <summary>
        /// 退出翻滚状态
        /// </summary>
        public override void Exit()
        {
            base.Exit();
            IsInvincible = false;
        }

        /// <summary>
        /// 翻滚完成回调
        /// </summary>
        protected override void OnActionComplete()
        {
            StateMachine.ChangeState(ActionType.Idle);
        }

        /// <summary>
        /// 设置翻滚方向
        /// </summary>
        public void SetDodgeDirection(float direction)
        {
            _dodgeDirection = direction;
        }

        /// <summary>
        /// 获取翻滚速度
        /// </summary>
        public float GetDodgeVelocity()
        {
            return DodgeSpeed * _dodgeDirection;
        }

        /// <summary>
        /// 状态转换检查
        /// </summary>
        public override bool CanTransitionTo(ActionStateBase targetState)
        {
            ActionType target = targetState.Config.ActionType;

            // 翻滚完成后可切换到攻击、跳跃、待机
            if (target == ActionType.Idle
                || target == ActionType.Attack
                || target == ActionType.Jump
                || target == ActionType.Dead)
            {
                return true;
            }

            // 无敌期间不受击
            if (IsInvincible && target == ActionType.Hit)
            {
                return false;
            }

            return target == ActionType.Hit;
        }
    }
}
