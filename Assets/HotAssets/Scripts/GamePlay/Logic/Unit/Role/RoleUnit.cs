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
    /// <summary>
    /// 角色单位的核心运行入口，负责串联角色数据、逻辑脑、行为执行以及输入转命令。
    /// </summary>
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

        /// <summary>
        /// 角色当前最终生效的状态集合，综合控制死亡、落地、贴墙、免疫等运行态。
        /// </summary>
        public RoleState RoleState;
        
        /// <summary>
        /// Timeline 专用的控制状态，用于过场或技能时间轴临时接管角色控制。
        /// </summary>
        public RoleState TimelineControlState = new RoleState(true,true,true);
       
        /// <summary>
        /// 是否已经绑定场景实体；未绑定时不推进逻辑帧，避免空引用或状态提前运转。
        /// </summary>
        public bool HasEntity = false;

        /// <summary>
        /// 创建并初始化一个角色单位实例，同时挂接角色数据、逻辑脑与行为控制器。
        /// </summary>
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

        /// <summary>
        /// 角色逻辑帧更新入口。
        /// 先做输入与物理相关计算，再执行位移，最后同步动画，确保动画读取到稳定结果。
        /// </summary>
        public void LogicUpdate(fix deltaTime)
        {
            if(!HasEntity) return;
            _brian?.LogicUpdate(deltaTime);      // 物理计算：CalcMove、CalcRotation
            _behaviour?.LogicUpdate(deltaTime);  // 移动执行：UnitMove 写入 IsGrounded
            _brian?.UpdateAnimation();           // 动画更新：IsGrounded 已完全稳定
        }

        /// <summary>
        /// 回收角色运行期对象，释放引用池中的数据与组件。
        /// </summary>
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
        /// 处理一帧输入，将输入层数据转换成角色命令。
        /// 包含移动、跳跃（含二段跳）、冲刺、瞄准以及测试用技能快捷键。
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
        
        /// <summary>
        /// 下发移动命令。
        /// 横版角色仅消费 X 轴输入，Y 轴位移由重力、跳跃或技能驱动。
        /// </summary>
        /// <param name="move">原始移动输入向量。</param>
        public void OrderMove(fix3 move){
            if(RoleState.IsDeath) return;
            _brian.MoveOrder.x = move.x * _data.MoveSpeed;
        }

        /// <summary>
        /// 下发跳跃命令。
        /// 地面或贴墙时走普通跳跃；空中且未贴墙时，尝试通过技能系统执行二段跳。
        /// 二段跳的资源消耗、冷却与竖向推力由技能时间轴负责。
        /// </summary>
        public void OrderJump(){
            if(RoleState.IsDeath) return;
            UnitMove unitMove = _behaviour.UnitMove;
            // 同时检查地形落地和平台落地状态
            // UnitMove.IsGrounded：地形碰撞检测（需要主动向下移动）
            // RoleState.IsGrounded：平台检测（站立在移动平台上）
            bool isActuallyGrounded = unitMove.IsGrounded || RoleState.IsGrounded;

            // 空中且未贴墙：尝试二段跳（通过技能系统扣耐力/检查冷却）
            if (!isActuallyGrounded && !RoleState.IsOnWall)
            {
                if (RoleState.JumpCount < 1)
                {
                    _behaviour.UseSkill(SkillIdDoubleJump);
                }
                return;
            }
            _brian.OrderJump();
        }

        /// <summary>
        /// 下发冲刺命令。
        /// 冲刺是否合法、是否扣费以及实际施加的推力，都由技能系统与时间轴处理。
        /// </summary>
        public void OrderDash()
        {
            if (RoleState.IsDeath) return;
            _behaviour.UseSkill(SkillIdDash);
        }

        /// <summary>
        /// 设置角色当前瞄准方向，供攻击、朝向或技能选择使用。
        /// </summary>
        public void OrderAim(GamePlayDefine.EAimDirection dir){
            if(RoleState.IsDeath) return;
            _brian.OrderAim(dir);
        }
        
        /// <summary>
        /// 设置角色旋转目标角度。
        /// </summary>
        /// <param name="degree">目标朝向角度。</param>
        public void OrderRotateTo(fix degree){
            if(RoleState.IsDeath) return;
            _brian.RotateToOrder = degree;
        }
        
        /// <summary>
        /// 学习一个技能，并在需要时为角色附加该技能自带的常驻 Buff。
        /// </summary>
        /// <param name="skillModel">技能模板数据。</param>
        /// <param name="level">技能等级。</param>
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

        /// <summary>
        /// 按技能 ID 主动施放技能。
        /// </summary>
        public void UseSkill(int id)
        {
            if(RoleState.IsDeath) return;
            _behaviour.UseSkill(id);
        }

        /// <summary>
        /// 为角色添加一个 Buff。
        /// </summary>
        public void AddBuff(AddBuffInfo buff)
        {
            if(RoleState.IsDeath) return;
            _brian.AddBuff(buff);
        }
        
        /// <summary>
        /// 判断当前伤害是否足以击杀角色。
        /// 若角色处于免疫期，或该伤害本质上是治疗，则直接视为不会击杀。
        /// </summary>
        /// <param name="damageInfo">待判定的伤害信息。</param>
        /// <returns>返回 <c>true</c> 表示这次伤害值足以将角色生命降至 0 或以下。</returns>
        public bool CanBeKilledByDamageInfo(DamageInfo damageInfo){
            if (RoleState.ImmuneTime > 0 || damageInfo.isHeal() == true)
            {
                return false;
            }
            
            int dValue = damageInfo.DamageValue(false);
            
            return dValue >= _data.Resource.hp;
        }

        /// <summary>
        /// 修改角色资源值，例如生命、体力或其他战斗资源。
        /// </summary>
        public void ModResource(ChaResource value)
        {
            _behaviour.ModResource(value);
        }

        /// <summary>
        /// 返回角色是否已经死亡。
        /// </summary>
        public bool IsDeath()
        {
            return RoleState.IsDeath;
        }
    }
}
