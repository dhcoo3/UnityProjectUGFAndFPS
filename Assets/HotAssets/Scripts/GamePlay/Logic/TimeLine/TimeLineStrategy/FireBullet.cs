using HotAssets.Scripts.GamePlay.Logic.Bullet;
using HotAssets.Scripts.GamePlay.Logic.ProxyManager;
using HotAssets.Scripts.GamePlay.Logic.Skill;
using HotAssets.Scripts.GamePlay.Logic.Unit;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GamePlay.Logic.TimeLine.TimeLineStrategy
{
    public partial class TimeLineStrategyManager
    {
        ///<summary>
        ///在Caster的某个绑点(Muzzle/Head/Body)上发射一个子弹出来
        ///<param name="args">总共3个参数：
        ///[0]BulletLauncher：子弹发射信息，其中caster和position是需要获得后该写的，degree则需要加上角色的转向
        ///[1]string：角色身上绑点位置，默认Muzzle
        ///</param>
        ///</summary>
        private static void FireBullet(TimelineObj tlo, TimelineNode timelineNode){
            Log.Info("FireBullet");
            if (timelineNode.TimelineNodeDef is cfg.Skill.FireBullet fireBullet)
            {
                if (tlo.caster != null && tlo.caster is RoleUnit roleUnit && !tlo.caster.IsDeath())
                {
                    //UnitBindManager ubm = tlo.caster.GetComponent<UnitBindManager>();
                    //if (!ubm) return;

                    SkillProxy skillProxy = GameProxyManger.Instance.GetProxy<SkillProxy>();
                    BulletData bData = skillProxy.GetBulletData(fireBullet.BulletDataId,tlo.caster);
                    bData.propWhileCast = roleUnit.Data.Property;
                    GameProxyManger.Instance.GetProxy<UnitProxy>().CreateBulletUnit(bData);
                }
            }
        }
    }
}