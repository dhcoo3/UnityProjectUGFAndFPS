using Cysharp.Threading.Tasks;

namespace GamePlay.ActionSystem
{
    /// <summary>
    /// 动作状态基类（银河恶魔城简化版）
    /// </summary>
    public abstract class ActionStateBase
    {
        /// <summary>
        /// 动作配置
        /// </summary>
        public ActionConfig Config { get; protected set; }

        /// <summary>
        /// 状态机引用
        /// </summary>
        protected ActionStateMachine StateMachine;

        /// <summary>
        /// 当前状态已运行时间
        /// </summary>
        protected float ElapsedTime;

        /// <summary>
        /// 是否处于可取消阶段
        /// </summary>
        public bool IsCancelable => Config.CancelableTime >= 0 && ElapsedTime >= Config.CancelableTime;

        /// <summary>
        /// 初始化状态
        /// </summary>
        public virtual void Initialize(ActionStateMachine stateMachine, ActionConfig config)
        {
            StateMachine = stateMachine;
            Config = config;
        }

        /// <summary>
        /// 进入状态
        /// </summary>
        public virtual void Enter()
        {
            ElapsedTime = 0f;
            UnityGameFramework.Runtime.Log.Info($"进入动作状态: {Config.ActionType}");
        }

        /// <summary>
        /// 更新状态
        /// </summary>
        public virtual void Update(float deltaTime)
        {
            ElapsedTime += deltaTime;

            // 检查动作是否完成
            if (Config.Duration > 0f && ElapsedTime >= Config.Duration)
            {
                OnActionComplete();
            }
        }

        /// <summary>
        /// 退出状态
        /// </summary>
        public virtual void Exit()
        {
            UnityGameFramework.Runtime.Log.Info($"退出动作状态: {Config.ActionType}");
        }

        /// <summary>
        /// 动作完成回调
        /// </summary>
        protected virtual void OnActionComplete()
        {
            // 默认返回待机状态
            StateMachine.ChangeState(ActionType.Idle);
        }

        /// <summary>
        /// 检查是否可以切换到目标状态（简化版：仅优先级+可取消检查）
        /// </summary>
        public virtual bool CanTransitionTo(ActionStateBase targetState)
        {
            // 优先级检查
            if (targetState.Config.Priority < Config.Priority && !Config.CanBeInterrupted)
            {
                return false;
            }

            // 可取消检查
            if (targetState.Config.Priority <= Config.Priority && !IsCancelable)
            {
                return false;
            }

            return true;
        }
    }
}
