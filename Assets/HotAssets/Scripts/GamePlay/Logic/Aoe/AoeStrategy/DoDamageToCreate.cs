using System.Collections.Generic;
using HotAssets.Scripts.Common;
using HotAssets.Scripts.GamePlay.Logic.Damage;
using HotAssets.Scripts.GamePlay.Logic.ProxyManager;
using HotAssets.Scripts.GamePlay.Logic.Unit.Aoe;
using HotAssets.Scripts.GamePlay.Logic.Unit.Core;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;

namespace HotAssets.Scripts.GamePlay.Logic.Aoe.AoeStrategy
{
    public partial class AoeStrategyManager
    {
        ///<summary>
        ///onChaEnter
        ///对于范围内的人造成伤害（治疗得另写一个，这是严肃的），参数：
        ///[0]Damage：基础伤害
        ///[1]float：施法者攻击倍率
        ///[2]bool：对敌人有效
        ///[3]bool：对盟军有效
        ///[4]bool：挨打的人是否受伤动作
        ///[5]string：挨打者身上特效
        ///[6]string：挨打者特效绑点，默认"Body"
        ///</summary>
        private static void DoDamageToCreate(AoeUnit aoe,List<IUnit> characters,List<IUnit> bullets)
        {
            if (aoe.Data.model.onCreateParams is cfg.Skill.DoDamageToCreate doDamageToEnterCha)
            {
                DamageVal baseDamage = DamageVal.Create(doDamageToEnterCha.BaseDamage);
                fix damageTimes = MathUtils.Convert(doDamageToEnterCha.DamageTimes);
                bool toFoe = doDamageToEnterCha.ToFoe;
                bool toAlly = doDamageToEnterCha.ToAlly;
                bool hurtAction = doDamageToEnterCha.HurtAction;
                string effect = doDamageToEnterCha.Effect;
                string bp = doDamageToEnterCha.BpName;

                DamageVal damage = baseDamage * (aoe.Data.propWhileCreate.Attack * damageTimes);

                int side = -1;
                if (aoe.Data.caster != null &&aoe.Data.caster is RoleUnit role)
                { 
                    side = role.Data.Side;
                }

                for (int i = 0; i < characters.Count; i++){
                    if (characters[i] is RoleUnit unit 
                        && unit.IsDeath() == false 
                        && ((toFoe && side != unit.Data.Side) 
                            || (toAlly && side == unit.Data.Side)))
                    {
                        fix3 chaToAoe = unit.Behaviour.Position - aoe.Behaviour.Position;
                        
                        GameProxyManger.Instance.GetProxy<DamageProxy>().DoDamage(
                            aoe.Data.caster,
                            unit,
                            damage,
                            fixMath.atan2(chaToAoe.x, chaToAoe.z) * 180 / fixMath.PI,
                            0.05f,
                            new DamageDefine.DamageInfoTag[] { DamageDefine.DamageInfoTag.directDamage, }
                        );

                        if (hurtAction)
                        {
                           //unit.Play("Hurt");
                        }

                        if (effect != "")
                        {
                           /*GameProxyManger.Instance.GetProxy<EffectProxy>().PlayToPositionRecycle1(
                               GF.AssetBridge.GetEffect(effect),unit.Behaviour.Position);*/
                        }
                    }
                }
            }
        }
    }
}