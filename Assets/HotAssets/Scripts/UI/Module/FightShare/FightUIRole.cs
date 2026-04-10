using GameFramework;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;

namespace HotAssets.Scripts.UI.Module.FightShare
{
    public class FightUIRole:IReference
    {
        public RoleUnit RoleUnit;
        public int Id => RoleUnit.RoleId;
        
        public FightUIRoleHud Hud;

        public static FightUIRole Create(RoleUnit roleUnit)
        {
            FightUIRole role = ReferencePool.Acquire<FightUIRole>();
            role.RoleUnit = roleUnit;
            return role;
        }
        
        public void Clear()
        {
        
        }
    }
}