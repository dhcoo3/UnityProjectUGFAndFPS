using UnityEngine;

namespace GamePlay.ActionSystem
{
    /// <summary>
    /// 攻击判定盒类型（银河恶魔城简化版）
    /// </summary>
    public enum HitBoxType
    {
        Attack,     // 攻击盒（我打别人）
        Hurt,       // 受击盒（别人打我）
    }

    /// <summary>
    /// 攻击判定数据
    /// </summary>
    [System.Serializable]
    public class HitBoxData
    {
        /// <summary>判定盒类型</summary>
        public HitBoxType Type;

        /// <summary>判定盒偏移位置</summary>
        public Vector2 Offset;

        /// <summary>判定盒大小</summary>
        public Vector2 Size;

        /// <summary>是否激活</summary>
        public bool IsActive;

        public HitBoxData(HitBoxType type, Vector2 offset, Vector2 size)
        {
            Type = type;
            Offset = offset;
            Size = size;
            IsActive = false;
        }

        /// <summary>
        /// 获取世界坐标下的判定盒范围
        /// </summary>
        public Rect GetWorldRect(Vector2 ownerPosition, bool facingRight)
        {
            Vector2 worldOffset = Offset;
            if (!facingRight)
            {
                worldOffset.x = -worldOffset.x;
            }

            Vector2 worldPos = ownerPosition + worldOffset;
            return new Rect(worldPos.x - Size.x * 0.5f, worldPos.y - Size.y * 0.5f, Size.x, Size.y);
        }
    }

    /// <summary>
    /// 攻击数据（银河恶魔城简化版）
    /// </summary>
    [System.Serializable]
    public class AttackData
    {
        /// <summary>伤害值</summary>
        public float Damage;

        /// <summary>击退力度</summary>
        public Vector2 KnockbackForce;

        /// <summary>攻击盒数据</summary>
        public HitBoxData AttackBox;

        public AttackData(float damage, Vector2 knockbackForce)
        {
            Damage = damage;
            KnockbackForce = knockbackForce;
        }
    }

    /// <summary>
    /// 受击数据（银河恶魔城简化版）
    /// </summary>
    [System.Serializable]
    public class HitData
    {
        /// <summary>伤害值</summary>
        public float Damage;

        /// <summary>击退方向</summary>
        public Vector2 KnockbackDirection;

        /// <summary>击退力度</summary>
        public float KnockbackForce;

        /// <summary>攻击来源</summary>
        public GameObject Attacker;
    }
}
