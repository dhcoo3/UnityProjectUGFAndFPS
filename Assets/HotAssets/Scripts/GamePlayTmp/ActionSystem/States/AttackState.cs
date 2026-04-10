namespace GamePlay.ActionSystem.States
{
    /// <summary>
    /// 普通攻击状态（银河恶魔城版，简化为前摇+判定，2段连击）
    /// </summary>
    public class AttackState : ActionStateBase
    {
        /// <summary>
        /// 当前攻击阶段
        /// </summary>
        public AttackPhase CurrentPhase { get; private set; }

        /// <summary>
        /// 当前连击数（1-based）
        /// </summary>
        public int ComboCount { get; private set; }

        /// <summary>
        /// 最大连击数（银河城标配2段）
        /// </summary>
        private const int MaxCombo = 2;

        /// <summary>
        /// 每段连击的攻击数据
        /// </summary>
        private AttackData[] _comboAttackData;

        /// <summary>
        /// 当前攻击数据
        /// </summary>
        public AttackData CurrentAttackData { get; private set; }

        /// <summary>
        /// 判定帧是否已触发（防止重复触发）
        /// </summary>
        private bool _activeTriggered;

        public AttackState()
        {
            InitComboData();
        }

        /// <summary>
        /// 初始化连击数据
        /// </summary>
        private void InitComboData()
        {
            _comboAttackData = new AttackData[]
            {
                // 第1段：轻攻击
                new AttackData(10f, new UnityEngine.Vector2(3f, 0.5f)),
                // 第2段：重攻击
                new AttackData(18f, new UnityEngine.Vector2(4f, 1f)),
            };
        }

        /// <summary>
        /// 进入攻击状态
        /// </summary>
        public override void Enter()
        {
            base.Enter();
            CurrentPhase = AttackPhase.Startup;
            _activeTriggered = false;

            // 连击计数
            ComboCount = UnityEngine.Mathf.Clamp(ComboCount + 1, 1, MaxCombo);
            int dataIndex = ComboCount - 1;
            CurrentAttackData = _comboAttackData[dataIndex];

            UnityGameFramework.Runtime.Log.Info($"普通攻击 第{ComboCount}段 - 前摇");

            // TODO: 播放对应连击动画
        }

        /// <summary>
        /// 更新攻击状态（简化为前摇+判定两阶段）
        /// </summary>
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            UpdatePhase();
        }

        /// <summary>
        /// 更新攻击阶段（简化版：前摇→判定）
        /// </summary>
        private void UpdatePhase()
        {
            if (ElapsedTime < Config.AttackDelay)
            {
                // 前摇阶段
                if (CurrentPhase != AttackPhase.Startup)
                {
                    CurrentPhase = AttackPhase.Startup;
                }
            }
            else
            {
                // 判定阶段
                if (CurrentPhase != AttackPhase.Active)
                {
                    CurrentPhase = AttackPhase.Active;
                    if (!_activeTriggered)
                    {
                        _activeTriggered = true;
                        OnEnterActivePhase();
                    }
                }
            }
        }

        /// <summary>
        /// 进入判定帧
        /// </summary>
        private void OnEnterActivePhase()
        {
            UnityGameFramework.Runtime.Log.Info($"普通攻击 第{ComboCount}段 - 判定帧");

            // TODO: 激活攻击盒
            // TODO: 检测攻击盒与受击盒的碰撞
        }

        /// <summary>
        /// 退出攻击状态
        /// </summary>
        public override void Exit()
        {
            base.Exit();
            CurrentPhase = AttackPhase.None;
        }

        /// <summary>
        /// 攻击完成回调
        /// </summary>
        protected override void OnActionComplete()
        {
            // 攻击完成后重置连击数，返回待机
            ComboCount = 0;
            StateMachine.ChangeState(ActionType.Idle);
        }

        /// <summary>
        /// 状态转换检查（银河城简化版）
        /// </summary>
        public override bool CanTransitionTo(ActionStateBase targetState)
        {
            ActionType target = targetState.Config.ActionType;

            // 前摇阶段只允许受击和死亡打断
            if (CurrentPhase == AttackPhase.Startup)
            {
                return target == ActionType.Dead || target == ActionType.Hit;
            }

            // 判定阶段后可被更多动作取消
            return target == ActionType.Idle    // 攻击完成回到待机
                || target == ActionType.Dodge
                || target == ActionType.Jump
                || target == ActionType.Attack  // 连击
                || target == ActionType.Skill
                || target == ActionType.Dead
                || target == ActionType.Hit;
        }

        /// <summary>
        /// 动画事件：判定帧触发（由动画事件驱动）
        /// </summary>
        public void OnAnimationAttackHit()
        {
            if (!_activeTriggered)
            {
                _activeTriggered = true;
                OnEnterActivePhase();
            }
        }
    }
}
