using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace GamePlay.ActionSystem
{
    /// <summary>
    /// 动作状态机（银河恶魔城简化版）
    /// </summary>
    public class ActionStateMachine
    {
        /// <summary>
        /// 所有状态字典
        /// </summary>
        private Dictionary<ActionType, ActionStateBase> _states;

        /// <summary>
        /// 当前状态
        /// </summary>
        public ActionStateBase CurrentState { get; private set; }

        /// <summary>
        /// 垂直速度控制委托（由Controller设置）
        /// </summary>
        public System.Action<float> SetVerticalVelocity;

        /// <summary>
        /// 获取垂直速度委托（由Controller设置）
        /// </summary>
        public System.Func<float> GetVerticalVelocity;

        /// <summary>
        /// 冷却时间记录
        /// </summary>
        private Dictionary<ActionType, float> _cooldownTimers;

        /// <summary>
        /// 冷却Key缓存（避免每帧GC）
        /// </summary>
        private List<ActionType> _cooldownKeyCache;

        /// <summary>
        /// 输入缓冲队列
        /// </summary>
        private Queue<ActionType> _inputBuffer;

        /// <summary>
        /// 输入缓冲最大容量（银河城简化为1）
        /// </summary>
        private const int MaxBufferSize = 1;

        /// <summary>
        /// 状态改变事件
        /// </summary>
        public event Action<ActionType, ActionType> OnStateChanged;

        public ActionStateMachine()
        {
            _states = new Dictionary<ActionType, ActionStateBase>();
            _cooldownTimers = new Dictionary<ActionType, float>();
            _cooldownKeyCache = new List<ActionType>();
            _inputBuffer = new Queue<ActionType>();
        }

        /// <summary>
        /// 注册状态
        /// </summary>
        public void RegisterState(ActionStateBase state, ActionConfig config)
        {
            state.Initialize(this, config);
            _states[config.ActionType] = state;
            _cooldownTimers[config.ActionType] = 0f;
            // 更新缓存key列表
            _cooldownKeyCache.Clear();
            _cooldownKeyCache.AddRange(_cooldownTimers.Keys);
        }

        /// <summary>
        /// 初始化状态机（设置初始状态）
        /// </summary>
        public void Initialize()
        {
            if (_states.ContainsKey(ActionType.Idle))
            {
                CurrentState = _states[ActionType.Idle];
                CurrentState.Enter();
            }
            else
            {
                UnityGameFramework.Runtime.Log.Error("状态机初始化失败：未找到待机状态");
            }
        }

        /// <summary>
        /// 更新状态机
        /// </summary>
        public void Update(float deltaTime)
        {
            // 更新当前状态
            CurrentState?.Update(deltaTime);

            // 更新冷却计时器
            UpdateCooldowns(deltaTime);

            // 处理输入缓冲
            ProcessInputBuffer();
        }

        /// <summary>
        /// 请求切换状态
        /// </summary>
        public bool ChangeState(ActionType targetType)
        {
            // 检查目标状态是否存在
            if (!_states.ContainsKey(targetType))
            {
                UnityGameFramework.Runtime.Log.Warning($"状态切换失败：未找到状态 {targetType}");
                return false;
            }

            ActionStateBase targetState = _states[targetType];

            // 检查冷却
            if (_cooldownTimers[targetType] > 0f)
            {
                return false;
            }

            // 检查是否可以切换
            if (CurrentState != null && !CurrentState.CanTransitionTo(targetState))
            {
                // 如果允许输入缓冲，加入缓冲队列
                if (targetState.Config.AllowInputBuffer && _inputBuffer.Count < MaxBufferSize)
                {
                    _inputBuffer.Enqueue(targetType);
                }
                return false;
            }

            // 执行状态切换
            ActionType previousType = CurrentState?.Config.ActionType ?? ActionType.None;

            CurrentState?.Exit();
            CurrentState = targetState;
            CurrentState.Enter();

            // 设置冷却
            if (targetState.Config.CooldownTime > 0f)
            {
                _cooldownTimers[targetType] = targetState.Config.CooldownTime;
            }

            // 触发事件
            OnStateChanged?.Invoke(previousType, targetType);

            return true;
        }

        /// <summary>
        /// 更新冷却计时器（使用缓存key列表避免GC）
        /// </summary>
        private void UpdateCooldowns(float deltaTime)
        {
            for (int i = 0; i < _cooldownKeyCache.Count; i++)
            {
                var key = _cooldownKeyCache[i];
                if (_cooldownTimers[key] > 0f)
                {
                    _cooldownTimers[key] -= deltaTime;
                    if (_cooldownTimers[key] < 0f)
                    {
                        _cooldownTimers[key] = 0f;
                    }
                }
            }
        }

        /// <summary>
        /// 处理输入缓冲队列
        /// </summary>
        private void ProcessInputBuffer()
        {
            if (_inputBuffer.Count > 0)
            {
                ActionType bufferedAction = _inputBuffer.Peek();
                if (ChangeState(bufferedAction))
                {
                    _inputBuffer.Dequeue();
                }
            }
        }

        /// <summary>
        /// 清空输入缓冲
        /// </summary>
        public void ClearInputBuffer()
        {
            _inputBuffer.Clear();
        }

        /// <summary>
        /// 获取指定动作的冷却剩余时间
        /// </summary>
        public float GetCooldownRemaining(ActionType actionType)
        {
            return _cooldownTimers.ContainsKey(actionType) ? _cooldownTimers[actionType] : 0f;
        }

        /// <summary>
        /// 检查动作是否在冷却中
        /// </summary>
        public bool IsInCooldown(ActionType actionType)
        {
            return GetCooldownRemaining(actionType) > 0f;
        }

        /// <summary>
        /// 获取指定类型的状态实例
        /// </summary>
        public T GetState<T>(ActionType actionType) where T : ActionStateBase
        {
            if (_states.TryGetValue(actionType, out var state))
            {
                return state as T;
            }
            return null;
        }
    }
}
