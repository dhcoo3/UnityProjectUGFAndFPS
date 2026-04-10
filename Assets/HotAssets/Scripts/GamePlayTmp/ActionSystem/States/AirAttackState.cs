namespace GamePlay.ActionSystem.States
{
    /// <summary>
    /// 空中下砸攻击状态（银河恶魔城经典能力）
    /// </summary>
    public class AirAttackState : ActionStateBase
    {
        /// <summary>
        /// 下砸速度
        /// </summary>
        private const float SlamSpeed = 25f;

        /// <summary>
        /// 下砸前短暂滞空时间
        /// </summary>
        private const float HoverDuration = 0.15f;

        /// <summary>
        /// 当前垂直速度
        /// </summary>
        private float _verticalVelocity;

        /// <summary>
        /// 是否已开始下砸
        /// </summary>
        private bool _isSlamming;

        /// <summary>
        /// 是否已落地
        /// </summary>
        private bool _isGrounded;

        /// <summary>
        /// 下砸攻击数据
        /// </summary>
        public AttackData SlamAttackData { get; private set; }

        public AirAttackState()
        {
            // 下砸攻击数据
            SlamAttackData = new AttackData(
                damage: 20f,
                knockbackForce: new UnityEngine.Vector2(2f, 3f)
            );
        }

        /// <summary>
        /// 进入空中下砸状态
        /// </summary>
        public override void Enter()
        {
            base.Enter();
            _verticalVelocity = 0f;
            _isSlamming = false;
            _isGrounded = false;

            UnityGameFramework.Runtime.Log.Info("空中下砸 - 滞空准备");

            // TODO: 播放滞空动画
        }

        /// <summary>
        /// 更新下砸状态
        /// </summary>
        public override void Update(float deltaTime)
        {
            // 不调用base.Update，由落地触发完成
            ElapsedTime += deltaTime;

            if (!_isSlamming)
            {
                // 滞空阶段
                if (ElapsedTime >= HoverDuration)
                {
                    _isSlamming = true;
                    _verticalVelocity = -SlamSpeed;
                    UnityGameFramework.Runtime.Log.Info("空中下砸 - 开始下砸");

                    // TODO: 播放下砸动画
                    // TODO: 激活攻击判定
                }
            }
            else
            {
                // 下砸阶段
                // TODO: 应用下砸位移
                // TODO: 检测地面碰撞

                if (_isGrounded)
                {
                    OnLanded();
                }
            }
        }

        /// <summary>
        /// 落地冲击处理
        /// </summary>
        private void OnLanded()
        {
            UnityGameFramework.Runtime.Log.Info("空中下砸 - 落地冲击");

            // TODO: 产生落地冲击波范围伤害
            // TODO: 播放落地特效
            // TODO: 屏幕震动

            StateMachine.ChangeState(ActionType.Idle);
        }

        /// <summary>
        /// 退出下砸状态
        /// </summary>
        public override void Exit()
        {
            base.Exit();
            _verticalVelocity = 0f;
        }

        /// <summary>
        /// 下砸完成回调
        /// </summary>
        protected override void OnActionComplete()
        {
            StateMachine.ChangeState(ActionType.Idle);
        }

        /// <summary>
        /// 设置落地状态（由外部碰撞检测调用）
        /// </summary>
        public void SetGrounded(bool grounded)
        {
            _isGrounded = grounded;
        }

        /// <summary>
        /// 获取当前垂直速度
        /// </summary>
        public float GetVerticalVelocity()
        {
            return _verticalVelocity;
        }

        /// <summary>
        /// 是否正在下砸中
        /// </summary>
        public bool IsSlamming()
        {
            return _isSlamming;
        }

        /// <summary>
        /// 状态转换检查
        /// </summary>
        public override bool CanTransitionTo(ActionStateBase targetState)
        {
            ActionType target = targetState.Config.ActionType;

            // 下砸中仅允许受击和死亡打断
            return target == ActionType.Idle    // 落地
                || target == ActionType.Hit
                || target == ActionType.Dead;
        }
    }
}
