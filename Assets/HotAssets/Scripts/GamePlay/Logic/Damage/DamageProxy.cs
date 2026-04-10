using System.Collections.Generic;
using GameFramework;
using HotAssets.Scripts.Bridge;
using HotAssets.Scripts.GamePlay.Logic.Buff;
using HotAssets.Scripts.GamePlay.Logic.ProxyManager;
using HotAssets.Scripts.GamePlay.Logic.Role;
using HotAssets.Scripts.GamePlay.Logic.Unit.Core;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GamePlay.Logic.Damage
{
    public class DamageProxy:GameProxy
    {
        private readonly List<DamageInfo> _damageInfos = new List<DamageInfo>();
        
        public override void Initialize()
        {
          
        }

        public override void LogicUpdate(fix deltaTime)
        {
            int i = 0;
            while( i < _damageInfos.Count ){
                DealWithDamage(_damageInfos[i]);
                ReferencePool.Release(_damageInfos[i]);
                _damageInfos.RemoveAt(0);
            }
        }

        ///<summary>
        ///处理DamageInfo的流程，也就是整个游戏的伤害流程
        ///<param name="dInfo">要处理的damageInfo</param>
        ///</summary>
        private void DealWithDamage(DamageInfo dInfo){
            //如果目标已经挂了，就直接return了
            if (dInfo.defender == null || dInfo.attacker == null) return;

            RoleUnit attacker = dInfo.attacker as RoleUnit;
            RoleUnit defender = dInfo.defender as RoleUnit;
            
            if(attacker == null || defender == null) return;
            
            if (defender.RoleState.IsDeath) return;

            List<BuffObj> attackerBuffs = attacker.Data.Buffs;
            List<BuffObj> defenderBuffs = defender.Data.Buffs;
            
            //先走一遍所有攻击者的onHit
            for (int i = 0; i < attackerBuffs.Count; i++){
                if (attackerBuffs[i].model.onHit != null){
                    attackerBuffs[i].model.onHit(attackerBuffs[i], ref dInfo, dInfo.defender);
                }
            }
            
            //然后走一遍挨打者的beHurt
            for (int i = 0; i < defenderBuffs.Count; i++){
                if (defenderBuffs[i].model.onBeHurt != null){
                    defenderBuffs[i].model.onBeHurt(defenderBuffs[i], ref dInfo, dInfo.attacker);
                }
            }
            
            if (defender.CanBeKilledByDamageInfo(dInfo) == true){
                //如果角色可能被杀死，就会走OnKill和OnBeKilled，这个游戏里面没有免死金牌之类的技能，所以只要判断一次就好
                for (int i = 0; i < attackerBuffs.Count; i++){
                    if (attackerBuffs[i].model.onKill != null){
                        attackerBuffs[i].model.onKill(attackerBuffs[i], dInfo, dInfo.defender);
                    }
                }
                
                for (int i = 0; i < defenderBuffs.Count; i++){
                    if (defenderBuffs[i].model.onBeKilled != null){
                        defenderBuffs[i].model.onBeKilled(defenderBuffs[i], dInfo, dInfo.attacker);
                    }
                }
            }
            
            //最后根据结果处理：如果是治疗或者角色非无敌，才会对血量进行调整。
            bool isHeal = dInfo.isHeal();
            int dVal = dInfo.DamageValue(isHeal);
            if (isHeal || defender.RoleState.ImmuneTime <= 0){
                if (dInfo.requireDoHurt()  && defender.CanBeKilledByDamageInfo(dInfo) == false)
                {
                    /*if (defender.Behaviour is RoleBehaviour roleBehaviour)
                    {
                        cfg.Anim.Direction direction = Utils.GetEightDirection(defender.Brian.FaceDegree);
                        roleBehaviour.AddAnim(direction,cfg.Anim.Type.TakeDamage,defender.Data.actionSpeed);
                    }*/
                }
                
                Log.Info("ModResource");
                
                defender.ModResource(new ChaResource(
                    -dVal
                ));
                
               //更新血条 
               GamePlayToUIBridge.Instance.UpdateHp(dInfo.defender as RoleUnit);
                
               //弹伤害数字
               GamePlayToUIBridge.Instance.PopUpNumberOnCharacter(dInfo.defender as RoleUnit, dVal, isHeal);
            }

            //伤害流程走完，添加buff
            for (int i = 0; i < dInfo.addBuffs.Count; i++){
                IUnit toCha = dInfo.addBuffs[i].target;
                RoleUnit toChaState = toCha.Equals(dInfo.attacker) ? attacker : defender;

                if (toChaState.RoleState.IsDeath)
                {
                    toChaState.AddBuff(dInfo.addBuffs[i]);
                }
            }
        }

        ///<summary>
        ///添加一个damageInfo
        ///<param name="attacker">攻击者，可以为null</param>
        ///<param name="target">挨打对象</param>
        ///<param name="damageVal">基础伤害值</param>
        ///<param name="damageDegree">伤害的角度</param>
        ///<param name="criticalRate">暴击率，0-1</param>
        ///<param name="tags">伤害信息类型</param>
        ///</summary>
        public void DoDamage(IUnit attacker, IUnit target, DamageVal damageVal, fix damageDegree, fix criticalRate, DamageDefine.DamageInfoTag[] tags)
        {
            DamageInfo dInfo = DamageInfo.Create(attacker, target, damageVal,damageDegree, criticalRate, tags);
            _damageInfos.Add(dInfo);
        }
    }
}