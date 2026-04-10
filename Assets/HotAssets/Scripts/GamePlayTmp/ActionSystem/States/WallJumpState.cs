namespace GamePlay.ActionSystem.States
{
    /// <summary>
    /// 蹬墙跳状态（银河恶魔城核心traversal能力）
    /// </summary>
    public class WallJumpState : ActionStateBase
    {
        /// <summary>
        /// 蹬墙跳垂直速度
        /// </summary>
        private const float WallJumpVerticalVelocity = 10f;

        /// <summary>
        /// 蹬墙跳水平速度
        /// </summary>
        private const float WallJumpHorizontalVelocity = 8f;

        /// <summary>
        /// 重力加速度
        /// </summary>
        private const float Gravity = 20f;

        /// <summary>
        /// 当前垂直速度
        /// </summary>
        private float _verticalVelocity;

        /// <summary>
        /// 当前水平速度（远离墙壁方向）
        /// </summary>
        private float _horizontalVelocity;

        /// <summary>
        /// 蹬墙跳方向（与墙壁方向相反）
        /// </summary>
        private float _jumpDirection;

        /// <summary>
        /// 进入蹬墙跳状态
        /// </summary>
        public override void Enter()
        {
            base.Enter();
            _verticalVelocity = WallJumpVerticalVelocity;
            _horizontalVelocity = WallJumpHorizontalVelocity * _jumpDirection;

            UnityGameFramework.Runtime.Log.Info($"蹬墙跳 - 方向: {(_jumpDirection > 0 ? "右" : "左")}");

            // TODO: 播放蹬墙跳动画
            // TODO: 播放蹬墙跳音效
        }

        /// <summary>
        /// 更新蹬墙跳状态
        /// </summary>
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            // 应用重力
            _verticalVelocity -= Gravity * deltaTime;

            // TODO: 应用位移（垂直+水平）

            // 开始下落时切换到Fall状态
            if (_verticalVelocity < 0f)
            {
                StateMachine.ChangeState(ActionType.Fall);
            }
        }

        /// <summary>
        /// 退出蹬墙跳状态
        /// </summary>
        public override void Exit()
        {
            base.Exit();
            _verticalVelocity = 0f;
            _horizontalVelocity = 0f;
        }

        /// <summary>
        /// 蹬墙跳完成回调
        /// </summary>
        protected override void OnActionComplete()
        {
            StateMachine.ChangeState(ActionType.Fall);
        }

        /// <summary>
        /// 设置蹬墙跳方向（与墙壁方向相反）
        /// </summary>
        public void SetJumpDirection(float wallDirection)
        {
            // 蹬墙跳方向与墙壁方向相反
            _jumpDirection = -wallDirection;
        }

        /// <summary>
        /// 获取当前垂直速度
        /// </summary>
        public float GetVerticalVelocity()
        {
            return _verticalVelocity;
        }

        /// <summary>
        /// 获取当前水平速度
        /// </summary>
        public float GetHorizontalVelocity()
        {
            return _horizontalVelocity;
        }

        /// <summary>
        /// 状态转换检查
        /// </summary>
        public override bool CanTransitionTo(ActionStateBase targetState)
        {
            ActionType target = targetState.Config.ActionType;

            return target == ActionType.Fall
                || target == ActionType.Attack
                || target == ActionType.Hit
                || target == ActionType.Dead;
        }
    }
}
