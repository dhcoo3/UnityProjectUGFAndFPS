namespace GamePlay.ActionSystem.States
{
    /// <summary>
    /// 移动状态
    /// </summary>
    public class RunState : ActionStateBase
    {
        /// <summary>
        /// 移动速度
        /// </summary>
        private const float MoveSpeed = 5f;

        /// <summary>
        /// 移动加速度
        /// </summary>
        private const float Acceleration = 20f;

        /// <summary>
        /// 移动减速度
        /// </summary>
        private const float Deceleration = 15f;

        /// <summary>
        /// 当前速度
        /// </summary>
        private float _currentSpeed;

        /// <summary>
        /// 移动方向（1为右，-1为左）
        /// </summary>
        private float _moveDirection;

        public override void Enter()
        {
            base.Enter();
            _currentSpeed = 0f;
        }

        public override void Update(float deltaTime)
        {
            // 移动状态不需要自动完成，由外部输入控制
        }

        /// <summary>
        /// 设置移动方向
        /// </summary>
        public void SetMoveDirection(float direction)
        {
            _moveDirection = direction;
        }

        /// <summary>
        /// 更新移动速度（带加速度）
        /// </summary>
        public void UpdateMovement(float deltaTime)
        {
            if (_moveDirection != 0f)
            {
                // 加速
                _currentSpeed += Acceleration * deltaTime;
                _currentSpeed = UnityEngine.Mathf.Min(_currentSpeed, MoveSpeed);
            }
            else
            {
                // 减速
                _currentSpeed -= Deceleration * deltaTime;
                _currentSpeed = UnityEngine.Mathf.Max(_currentSpeed, 0f);

                // 速度为0时返回待机
                if (_currentSpeed <= 0.01f)
                {
                    StateMachine.ChangeState(ActionType.Idle);
                }
            }
        }

        /// <summary>
        /// 获取当前移动速度
        /// </summary>
        public float GetCurrentSpeed()
        {
            return _currentSpeed * _moveDirection;
        }

        protected override void OnActionComplete()
        {
            // 移动状态不会自动完成
        }

        public override bool CanTransitionTo(ActionStateBase targetState)
        {
            // 移动状态可以切换到大部分状态
            return true;
        }
    }
}
