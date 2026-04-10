namespace GamePlay.ActionSystem.States
{
    /// <summary>
    /// 贴墙下滑状态（银河恶魔城核心traversal能力）
    /// </summary>
    public class WallSlideState : ActionStateBase
    {
        /// <summary>
        /// 贴墙下滑速度（比正常下落慢很多）
        /// </summary>
        private const float WallSlideSpeed = 2f;

        /// <summary>
        /// 当前垂直速度
        /// </summary>
        private float _verticalVelocity;

        /// <summary>
        /// 墙壁方向（1=右侧墙壁，-1=左侧墙壁）
        /// </summary>
        private float _wallDirection;

        /// <summary>
        /// 进入贴墙下滑状态
        /// </summary>
        public override void Enter()
        {
            base.Enter();
            _verticalVelocity = -WallSlideSpeed;

            UnityGameFramework.Runtime.Log.Info("进入贴墙下滑");

            // TODO: 播放贴墙动画
            // TODO: 显示贴墙粒子特效（灰尘）
        }

        /// <summary>
        /// 更新贴墙下滑状态
        /// </summary>
        public override void Update(float deltaTime)
        {
            // 不调用base.Update，贴墙下滑无Duration限制
            ElapsedTime += deltaTime;

            // 持续下滑
            _verticalVelocity = -WallSlideSpeed;

            // TODO: 应用下滑位移
            // TODO: 检测是否仍然接触墙壁
            // TODO: 检测是否落地
        }

        /// <summary>
        /// 退出贴墙下滑状态
        /// </summary>
        public override void Exit()
        {
            base.Exit();
            _verticalVelocity = 0f;
        }

        /// <summary>
        /// 贴墙完成回调（落地）
        /// </summary>
        protected override void OnActionComplete()
        {
            StateMachine.ChangeState(ActionType.Idle);
        }

        /// <summary>
        /// 设置墙壁方向
        /// </summary>
        public void SetWallDirection(float direction)
        {
            _wallDirection = direction;
        }

        /// <summary>
        /// 获取墙壁方向
        /// </summary>
        public float GetWallDirection()
        {
            return _wallDirection;
        }

        /// <summary>
        /// 获取当前下滑速度
        /// </summary>
        public float GetVerticalVelocity()
        {
            return _verticalVelocity;
        }

        /// <summary>
        /// 状态转换检查
        /// </summary>
        public override bool CanTransitionTo(ActionStateBase targetState)
        {
            ActionType target = targetState.Config.ActionType;

            return target == ActionType.WallJump     // 蹬墙跳
                || target == ActionType.Jump         // 离墙跳
                || target == ActionType.Fall         // 离开墙壁
                || target == ActionType.Idle         // 落地
                || target == ActionType.Attack
                || target == ActionType.Hit
                || target == ActionType.Dead;
        }
    }
}
