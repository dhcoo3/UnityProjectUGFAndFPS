using System;
using System.Collections.Generic;
using Builtin.Scripts.Game;
using HotAssets.Scripts.Common;
using HotAssets.Scripts.Common.Event;
using HotAssets.Scripts.Extension;
using HotAssets.Scripts.GamePlay.Logic.Common;
using HotAssets.Scripts.GamePlay.Logic.ProxyManager;
using HotAssets.Scripts.GamePlay.Logic.Unit;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GamePlay.Logic.MonsterSpawner
{
    public class MonsterSpawnerProxy : GameProxy
    {
        private cfg.Spawner.TbMonsterSpawner _tbMonsterSpawner;

        private readonly List<SpawnerState> _spawnerStates = new List<SpawnerState>();

        private UnitProxy _unitProxy;

        /// <summary>
        /// 单个触发点的运行时状态
        /// </summary>
        private class SpawnerState
        {
            public cfg.Spawner.MonsterSpawner Config;

            /// <summary>是否已触发过生成（每个触发点只触发一次）</summary>
            public bool Triggered;

            /// <summary>该触发点当前存活的怪物id，用于死亡回调清理</summary>
            public readonly List<int> AliveMonsterIds = new List<int>();
        }

        public override async void Initialize()
        {
            _unitProxy = GetProxy<UnitProxy>();

            try
            {
                _tbMonsterSpawner = await AppEntry.DataTable.GetDataTableLuBan<cfg.Spawner.TbMonsterSpawner>(
                    cfg.Tables.spawner_tbmonsterspawner);

                foreach (cfg.Spawner.MonsterSpawner config in _tbMonsterSpawner.DataList)
                {
                    _spawnerStates.Add(new SpawnerState { Config = config });
                }
            }
            catch (Exception e)
            {
                Log.Error("MonsterSpawnerProxy Initialize error = {0}", e.Message);
            }

            Subscribe(GamePlayEvent.EFightLoadingFinish, OnFightLoadingFinish);
            Subscribe(GamePlayEvent.EStopRenderRole, OnRoleDie);
        }

        public override void Clear()
        {
            _spawnerStates.Clear();
            base.Clear();
        }

        public override void LogicUpdate(fix deltaTime)
        {
            /*// 检测任意玩家是否到达触发点（多人游戏支持）
            List<RoleUnit> allPlayers = _unitProxy.GetAllPlayerRoles();
            if (allPlayers == null || allPlayers.Count == 0) return;

            for (int i = 0; i < _spawnerStates.Count; i++)
            {
                SpawnerState state = _spawnerStates[i];
                if (state.Triggered) continue;

                fix triggerX = MathUtils.Convert(state.Config.TriggerX);

                // 只要有任意一个玩家到达触发点，就生成怪物
                bool anyPlayerTriggered = false;
                for (int j = 0; j < allPlayers.Count; j++)
                {
                    if (allPlayers[j] != null && allPlayers[j].RoleBehaviour.Position.x > triggerX)
                    {
                        anyPlayerTriggered = true;
                        break;
                    }
                }

                if (anyPlayerTriggered)
                {
                    state.Triggered = true;
                    SpawnMonsters(state);
                }
            }*/
        }

        /// <summary>
        /// 根据触发点配置批量生成怪物
        /// </summary>
        private void SpawnMonsters(SpawnerState state)
        {
            cfg.Spawner.MonsterSpawner config = state.Config;

            fix spawnX = MathUtils.Convert(config.SpawnX);
            fix spawnY = MathUtils.Convert(config.SpawnY);
            fix3 spawnPos = new fix3(spawnX, spawnY, fix.Zero);

            // PatrolMode 0=相对生成点，1=绝对坐标
            fix patrolCenterX = config.PatrolMode == 0
                ? spawnX + MathUtils.Convert(config.PatrolCenterX)
                : MathUtils.Convert(config.PatrolCenterX);

            fix patrolHalfRange = MathUtils.Convert(config.PatrolHalfRange);

            for (int i = 0; i < config.MaxCount; i++)
            {
                RoleUnit monster = _unitProxy.CreateMonsterRoleUnit(
                    config.MonsterId, spawnPos, patrolCenterX, patrolHalfRange);

                if (monster == null) continue;

                state.AliveMonsterIds.Add(monster.RoleId);
                Fire(GamePlayEvent.ERenderMonster, monster);
            }
        }

        private void OnFightLoadingFinish(object sender, GameEvent e)
        {
            // 战斗加载完毕，LogicUpdate 开始检测玩家位置
        }

        /// <summary>
        /// 角色死亡时从对应触发点的存活列表中移除
        /// </summary>
        private void OnRoleDie(object sender, GameEvent e)
        {
            int roleId = e.GetParam1<int>();

            for (int i = 0; i < _spawnerStates.Count; i++)
            {
                _spawnerStates[i].AliveMonsterIds.Remove(roleId);
            }
        }
    }
}
