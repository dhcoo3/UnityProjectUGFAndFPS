using UnityEngine;

namespace GamePlay.ActionSystem
{
    /// <summary>
    /// 动画事件接收器
    /// </summary>
    public class AnimationEventReceiver : MonoBehaviour
    {
        /// <summary>
        /// 动作控制器引用
        /// </summary>
        private IActionController _actionController;

        private void Awake()
        {
            _actionController = GetComponentInParent<IActionController>();
            if (_actionController == null)
            {
                UnityGameFramework.Runtime.Log.Error("AnimationEventReceiver: 未找到 IActionController 组件");
            }
        }

        /// <summary>
        /// 攻击开始事件
        /// </summary>
        public void OnAttackStart()
        {
            _actionController?.OnAnimationEvent_AttackStart();
        }

        /// <summary>
        /// 攻击判定帧事件
        /// </summary>
        public void OnAttackHit()
        {
            _actionController?.OnAnimationEvent_AttackHit();
        }

        /// <summary>
        /// 攻击结束事件
        /// </summary>
        public void OnAttackEnd()
        {
            _actionController?.OnAnimationEvent_AttackEnd();
        }

        /// <summary>
        /// 脚步声事件
        /// </summary>
        public void OnFootstep()
        {
            _actionController?.OnAnimationEvent_Footstep();
        }

        /// <summary>
        /// 技能特效生成事件
        /// </summary>
        public void OnSpawnEffect(string effectName)
        {
            _actionController?.OnAnimationEvent_SpawnEffect(effectName);
        }

        /// <summary>
        /// 自定义事件
        /// </summary>
        public void OnCustomEvent(string eventName)
        {
            _actionController?.OnAnimationEvent_Custom(eventName);
        }
    }

    /// <summary>
    /// 动作控制器接口（银河恶魔城简化版，删除HitStop）
    /// </summary>
    public interface IActionController
    {
        void OnAnimationEvent_AttackStart();
        void OnAnimationEvent_AttackHit();
        void OnAnimationEvent_AttackEnd();
        void OnAnimationEvent_Footstep();
        void OnAnimationEvent_SpawnEffect(string effectName);
        void OnAnimationEvent_Custom(string eventName);
    }
}
