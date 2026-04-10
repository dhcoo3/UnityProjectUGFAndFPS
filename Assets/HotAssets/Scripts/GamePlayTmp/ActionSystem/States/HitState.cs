using UnityEngine;

namespace GamePlay.ActionSystem.States
{
    /// <summary>
    /// 受击状态（银河恶魔城版，简化：无浮空/倒地/Stun）
    /// </summary>
    public class HitState : ActionStateBase
    {
        /// <summary>
        /// 受击数据
        /// </summary>
        private HitData _hitData;

        /// <summary>
        /// 击退速度衰减系数
        /// </summary>
        private const float KnockbackDecay = 5f;

        /// <summary>
        /// 当前击退速度
        /// </summary>
        private Vector2 _currentKnockbackVelocity;

        /// <summary>
        /// 进入受击状态
        /// </summary>
        public override void Enter()
        {
            base.Enter();

            if (_hitData != null)
            {
                // 应用击退
                _currentKnockbackVelocity = _hitData.KnockbackDirection * _hitData.KnockbackForce;

                UnityGameFramework.Runtime.Log.Info($"受击 - 伤害: {_hitData.Damage}, 击退: {_hitData.KnockbackForce}");

                // TODO: 播放受击动画
                // TODO: 播放受击音效
                // TODO: 显示受击特效
            }
        }

        /// <summary>
        /// 更新受击状态
        /// </summary>
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            // 击退速度衰减
            _currentKnockbackVelocity = Vector2.Lerp(_currentKnockbackVelocity, Vector2.zero, KnockbackDecay * deltaTime);

            // TODO: 应用击退位移
        }

        /// <summary>
        /// 退出受击状态
        /// </summary>
        public override void Exit()
        {
            base.Exit();
            _currentKnockbackVelocity = Vector2.zero;
        }

        /// <summary>
        /// 受击完成回调（银河城简化：直接回Idle）
        /// </summary>
        protected override void OnActionComplete()
        {
            StateMachine.ChangeState(ActionType.Idle);
        }

        /// <summary>
        /// 设置受击数据
        /// </summary>
        public void SetHitData(HitData hitData)
        {
            _hitData = hitData;
        }

        /// <summary>
        /// 获取当前击退速度
        /// </summary>
        public Vector2 GetKnockbackVelocity()
        {
            return _currentKnockbackVelocity;
        }

        /// <summary>
        /// 状态转换检查
        /// </summary>
        public override bool CanTransitionTo(ActionStateBase targetState)
        {
            // 受击状态仅允许死亡打断，完成后回待机
            return targetState.Config.ActionType == ActionType.Dead
                || targetState.Config.ActionType == ActionType.Idle;
        }
    }
}
