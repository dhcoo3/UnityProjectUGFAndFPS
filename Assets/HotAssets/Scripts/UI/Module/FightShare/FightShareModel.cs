using System.Collections.Generic;
using cfg.Fight;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;
using HotAssets.Scripts.UI.Core;

namespace HotAssets.Scripts.UI.Module.FightShare
{
    #if ENABLE_OBFUZ
    [Obfuz.ObfuzIgnore]
    #endif
    public class FightShareModel : ModuleModel<FightShareModel>,IModel
    {
        private readonly Dictionary<int,FightUIRole> _fightUiRoles = new Dictionary<int,FightUIRole>();
        
        public Dictionary<int,FightUIRole> FightUiRoles => _fightUiRoles;

        public bool FightStart { get; set; } = false;
        public DungeonType FightDungeonType {get;set;}= DungeonType.Main;
        
        public void AddNewRole(RoleUnit roleData)
        {
            FightUIRole uiRole = FightUIRole.Create(roleData);
            _fightUiRoles.TryAdd(uiRole.Id,uiRole);
        }
    }
}
