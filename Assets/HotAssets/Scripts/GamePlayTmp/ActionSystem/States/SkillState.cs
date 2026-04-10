namespace GamePlay.ActionSystem.States
{
    /// <summary>
    /// 技能状态（银河恶魔城版，简化为前摇+判定，支持解锁能力）
    /// </summary>
    public class SkillState : ActionStateBase
    {
        /// <summary>
        /// 当前攻击阶段
        /// </summary>
        public AttackPhase CurrentPhase { get; private set; }

        /// <summary>
        /// 技能ID
        /// </summary>
        private int _skillId;

        /// <summary>
        /// 技能攻击数据
        /// </summary>
        public AttackData SkillAttackData { get; private set; }

        /// <summary>
        /// 判定帧是否已触发
        /// </summary>
        private bool _activeTriggered;

        /// <summary>
        /// 设置技能ID
        /// </summary>
        public void SetSkillId(int skillId)
        {
            _skillId = skillId;
            LoadSkillData(skillId);
        }

        /// <summary>
        /// 加载技能数据
        /// </summary>
        private void LoadSkillData(int skillId)
        {
            // TODO: 从配置表加载技能数据
            SkillAttackData = new AttackData(
                damage: 25f,
                knockbackForce: new UnityEngine.Vector2(5f, 1f)
            );
        }

        /// <summary>
        /// 进入技能状态
        /// </summary>
        public override void Enter()
        {
            base.Enter();
            CurrentPhase = AttackPhase.Startup;
            _activeTriggered = false;

            UnityGameFramework.Runtime.Log.Info($"释放技能: {_skillId} - 前摇");

            // TODO: 播放技能动画
            // TODO: 播放技能音效
        }

        /// <summary>
        /// 更新技能状态（简化为前摇+判定）
        /// </summary>
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            UpdatePhase();
        }

        /// <summary>
        /// 更新技能阶段
        /// </summary>
        private void UpdatePhase()
        {
            if (ElapsedTime < Config.AttackDelay)
            {
                if (CurrentPhase != AttackPhase.Startup)
                {
                    CurrentPhase = AttackPhase.Startup;
                }
            }
            else
            {
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
            UnityGameFramework.Runtime.Log.Info($"技能 {_skillId} - 判定帧");

            // TODO: 生成技能特效
            // TODO: 激活技能判定盒
        }

        /// <summary>
        /// 退出技能状态
        /// </summary>
        public override void Exit()
        {
            base.Exit();
            CurrentPhase = AttackPhase.None;
        }

        /// <summary>
        /// 技能完成回调
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

            // 技能霸体，仅允许受击和死亡打断，完成后回待机
            return target == ActionType.Idle || target == ActionType.Dead || target == ActionType.Hit;
        }
    }
}
