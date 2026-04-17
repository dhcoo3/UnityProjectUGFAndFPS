using GameFramework;
using GameFramework.Event;
using HotAssets.Scripts.Common.Event;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;
using UnityEngine;

namespace HotAssets.Scripts.UI.Module.FightShare
{
    public sealed class FightCreateRoleEventArgs : GameEventArgs
    {
        public static readonly int EventId = ++GameEvent.UIEventId;

        public RoleUnit RoleUnit { get; private set; }

        public override int Id => EventId;

        public static FightCreateRoleEventArgs Create(RoleUnit roleUnit)
        {
            FightCreateRoleEventArgs eventArgs = ReferencePool.Acquire<FightCreateRoleEventArgs>();
            eventArgs.RoleUnit = roleUnit;
            return eventArgs;
        }

        public override void Clear()
        {
            RoleUnit = null;
        }
    }

    public sealed class FightUpdatePosEventArgs : GameEventArgs
    {
        public static readonly int EventId = ++GameEvent.UIEventId;

        public int RoleId { get; private set; }
        public Vector3 Position { get; private set; }

        public override int Id => EventId;

        public static FightUpdatePosEventArgs Create(int roleId, Vector3 position)
        {
            FightUpdatePosEventArgs eventArgs = ReferencePool.Acquire<FightUpdatePosEventArgs>();
            eventArgs.RoleId = roleId;
            eventArgs.Position = position;
            return eventArgs;
        }

        public override void Clear()
        {
            RoleId = 0;
            Position = default;
        }
    }

    public sealed class FightUpdateHpEventArgs : GameEventArgs
    {
        public static readonly int EventId = ++GameEvent.UIEventId;

        public RoleUnit RoleUnit { get; private set; }

        public override int Id => EventId;

        public static FightUpdateHpEventArgs Create(RoleUnit roleUnit)
        {
            FightUpdateHpEventArgs eventArgs = ReferencePool.Acquire<FightUpdateHpEventArgs>();
            eventArgs.RoleUnit = roleUnit;
            return eventArgs;
        }

        public override void Clear()
        {
            RoleUnit = null;
        }
    }

    public sealed class FightRoleDieEventArgs : GameEventArgs
    {
        public static readonly int EventId = ++GameEvent.UIEventId;

        public RoleUnit RoleUnit { get; private set; }

        public override int Id => EventId;

        public static FightRoleDieEventArgs Create(RoleUnit roleUnit)
        {
            FightRoleDieEventArgs eventArgs = ReferencePool.Acquire<FightRoleDieEventArgs>();
            eventArgs.RoleUnit = roleUnit;
            return eventArgs;
        }

        public override void Clear()
        {
            RoleUnit = null;
        }
    }

    public sealed class FightPopUpNumberEventArgs : GameEventArgs
    {
        public static readonly int EventId = ++GameEvent.UIEventId;

        public fix3 Position { get; private set; }
        public int DamageVal { get; private set; }
        public bool IsHeal { get; private set; }

        public override int Id => EventId;

        public static FightPopUpNumberEventArgs Create(fix3 position, int damageVal, bool isHeal)
        {
            FightPopUpNumberEventArgs eventArgs = ReferencePool.Acquire<FightPopUpNumberEventArgs>();
            eventArgs.Position = position;
            eventArgs.DamageVal = damageVal;
            eventArgs.IsHeal = isHeal;
            return eventArgs;
        }

        public override void Clear()
        {
            Position = default;
            DamageVal = 0;
            IsHeal = false;
        }
    }
}
