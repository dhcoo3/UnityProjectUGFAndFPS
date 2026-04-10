namespace GamePlay.ActionSystem.States
{
    /// <summary>
    /// 待机状态
    /// </summary>
    public class IdleState : ActionStateBase
    {
        public override void Enter()
        {
            base.Enter();
            // 待机状态清空输入缓冲
            StateMachine.ClearInputBuffer();
        }

        public override void Update(float deltaTime)
        {
            // 待机状态不需要计时，持续存在
        }

        protected override void OnActionComplete()
        {
            // 待机状态不会自动完成
        }

        public override bool CanTransitionTo(ActionStateBase targetState)
        {
            // 待机状态可以切换到任何状态
            return true;
        }
    }
}
