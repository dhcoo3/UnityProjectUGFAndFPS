using System;
using System.Collections.Generic;
using AAAGame.ScriptsHotfix.GamePlay.Logic.Map;
using Builtin.Scripts.Game;
using cfg.Entity;
using GameFramework;
using HotAssets.Scripts.Bridge;
using HotAssets.Scripts.Common.Event;
using HotAssets.Scripts.Extension;
using HotAssets.Scripts.GamePlay.Logic.Aoe;
using HotAssets.Scripts.GamePlay.Logic.Bullet;
using HotAssets.Scripts.GamePlay.Logic.Common;
using HotAssets.Scripts.GamePlay.Logic.GameInput;
using HotAssets.Scripts.GamePlay.Logic.Map;
using HotAssets.Scripts.GamePlay.Logic.Player;
using HotAssets.Scripts.GamePlay.Logic.ProxyManager;
using HotAssets.Scripts.GamePlay.Logic.Role;
using HotAssets.Scripts.GamePlay.Logic.Skill;
using HotAssets.Scripts.GamePlay.Logic.Unit.Aoe;
using HotAssets.Scripts.GamePlay.Logic.Unit.Bullet;
using HotAssets.Scripts.GamePlay.Logic.Unit.Core;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;
using HotAssets.Scripts.UI;
using Unity.Mathematics;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GamePlay.Logic.Unit
{
    /// <summary>
    /// 游戏单位处理
    /// </summary>
    public class UnitProxy:GameProxy
    {
        private int _id = GamePlayDefine.CreateId.Unit;

        private PlayerProxy _playerProxy;

        // 查找用（按id）
        public Dictionary<string, RoleUnit> PlayerRole { get; } = new Dictionary<string, RoleUnit>();
        public Dictionary<int, IUnit> Units { get; } = new Dictionary<int, IUnit>();
        public Dictionary<int, IUnit> RoleUnits { get; } = new Dictionary<int, IUnit>();
        public Dictionary<int, RoleUnit> Heros { get; } = new Dictionary<int, RoleUnit>();
        public Dictionary<int, RoleUnit> Monsters { get; } = new Dictionary<int, RoleUnit>();
        public Dictionary<int, BulletUnit> Bullets { get; } = new Dictionary<int, BulletUnit>();
        public Dictionary<int, AoeUnit> Aoes { get; } = new Dictionary<int, AoeUnit>();

        // 遍历用（无 GC alloc）
        private readonly List<IUnit> _unitList = new List<IUnit>();

        private readonly List<int> _waitRecycleIds = new List<int>();

        public RoleUnit MainRole;
        
        private SkillProxy _skillProxy;

        private MapProxy _mapProxy;

        private TbHero _tbHero;
        private TbMonster _tbMonster;
        
        public override async void Initialize()
        {
            try
            {
                _tbHero = await AppEntry.DataTable.GetDataTableLuBan<TbHero>(cfg.Tables.entity_tbhero);
                _tbMonster = await AppEntry.DataTable.GetDataTableLuBan<TbMonster>(cfg.Tables.entity_tbmonster);
            }
            catch (Exception e)
            {
                Log.Error("Initialize error = {0}",e.Message);
            }
            
            _playerProxy = GetProxy<PlayerProxy>();
            _skillProxy = GetProxy<SkillProxy>();
            _mapProxy = GetProxy<MapProxy>();
            Subscribe(GamePlayEvent.EFightLoadingFinish,FightLoadingFinish);
        }

        public override void Clear()
        { 
            base.Clear();
        }
        
        public void InitRole()
        {
            Log.Info("InitRole");
            foreach (var playerData in _playerProxy.Players)
            {
                foreach (var heroId in playerData.HeroIds)
                {
                    CreateHeroRoleUnit(heroId,playerData);
                }
            }
            
            Fire(GamePlayEvent.ECreateRoleAll);
            Fire(GamePlayEvent.ERenderAllRole);
        }
        
        public RoleUnit CreateHeroRoleUnit(int heroId,PlayerData playerData)
        {
            Hero data = _tbHero.GetOrDefault(heroId);
            if (data == null)
            {
                Log.Warning("CreateHeroRoleUnit Null {0}",heroId);
                return null;
            }

            int id = int.Parse(playerData.Id);
            RoleHero roleData = RoleHero.Create(playerData,id,data);
            int2 pos = _mapProxy.MapInfo.GetGridPosByMeter(playerData.Pos,playerData.Pos+10);
            roleData.InitPosition = new fix3(pos.x,pos.y,0);
            RoleUnit roleUnit = RoleUnit.Create(roleData);
            
            Units.Add(id, roleUnit);
            Heros.Add(id, roleUnit);
            RoleUnits.Add(id, roleUnit);
            PlayerRole.Add(playerData.Id, roleUnit);
            _unitList.Add(roleUnit);
            GamePlayToUIBridge.Instance.CreateRole(roleUnit);
            return roleUnit;
        }
        
        public RoleUnit CreateMonsterRoleUnit(int monsterId)
        {
            cfg.Entity.Monster data = _tbMonster.GetOrDefault(monsterId);
            if (data == null)
            {
                Log.Info("CreateMonsterRoleUnit Null {0}",monsterId);
                return null;
            }

            _id++;
            RoleMonster roleData = RoleMonster.Create(_id,data);
            RectInt rect = new RectInt();
            rect.width = 9;
            rect.height = 10;
            fix3 pos = _mapProxy.MapInfo.GetRandomPosForCharacter(rect, roleData.Prop.BodyRadius);
            roleData.InitPosition = pos;

            RoleUnit roleUnit = RoleUnit.Create(roleData);
            Units.Add(_id, roleUnit);
            Monsters.Add(_id, roleUnit);
            RoleUnits.Add(_id, roleUnit);
            _unitList.Add(roleUnit);
            GamePlayToUIBridge.Instance.CreateRole(roleUnit);
            return roleUnit;
        }

        /// <summary>
        /// 在指定坐标生成怪物，巡逻参数由 MonsterSpawnerProxy 传入
        /// </summary>
        /// <param name="monsterId">怪物配置id</param>
        /// <param name="spawnPos">生成坐标（绝对）</param>
        /// <param name="patrolCenterX">巡逻中心X（绝对坐标，已根据 PatrolMode 换算）</param>
        /// <param name="patrolHalfRange">巡逻半程</param>
        public RoleUnit CreateMonsterRoleUnit(int monsterId, fix3 spawnPos, fix patrolCenterX, fix patrolHalfRange)
        {
            cfg.Entity.Monster data = _tbMonster.GetOrDefault(monsterId);
            if (data == null)
            {
                Log.Info("CreateMonsterRoleUnit Null {0}", monsterId);
                return null;
            }

            _id++;
            RoleMonster roleData = RoleMonster.Create(_id, data, patrolCenterX, patrolHalfRange);
            roleData.InitPosition = spawnPos;

            RoleUnit roleUnit = RoleUnit.Create(roleData);
            Units.Add(_id, roleUnit);
            Monsters.Add(_id, roleUnit);
            RoleUnits.Add(_id, roleUnit);
            _unitList.Add(roleUnit);

            for (int i = 0; i < data.SkillList.Count; i++)
            {
                roleUnit.LearnSkill(_skillProxy.GetSkillModel(data.SkillList[i]));
            }
           
            GamePlayToUIBridge.Instance.CreateRole(roleUnit);
            
            return roleUnit;
        }
        
        public BulletUnit CreateBulletUnit(BulletData bulletData)
        {
            _id++;
            bulletData.BulletId = _id;
            BulletUnit bulletUnit = BulletUnit.Create(bulletData);
            Units.Add(_id, bulletUnit);
            Bullets.Add(_id, bulletUnit);
            _unitList.Add(bulletUnit);
            Fire(GamePlayEvent.ERenderBullet,bulletUnit);
            return bulletUnit;
        }

        public AoeUnit CreateAoeUnit(AoeData aoeData)
        {
            _id++;
            aoeData.AoeId = _id;
            AoeUnit aoeUnit = AoeUnit.Create(aoeData);
            Units.Add(_id, aoeUnit);
            Aoes.Add(_id, aoeUnit);
            _unitList.Add(aoeUnit);
            Fire(GamePlayEvent.ERenderAoe,aoeUnit);
            return aoeUnit;
        }

        public IUnit GetRoleUnit(int id)
        {
            return Units.GetValueOrDefault(id);
        }

        public void RemoveRoleUnit(int id)
        {
            if (Units.ContainsKey(id))
            {
                _waitRecycleIds.Add(id);
            }
        }

        private void CheckDestroy()
        {
            if (_waitRecycleIds.Count == 0) return;

            for (int i = 0; i < _waitRecycleIds.Count; i++)
            {
                int id = _waitRecycleIds[i];
                if (Units.TryGetValue(id, out IUnit unit))
                {
                    Units.Remove(id);
                    _unitList.Remove(unit);

                    if (unit is RoleUnit roleUnit)
                    {
                        Heros.Remove(id);
                        Monsters.Remove(id);
                        RoleUnits.Remove(id);
                        GamePlayToUIBridge.Instance.RoleDie(roleUnit);
                        FireNow(GamePlayEvent.EStopRenderRole, id);
                        ReferencePool.Release(roleUnit);
                    }
                    else if (unit is BulletUnit bulletUnit)
                    {
                        Bullets.Remove(id);
                        FireNow(GamePlayEvent.EStopRenderBullet, id);
                        ReferencePool.Release(bulletUnit);
                    }
                    else if (unit is AoeUnit aoeUnit)
                    {
                        Aoes.Remove(id);
                        FireNow(GamePlayEvent.EStopRenderAoe, id);
                        ReferencePool.Release(aoeUnit);
                    }
                }
            }

            _waitRecycleIds.Clear();
        }

        public override void LogicUpdate(fix deltaTime)
        {
            CheckDestroy();

            for (int i = 0; i < _unitList.Count; i++)
            {
                _unitList[i].LogicUpdate(deltaTime);
            }
        }

        /// <summary>
        /// 获取所有玩家角色列表（用于多人游戏）
        /// </summary>
        public List<RoleUnit> GetAllPlayerRoles()
        {
            List<RoleUnit> allPlayers = new List<RoleUnit>();
            foreach (var kvp in PlayerRole)
            {
                if (kvp.Value != null)
                {
                    allPlayers.Add(kvp.Value);
                }
            }
            return allPlayers;
        }

        public void OperatorMainRole(InputObj inputObj)
        {
            if (PlayerRole.TryGetValue(inputObj.PlayerId, out RoleUnit playerUnit))
            {
                //Log.Info("inputObj.PlayerId:{0} {1} {2} {3}",GameExtension.UserId,playerUnit.RoleId,inputObj.PlayerId,playerUnit.Data.OperateId);
                playerUnit.SetOperator(inputObj);
            }
        }

        private void FightLoadingFinish(object sender, GameEvent e)
        {
            //加载完 聚焦自己的角色
            if (PlayerRole.TryGetValue(GameExtension.UserId, out RoleUnit playerUnit))
            {
                MainRole = playerUnit;
                
                fix3 pos = new fix3(1, 5, 0);
                MainRole.RoleBehaviour.ApplyPosition(pos);
                
                //测试技能
                MainRole.LearnSkill(_skillProxy.GetSkillModel(100001));
                MainRole.LearnSkill(_skillProxy.GetSkillModel(100002));
                MainRole.LearnSkill(_skillProxy.GetSkillModel(100004));
                // 冲刺 & 二段跳
                MainRole.LearnSkill(_skillProxy.GetSkillModel(100010));
                MainRole.LearnSkill(_skillProxy.GetSkillModel(100011));
                
                Fire(GamePlayEvent.ECameraFocus,MainRole);
            }
        }
    }
}