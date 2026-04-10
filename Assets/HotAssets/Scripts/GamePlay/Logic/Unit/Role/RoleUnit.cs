using GameFramework;
using HotAssets.Scripts.GamePlay.Logic.Buff;
using HotAssets.Scripts.GamePlay.Logic.Common;
using HotAssets.Scripts.GamePlay.Logic.Damage;
using HotAssets.Scripts.GamePlay.Logic.GameInput;
using HotAssets.Scripts.GamePlay.Logic.Role;
using HotAssets.Scripts.GamePlay.Logic.Skill;
using HotAssets.Scripts.GamePlay.Logic.Unit.Component;
using HotAssets.Scripts.GamePlay.Logic.Unit.Core;
using UnityEngine;

namespace HotAssets.Scripts.GamePlay.Logic.Unit.Role
{
    public class RoleUnit:IUnit
    {
        /// <summary>冲刺技能ID（对应配置表 SkillDef 100010）</summary>
        private const int SkillIdDash = 100010;

        /// <summary>二段跳技能ID（对应配置表 SkillDef 100011）</summary>
        private const int SkillIdDoubleJump = 100011;
        #region interface
        public IBrian Brian => _brian;
        public IBehaviour Behaviour =>_behaviour;
        public cfg.Game.MoveType MoveType => cfg.Game.MoveType.Ground;
        public fix BodyRadius => _data.Prop.BodyRadius;
        public bool SmoothMove => true;
        public bool IgnoreBorder => false;
        
        #endregion
        
        private RoleBrian _brian;
        public RoleBrian RoleBrian => _brian;
        
        private RoleBehaviour _behaviour;
        public RoleBehaviour RoleBehaviour => _behaviour;
        
        private RoleData _data;
        public RoleData Data => _data;
        public int RoleId => _data.RoleId;

        ///<summary>
        //角色最终的可操作性状态
        ///</summary>
        public RoleState RoleState;
        
        ///<summary>
        ///GameTimeline专享的ChaControlState
        ///</summary>
        public RoleState TimelineControlState = new RoleState(true,true,true);
       
        
        public bool HasEntity = false;

        public static RoleUnit Create(RoleData roleData)
        {
            RoleUnit roleUnit = ReferencePool.Acquire<RoleUnit>();
            roleUnit.RoleState = new RoleState(true,true,true);
            roleUnit.TimelineControlState = new RoleState(true,true,true);
            roleUnit._data = roleData;
            roleUnit._brian = RoleBrian.CreateRoleBrian(roleUnit);
            roleUnit._behaviour = RoleBehaviour.CreateRoleBehaviour(roleUnit);
            return roleUnit;
        }

        public void LogicUpdate(fix deltaTime)
        {
            if(!HasEntity) return;
            _brian?.LogicUpdate(deltaTime);
            _behaviour?.LogicUpdate(deltaTime);
        }

        public void Clear()
        {
            ReferencePool.Release(_data);
            ReferencePool.Release(_brian);
            ReferencePool.Release(_behaviour);
            _data = null;
            _brian = null;
            _behaviour = null;
        }

        /// <summary>
        /// 处理一帧输入：移动、跳跃（含二段跳）、冲刺、瞄准、技能快捷键
        /// </summary>
        public void SetOperator(InputObj inputObj)
        {
            OrderMove(new fix3(inputObj.Horizontal, fix.Zero, fix.Zero));
            if (inputObj.Jump) OrderJump();
            if (inputObj.Dash) OrderDash();
            OrderAim(inputObj.AimDir);

            if (inputObj.Key != KeyCode.None)
            {
                if (inputObj.Key == KeyCode.K)
                {
                    UseSkill(100001);
                }
                else if (inputObj.Key == KeyCode.L)
                {
                    UseSkill(100002);
                }
                else if (inputObj.Key == KeyCode.J)
                {
                    UseSkill(100004);
                }
            }
        }
        
        ///<summary>
        ///命令移动（横版：只取X轴，Y轴由重力/跳跃管理）
        ///<param name="move">移动力</param>
        ///</summary>
        public void OrderMove(fix3 move){
            if(RoleState.IsDeath) return;
            _brian.MoveOrder.x = move.x * _data.MoveSpeed;
        }

        ///<summary>
        ///命令跳跃：地面/贴墙走普通跳跃；空中且未用过二段跳时走技能系统消耗耐力，
        ///竖向速度由技能 Timeline 的 ApplyForceToCaster 节点驱动
        ///</summary>
        public void OrderJump(){
            if(RoleState.IsDeath) return;
            UnitMove unitMove = _behaviour.UnitMove;
            // 空中且未贴墙：尝试二段跳（通过技能系统扣耐力/检查冷却）
            if (!unitMove.IsGrounded && !RoleState.IsOnWall)
            {
                if (RoleState.JumpCount < 1)
                {
                    _behaviour.UseSkill(SkillIdDoubleJump);
                }
                return;
            }
            _brian.OrderJump();
        }

        ///<summary>
        ///命令冲刺：通过技能系统检查耐力/冷却并扣费，推力由技能 Timeline 的 ApplyForceToCaster 节点驱动
        ///</summary>
        public void OrderDash()
        {
            if (RoleState.IsDeath) return;
            _behaviour.UseSkill(SkillIdDash);
        }

        ///<summary>
        ///命令瞄准方向
        ///</summary>
        public void OrderAim(GamePlayDefine.EAimDirection dir){
            if(RoleState.IsDeath) return;
            _brian.OrderAim(dir);
        }
        
        ///<summary>
        ///命令旋转到多少度
        ///<param name="degree">旋转目标</param>
        ///</summary>
        public void OrderRotateTo(fix degree){
            if(RoleState.IsDeath) return;
            _brian.RotateToOrder = degree;
        }
        
        ///<summary>
        ///学习某个技能
        ///<param name="skillModel">技能的模板</param>
        ///<param name="level">技能等级</param>
        ///</summary>
        public void LearnSkill(SkillModel skillModel, int level = 1)
        {
            if(RoleState.IsDeath) return;
            
            _data.Skills.Add(new SkillObj(skillModel, level));
            
            if (skillModel.buff != null)
            {
                for (int i = 0; i < skillModel.buff.Length; i++){
                    AddBuffInfo abi = skillModel.buff[i];
                    abi.permanent = true;
                    abi.duration = 10;
                    abi.durationSetTo = true;
                    AddBuff(abi);
                }
            }
        }

        public void UseSkill(int id)
        {
            if(RoleState.IsDeath) return;
            _behaviour.UseSkill(id);
        }

        public void AddBuff(AddBuffInfo buff)
        {
            if(RoleState.IsDeath) return;
            _brian.AddBuff(buff);
        }
        
        ///<summary>
        ///判断这个角色是否会被这个damageInfo所杀
        ///<param name="dInfo">要判断的damageInfo</param>
        ///<return>如果是true代表角色可能会被这次伤害所杀</return>
        ///</summary>
        public bool CanBeKilledByDamageInfo(DamageInfo damageInfo){
            if (RoleState.ImmuneTime > 0 || damageInfo.isHeal() == true)
            {
                return false;
            }
            
            int dValue = damageInfo.DamageValue(false);
            
            return dValue >= _data.Resource.hp;
        }

        public void ModResource(ChaResource value)
        {
            _behaviour.ModResource(value);
        }

        public bool IsDeath()
        {
            return RoleState.IsDeath;
        }
    }
}