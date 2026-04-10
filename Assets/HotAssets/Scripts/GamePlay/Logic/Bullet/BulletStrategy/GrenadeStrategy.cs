using System.Collections.Generic;
using HotAssets.Scripts.Common;
using HotAssets.Scripts.GamePlay.Logic.Aoe;
using HotAssets.Scripts.GamePlay.Logic.ProxyManager;
using HotAssets.Scripts.GamePlay.Logic.Skill;
using HotAssets.Scripts.GamePlay.Logic.Unit;
using HotAssets.Scripts.GamePlay.Logic.Unit.Bullet;
using HotAssets.Scripts.GamePlay.Logic.Unit.Component;
using HotAssets.Scripts.GamePlay.Logic.Unit.Core;

namespace HotAssets.Scripts.GamePlay.Logic.Bullet.BulletStrategy
{
    public partial class BulletStrategyManager
    {
        /// <summary>
        /// 手雷每帧物理状态（存入 BulletData.param["grenadeState"]，避免 fix 装箱）
        /// </summary>
        private class GrenadeState
        {
            public fix VelocityX;
            public fix VelocityY;
            public int BounceCount;
            public fix PrevT;
        }

        /// <summary>
        /// OnCreate：根据发射角度和初速度初始化手雷物理状态
        /// </summary>
        private static void GrenadeOnCreate(IUnit bullet)
        {
            if (bullet is not BulletUnit bulletUnit) return;
            BulletData data = bulletUnit.Data;
            if (data.model.onCreateParam is not cfg.Skill.GrenadeCreate) return;

            fix rad = data.fireDegree * fix.Pi / 180;
            GrenadeState state = new GrenadeState
            {
                VelocityX = fixMath.cos(rad) * data.speed,
                VelocityY = fixMath.sin(rad) * data.speed,
                BounceCount = 0,
                PrevT = fix.Zero
            };

            data.param ??= new Dictionary<string, object>();
            data.param["grenadeState"] = state;
        }

        /// <summary>
        /// BulletTween：每帧更新手雷速度（重力、落地弹跳、撞墙反弹）。
        /// 返回本帧绝对速度（m/s），由 CalcMoveWorldSpace 直接使用。
        /// 物理参数从 model.tweenParam（cfg.Skill.GrenadeTween）读取，与 onCreate 解耦。
        /// IsGrounded / IsHitWall 反映上一帧 UnitMove 结果，时序正确。
        /// </summary>
        private static fix3 GrenadeTween(fix t, IUnit bullet, IUnit target)
        {
            if (bullet is not BulletUnit bulletUnit) return fix3.zero;

            BulletData data = bulletUnit.Data;
            if (data.param == null || !data.param.ContainsKey("grenadeState")) return fix3.zero;

            // 物理参数来自 BulletTweenParam 子类型 GrenadeTween（配置表驱动）
            if (data.model.tweenParam is not cfg.Skill.GrenadeTween cfg) return fix3.zero;

            GrenadeState state = (GrenadeState)data.param["grenadeState"];
            BulletBehaviour behaviour = bulletUnit.Behaviour as BulletBehaviour;
            UnitMove unitMove = behaviour?.UnitMove;
            if (unitMove == null) return fix3.zero;

            fix dt = t - state.PrevT;
            state.PrevT = t;

            // 重力加速
            state.VelocityY -= MathUtils.Convert(cfg.Gravity) * dt;

            // 落地弹跳（IsGrounded 来自上帧 UnitMove.LogicUpdate）
            if (unitMove.IsGrounded)
            {
                state.BounceCount++;

                if (state.BounceCount >= cfg.MaxBounces)
                {
                    // 已达最大弹跳次数：原地停止并触发爆炸
                    data.duration = fix.Zero;
                    return fix3.zero;
                }

                state.VelocityY = -state.VelocityY * MathUtils.Convert(cfg.BounceCoeff);
                state.VelocityX *= MathUtils.Convert(cfg.GroundFriction);
            }

            // 撞墙水平反弹
            if (unitMove.IsHitWall)
            {
                state.VelocityX = -state.VelocityX * MathUtils.Convert(cfg.WallBounceCoeff);
            }

            return new fix3(state.VelocityX, state.VelocityY, fix.Zero);
        }

        /// <summary>
        /// OnRemoved：在手雷当前位置创建爆炸 AOE，走现有 SkillAoeDef 管线
        /// </summary>
        private static void GrenadeOnRemoved(IUnit bullet)
        {
            if (bullet is not BulletUnit bulletUnit) return;
            if (bulletUnit.Data.model.onRemovedParams is not cfg.Skill.GrenadeRemoved grenadeRemoved) return;

            SkillProxy skillProxy = GameProxyManger.Instance.GetProxy<SkillProxy>();
            AoeData aoeData = skillProxy.GetAoeData(grenadeRemoved.ExplosionSkillAoeId, bulletUnit.Data.caster);
            if (aoeData == null) return;

            aoeData.position = bulletUnit.Behaviour.Position;
            GameProxyManger.Instance.GetProxy<UnitProxy>().CreateAoeUnit(aoeData);
        }
    }
}
