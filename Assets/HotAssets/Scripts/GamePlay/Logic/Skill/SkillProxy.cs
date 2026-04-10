using System;
using Builtin.Scripts.Game;
using cfg.Skill;
using HotAssets.Scripts.Common;
using HotAssets.Scripts.Extension;
using HotAssets.Scripts.GamePlay.Logic.Aoe;
using HotAssets.Scripts.GamePlay.Logic.Buff;
using HotAssets.Scripts.GamePlay.Logic.Bullet;
using HotAssets.Scripts.GamePlay.Logic.Bullet.BulletStrategy;
using HotAssets.Scripts.GamePlay.Logic.ProxyManager;
using HotAssets.Scripts.GamePlay.Logic.Role;
using HotAssets.Scripts.GamePlay.Logic.TimeLine;
using HotAssets.Scripts.GamePlay.Logic.Unit.Core;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;
using HotAssets.Scripts.UI;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GamePlay.Logic.Skill
{
    public class SkillProxy:GameProxy
    {
        private TbSkillDef _tbSkillDef;
        private TbTimeLineDef _tbTimeLineDef;
        private TbSkillBulletDef _tbSkillBulletDef;
        private TbBulletDef _tbBulletDef;
        private TbBuffDef _tbBuffDef;
        private TbSkillAoeDef _tbSkillAoeDef;
        private TbAoeDef _tbAoeDef;
        
        public override async void Initialize()
        {
            try
            {
                _tbSkillDef = await AppEntry.DataTable.GetDataTableLuBan<TbSkillDef>(cfg.Tables.skill_tbskilldef);
                _tbTimeLineDef = await AppEntry.DataTable.GetDataTableLuBan<TbTimeLineDef>(cfg.Tables.skill_tbtimelinedef);
                _tbSkillBulletDef = await AppEntry.DataTable.GetDataTableLuBan<TbSkillBulletDef>(cfg.Tables.skill_tbskillbulletdef);
                _tbBulletDef = await AppEntry.DataTable.GetDataTableLuBan<TbBulletDef>(cfg.Tables.skill_tbbulletdef);
                _tbBuffDef = await AppEntry.DataTable.GetDataTableLuBan<TbBuffDef>(cfg.Tables.skill_tbbuffdef);
                _tbSkillAoeDef = await AppEntry.DataTable.GetDataTableLuBan<TbSkillAoeDef>(cfg.Tables.skill_tbskillaoedef);
                _tbAoeDef = await AppEntry.DataTable.GetDataTableLuBan<TbAoeDef>(cfg.Tables.skill_tbaoedef);
            }
            catch (Exception e)
            {
                Log.Error("Initialize error = {0}",e.Message);
            }
        }
        
        public SkillModel GetSkillModel(int id)
        {
            SkillDef skillDef = _tbSkillDef.GetOrDefault(id);
            if (skillDef == null)
            {
                Log.Error("skillDef is null id = {0}",id);
                return null;
            }
            
            ChaResource cost = new ChaResource(skillDef.Cost.Hp, skillDef.Cost.Ammo, skillDef.Cost.Stamina);
            ChaResource condition = new ChaResource(skillDef.Condition.Hp, skillDef.Condition.Ammo, skillDef.Condition.Stamina);

            AddBuffInfo[] addBuffInfos = new AddBuffInfo[skillDef.AddBuff.Count];
            for (int i = 0; i < skillDef.AddBuff.Count; i++)
            {
                int buffId = skillDef.AddBuff[i];
                addBuffInfos[i] = GetAddBuffInfo(buffId);
            }
            
            SkillModel skillModel = new SkillModel(id,cost,condition,GetTimeline(skillDef.TimeLine),addBuffInfos);
            return skillModel;
        }

        public TimelineModel GetTimeline(int id)
        {
            TimeLineDef timeLineDef = _tbTimeLineDef.GetOrDefault(id);
            if (timeLineDef == null)
            {
                Log.Error("timeLineDef is null id = {0}",id);
                return null;
            }

            TimelineNode[] timelineNodes = new TimelineNode[timeLineDef.TimeLineParams.Count];
            for (int i = 0; i < timeLineDef.TimeLineParams.Count; i++)
            {
                timelineNodes[i] = new TimelineNode(timeLineDef.TimeLineParams[i]);
            }

            TimelineModel model = new TimelineModel(id, timelineNodes, MathUtils.Convert(timeLineDef.Duration),TimeLineGoTo.Null);
            return model;
        }

        public BulletData GetBulletData(int id,IUnit caster)
        {
            SkillBulletDef skillBulletDef = _tbSkillBulletDef.GetOrDefault(id);
            if (skillBulletDef == null)
            {
                Log.Error("skillBulletDef is null id = {0}",id);
                return null;
            }

            BulletModel bulletModel = GetBulletModel(skillBulletDef.BulletData);

            fix3 firePos = MathUtils.Convert(skillBulletDef.FirePos);
            // FirePos 以朝右(0°)为基准配置：
            //   X = 枪口沿朝向的水平偏移，根据瞄准水平分量翻转
            //   Y = 枪口高度偏移，不随方向变化
            // LeftUp(135°)/Left(180°)/LeftDown(225°) 属于左侧方向，翻转 X
            // Up(90°)/Down(270°) 无水平分量，跟随角色当前 FacingRight
            fix aimDeg = caster.Brian.FaceDegree;
            bool facingLeft;
            if (aimDeg > new fix(90) && aimDeg < new fix(270))
                facingLeft = true;
            else if (caster.Behaviour is RoleBehaviour rb)
                facingLeft = !rb.FacingRight;
            else
                facingLeft = false;
            fix3 newFirePos = new fix3(
                caster.Behaviour.Position.x + (facingLeft ? -firePos.x : firePos.x),
                caster.Behaviour.Position.y + firePos.y,
                caster.Behaviour.Position.z + firePos.z
            );
            
            fix skillDeg = MathUtils.Convert(skillBulletDef.Degree);
            fix fireDegree;
            if (bulletModel.tween != null)
            {
                // 手雷等物理子弹：发射角仅区分左右，arc 由物理模拟决定
                fireDegree = facingLeft ? (new fix(180) - skillDeg) : skillDeg;
            }
            else
            {
                // 普��子弹：以瞄准角度为基础，偏角跟随朝向镜像
                fireDegree = facingLeft ? (aimDeg - skillDeg) : (aimDeg + skillDeg);
            }

            BulletData bulletData = BulletData.Create(
                bulletModel, caster,
                newFirePos,
                fireDegree,
                MathUtils.Convert(skillBulletDef.Speed),
                MathUtils.Convert(skillBulletDef.Duration),
                fix.Zero,
                tween: bulletModel.tween    // 手雷等物理子弹在此传入 tween
            );

            return bulletData;
        }

        public BulletModel GetBulletModel(int id)
        {
            BulletDef bulletDef = _tbBulletDef.GetOrDefault(id);
            if (bulletDef == null)
            {
                Log.Error("bulletDef is null id = {0}",id);
                return null;
            }

            BulletModel bulletModel = BulletModel.Create(bulletDef.Id,
                bulletDef.Prefab,
                bulletDef.OnCreate,
                bulletDef.OnHit,
                bulletDef.OnRemoved,
                bulletDef.MoveType,
                bulletDef.RemoveOnObstacle,
                MathUtils.Convert(bulletDef.Radius),
                bulletDef.HitTimes,
                MathUtils.Convert(bulletDef.SameTargetDelay),
                bulletDef.HitFoe,
                bulletDef.HitAlly,
                bulletDef.TweenParam,
                bulletDef.UseWorldSpaceTween,
                bulletDef.SmoothMove);

            return bulletModel;
        }

        public AddBuffInfo GetAddBuffInfo(int buffId)
        {
            BuffDef buffDef = _tbBuffDef.GetOrDefault(buffId);
            if(buffDef == null)
            {
                Log.Error("buffDef is null id = {0}",buffId);
                return null;
            }

            BuffModel buffModel = GetBuffModel(buffId);

            return AddBuffInfo.Create(buffModel, null, 
                null, 
                1,  
                MathUtils.Convert(buffDef.Duration), 
                true, 
                buffDef.Permanent);
        }

        public BuffModel GetBuffModel(int buffId)
        {
            BuffDef buffDef = _tbBuffDef.GetOrDefault(buffId);
            if(buffDef == null)
            {
                Log.Error("buffDef is null id = {0}",buffId);
                return null;
            }
            
            RoleState roleState = new RoleState(buffDef.BuffAddState.CanMove,
                buffDef.BuffAddState.CanRotate,
                buffDef.BuffAddState.CanUseSkill);

            ChaProperty[] chaProperties = new ChaProperty[buffDef.BuffPropMod.Length];
            
            for (int j = 0; j < buffDef.BuffPropMod.Length; j++)
            {
                BuffPropMod buffPropMod = buffDef.BuffPropMod[j];
                chaProperties[j] = new ChaProperty(buffPropMod.MoveSpeed, 
                    buffPropMod.Hp,
                    buffPropMod.Ammo,
                    buffPropMod.Attack,
                    buffPropMod.ActionSpeed, 
                    MathUtils.Convert(buffPropMod.BodyRadius),
                    MathUtils.Convert(buffPropMod.HitRadius));
            }
            
            BuffModel buffModel = BuffModel.Create(buffId, "", 
                buffDef.Tag, 
                buffDef.Priority, 
                buffDef.MaxStack,
                buffDef.TickTime, 
                buffDef.OnOccur,
                buffDef.OnRemoved, 
                buffDef.OnTick, 
                buffDef.OnCast, 
                buffDef.OnHit,
                buffDef.BeHurt, 
                buffDef.OnKill, 
                buffDef.BeKilled,
                roleState,
                chaProperties);

            return buffModel;
        }

        public AoeData GetAoeData(int skillAoeId,IUnit caster)
        {
            SkillAoeDef  skillAoeDef = _tbSkillAoeDef.GetOrDefault(skillAoeId);
            if (skillAoeDef == null)
            {
                Log.Error("skillAoeDef is null id = {0}",skillAoeId);
                return null;
            }

            AoeModel model = GetAodeModel(skillAoeDef.AoeDataId);
            if (model == null)
            {
                return null;
            }
        
            AoeData aoeData = AoeData.Create(model, 
                caster, 
                fix3.zero, 
                MathUtils.Convert(skillAoeDef.Radius),
                MathUtils.Convert(skillAoeDef.Duration), 
                MathUtils.Convert(skillAoeDef.Degree));
            
            Log.Info("创建AOE: {0} 角度:{1} 半径:{2}", aoeData.AoeId,aoeData.degree,aoeData.radius);
            return aoeData;
        }

        private AoeModel GetAodeModel(int aoeId)
        {
            AoeDef aoeDef = _tbAoeDef.GetOrDefault(aoeId);
            if (aoeDef == null)
            {
                Log.Error("aoeDef is null id = {0}",aoeId);
                return null;
            }
            
            AoeModel aoeModel = AoeModel.Create(aoeDef.Id,
                aoeDef.Prefab,
                aoeDef.Tags,
                MathUtils.Convert(aoeDef.TickTime),
                aoeDef.RemoveOnObstacle,
                aoeDef.OnCreate,
                aoeDef.OnRemoved,
                aoeDef.OnTick,
                aoeDef.OnChaEnter,
                aoeDef.OnChaLeave,
                aoeDef.OnBulletEnter,
                aoeDef.OnBulletLeave);
            
            return aoeModel;
        }
    }
}