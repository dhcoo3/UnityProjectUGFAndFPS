using GameFramework;
using HotAssets.Scripts.Common;
using HotAssets.Scripts.Common.Event;
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
        private readonly AAAGameEventHelper _eventHelper = ReferencePool.Acquire<AAAGameEventHelper>();
      
        public void CreateRole(RoleUnit roleUnit)
        { 
            _eventHelper.Fire(FightShareConst.Event.ECreateRole,roleUnit);
        }
        
        public void UpdatePos(int id,Vector3 pos)
        {
            _eventHelper.Fire(FightShareConst.Event.EUpdatePos,id,pos);
        }
        
        public void UpdateHp(RoleUnit roleUnit)
        {
            _eventHelper.Fire(FightShareConst.Event.EUpdateHp,roleUnit);
        }
        
        public void RoleDie(RoleUnit roleUnit)
        {
            _eventHelper.Fire(FightShareConst.Event.ERoleDie,roleUnit);
        }
        
        public void PopUpNumberOnCharacter(RoleUnit roleUnit,int dmageVal,bool isHeal)
        {
            _eventHelper.Fire(FightShareConst.Event.EPopUpNumber,roleUnit.Behaviour.Position,dmageVal,isHeal);
        }
    }
}