using cfg.Skill;
using HotAssets.Scripts.Common;
using HotAssets.Scripts.GamePlay.Logic.Damage;
using HotAssets.Scripts.GamePlay.Logic.Effect;
using HotAssets.Scripts.GamePlay.Logic.ProxyManager;
using HotAssets.Scripts.GamePlay.Logic.Unit.Bullet;
using HotAssets.Scripts.GamePlay.Logic.Unit.Core;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;

namespace HotAssets.Scripts.GamePlay.Logic.Bullet.BulletStrategy
{
    public partial class BulletStrategyManager
    {
        ///<summary>
        ///onHit
        ///普通子弹命中效果，参数：
        ///[0]伤害倍数
        ///[1]基础暴击率
        ///[2]命中视觉特效
        ///[3]播放特效位于目标的绑点，默认Body
        ///</summary>
        private static void CommonBulletHit(IUnit bullet, IUnit target){
            BulletUnit bulletUnit = bullet as BulletUnit;
            if (bulletUnit != null && 
                bulletUnit.Data.model.onHitParams is CommonBulletHit tbCommonBulletHit)
            {
                
                fix damageTimes = MathUtils.Convert(tbCommonBulletHit.DamageTimes);
                fix critRate = MathUtils.Convert(tbCommonBulletHit.CritRate);
                string sightEffect = tbCommonBulletHit.SightEffect;
                string bpName = tbCommonBulletHit.BpName;
                
                if (sightEffect != "" && target != null && target is RoleUnit role){
                    // 在目标位置播放命中特效，1秒后自动回收
                    GameProxyManger.Instance.GetProxy<EffectProxy>().PlayToPositionRecycle1(
                        sightEffect, role.Behaviour.Position);
                }

                int bulletVal = fixMath.ceilToInt(damageTimes * bulletUnit.Data.propWhileCast.Attack);
                DamageVal damageVal = DamageVal.Create(bulletVal);
            
                GameProxyManger.Instance.GetProxy<DamageProxy>().DoDamage(
                    bulletUnit.Data.caster,
                    target,
                    damageVal,
                    bulletUnit.Behaviour.Rotation.eulerAngles.y,
                    critRate,
                    new DamageDefine.DamageInfoTag[] { DamageDefine.DamageInfoTag.directDamage, }
                );
            }
        }
    }
}