using System.Collections.Generic;
using Builtin.Scripts.Game;
using HotAssets.Scripts.Common.Event;
using HotAssets.Scripts.Extension;
using HotAssets.Scripts.GamePlay.Logic.Common;
using HotAssets.Scripts.GamePlay.Logic.Fight;
using HotAssets.Scripts.GamePlay.Logic.Player;
using HotAssets.Scripts.GamePlay.Logic.Unit;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;
using HotAssets.Scripts.GamePlay.Render.RenderManager;
using TuanjieAI.Assistant.Schema;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GamePlay.Render.Role
{
    public class RoleRender:GameRender
    {
        private readonly Dictionary<int, RoleEntity> _roleViewDictionary = new Dictionary<int, RoleEntity>();
        
        private FightLoadingProxy _fightLoadingProxy;
        private UnitProxy _unitProxy;
        private PlayerProxy _currentPlayer;
        
        public override void Initialize()
        {
            _unitProxy = GetProxy<UnitProxy>();
            _fightLoadingProxy = GetProxy<FightLoadingProxy>();
            _currentPlayer = GetProxy<PlayerProxy>();
                
            EventHelper.SubscribeCommon(GamePlayEvent.ERenderAllRole,RenderAllRole);
            EventHelper.SubscribeCommon(GamePlayEvent.ERenderMonster,RenderMonster);
            EventHelper.SubscribeCommon(GamePlayEvent.EStopRenderRole,StopRenderRole);
        }

        public override void Clear()
        {
          
        }

        public override void LogicUpdate(fix deltaTime)
        {
            if(_roleViewDictionary.Count == 0) return;

            foreach (var data in _roleViewDictionary.Values)
            {
                data.LogicUpdate(deltaTime);
            }
            
            base.LogicUpdate(deltaTime);
        }

        private void RenderAllRole(object sender, GameEvent e)
        {
            foreach (var roleUnit in _unitProxy.Heros.Values)
            {
                EntityParams param = EntityParams.Create(roleUnit.Behaviour.Position);
                param.OnShowCallback += ShowRoleFinish;
                param.Unit = roleUnit;
                AppEntry.Entity.ShowEntity<RoleEntity>(roleUnit.Data.AssetPath,
                    GamePlayDefine.EntityGroup.RoleEntity,
                    GamePlayDefine.LoadPriority.Role, param);
            }
        }

        private void ShowRoleFinish(EntityLogic logic)
        {
            RoleEntity roleEntity = logic as RoleEntity;
            if (roleEntity == null)
            {
                Log.Error("实体加载失败");
                return;
            }
            
            _roleViewDictionary.Add(roleEntity.RoleId,roleEntity);
            _fightLoadingProxy.SetRoleProgress(_roleViewDictionary.Count/(float)_currentPlayer.Players.Count);
        }

        private void RenderMonster(object sender, GameEvent e)
        {
            RoleUnit roleUnit = e.GetParam1<RoleUnit>();
            if(roleUnit == null) return;
            
            EntityParams param = EntityParams.Create(roleUnit.Behaviour.Position);
            param.OnShowCallback += ShowRoleFinish;
            param.Unit = roleUnit;
            AppEntry.Entity.ShowEntity<RoleEntity>(roleUnit.Data.AssetPath,
                GamePlayDefine.EntityGroup.MonsterEntity,
                GamePlayDefine.LoadPriority.Role, param);
        }

        private void StopRenderRole(object sender, GameEvent e)
        {
            int id = e.GetParam1<int>();

            if (_roleViewDictionary.TryGetValue(id, out RoleEntity roleEntity))
            {
                AppEntry.Entity.HideEntity(roleEntity.Entity);
                _roleViewDictionary.Remove(id);
            }
        }
    }
}
