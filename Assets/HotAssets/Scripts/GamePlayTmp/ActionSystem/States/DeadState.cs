namespace GamePlay.ActionSystem.States
{
    /// <summary>
    /// 死亡状态
    /// </summary>
    public class DeadState : ActionStateBase
    {
        /// <summary>
        /// 死亡动画持续时间
        /// </summary>
        private const float DeathAnimationDuration = 2f;

        public override void Enter()
        {
            base.Enter();

            UnityGameFramework.Runtime.Log.Info("角色死亡");

            // TODO: 播放死亡动画
            // TODO: 播放死亡音效
            // TODO: 显示死亡特效
            // TODO: 禁用所有输入
            // TODO: 禁用碰撞
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            // 死亡动画播放完成后
            if (ElapsedTime >= DeathAnimationDuration)
            {
                OnActionComplete();
            }
        }

        public override void Exit()
        {
            base.Exit();
        }

        protected override void OnActionComplete()
        {
            // 死亡状态完成后不切换状态，等待外部处理（如重生、游戏结束等）
            UnityGameFramework.Runtime.Log.Info("死亡动画播放完成");

            // TODO: 触发死亡事件，通知游戏管理器
        }

        public override bool CanTransitionTo(ActionStateBase targetState)
        {
            // 死亡状态不能切换到任何其他状态
            return false;
        }
    }
}
