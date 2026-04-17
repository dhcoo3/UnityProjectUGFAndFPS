using Builtin.Scripts.Game;
using GameFramework;
using HotAssets.Scripts.Common;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;
using HotAssets.Scripts.UI.Module.FightShare;
using UnityEngine;

namespace HotAssets.Scripts.Bridge
{
    /// <summary>
    /// 战斗层向UI层通信
    /// </summary>
    public class GamePlayToUIBridge:Singleton<GamePlayToUIBridge>
    {
        /// <summary>
        /// 通知UI创建角色。
        /// </summary>
        public void CreateRole(RoleUnit roleUnit)
        { 
            AppEntry.Event.Fire(this, FightCreateRoleEventArgs.Create(roleUnit));
        }
        
        /// <summary>
        /// 通知UI同步角色位置。
        /// </summary>
        public void UpdatePos(int id,Vector3 pos)
        {
            AppEntry.Event.Fire(this, FightUpdatePosEventArgs.Create(id, pos));
        }
        
        /// <summary>
        /// 通知UI刷新角色血量。
        /// </summary>
        public void UpdateHp(RoleUnit roleUnit)
        {
            AppEntry.Event.Fire(this, FightUpdateHpEventArgs.Create(roleUnit));
        }
        
        /// <summary>
        /// 通知UI角色死亡。
        /// </summary>
        public void RoleDie(RoleUnit roleUnit)
        {
            AppEntry.Event.Fire(this, FightRoleDieEventArgs.Create(roleUnit));
        }
        
        /// <summary>
        /// 通知UI弹出伤害数字。
        /// </summary>
        public void PopUpNumberOnCharacter(RoleUnit roleUnit,int dmageVal,bool isHeal)
        {
            AppEntry.Event.Fire(this, FightPopUpNumberEventArgs.Create(roleUnit.Behaviour.Position, dmageVal, isHeal));
        }
    }
}
