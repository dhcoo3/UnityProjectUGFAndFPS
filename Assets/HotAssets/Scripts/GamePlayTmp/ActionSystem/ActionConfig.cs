using System;

namespace GamePlay.ActionSystem
{
    /// <summary>
    /// 动作类型枚举（银河恶魔城版）
    /// </summary>
    public enum ActionType
    {
        None = 0,
        Idle = 1,           // 待机
        Run = 2,            // 移动
        Jump = 3,           // 跳跃
        Fall = 4,           // 下落
        Attack = 5,         // 普通攻击
        Skill = 6,          // 技能（解锁能力）
        Hit = 7,            // 受击
        Crouch = 8,         // 下蹲
        Dodge = 9,          // 翻滚
        Dash = 10,          // 冲刺
        Dead = 11,          // 死亡
        WallSlide = 12,     // 贴墙下滑
        WallJump = 13,      // 蹬墙跳
        AirAttack = 14,     // 空中下砸攻击
    }

    /// <summary>
    /// 攻击阶段枚举（简化版，仅前摇+判定）
    /// </summary>
    public enum AttackPhase
    {
        None,
        Startup,    // 前摇（不可打断）
        Active,     // 判定帧（伤害生效）
    }

    /// <summary>
    /// 动作配置数据（银河恶魔城简化版）
    /// </summary>
    [Serializable]
    public class ActionConfig
    {
        /// <summary>动作类型</summary>
        public ActionType ActionType;

        /// <summary>动作优先级（0=普通, 1=动作, 2=受击, 3=死亡）</summary>
        public int Priority;

        /// <summary>是否可被打断</summary>
        public bool CanBeInterrupted;

        /// <summary>动作持续时间（秒）</summary>
        public float Duration;

        /// <summary>冷却时间（秒）</summary>
        public float CooldownTime;

        /// <summary>是否允许输入缓冲</summary>
        public bool AllowInputBuffer;

        /// <summary>可取消时间点（动作进行到该时间后可被取消，-1表示不可取消）</summary>
        public float CancelableTime;

        /// <summary>攻击前摇时间（秒，前摇结束后进入判定帧）</summary>
        public float AttackDelay;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ActionConfig(ActionType actionType, int priority, bool canBeInterrupted,
            float duration, float cooldownTime = 0f,
            bool allowInputBuffer = true, float cancelableTime = -1f,
            float attackDelay = 0f)
        {
            ActionType = actionType;
            Priority = priority;
            CanBeInterrupted = canBeInterrupted;
            Duration = duration;
            CooldownTime = cooldownTime;
            AllowInputBuffer = allowInputBuffer;
            CancelableTime = cancelableTime;
            AttackDelay = attackDelay;
        }
    }
}
