using System.Collections.Generic;
using HotAssets.Scripts.GamePlay.Logic.Buff;
using HotAssets.Scripts.GamePlay.Logic.Unit.Bullet;
using HotAssets.Scripts.GamePlay.Logic.Unit.Core;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;

namespace HotAssets.Scripts.GamePlay.Logic.Bullet.BulletStrategy
{
      public partial class BulletStrategyManager
     {
        ///<summary>
        ///onCreate
        ///记录一下这个子弹，作为最后发射的子弹
        ///</summary>
        private static void RecordBullet(IUnit bullet){
            if (bullet is BulletUnit bulletUnit)
            {
                if (bulletUnit.Data.caster == null) return;
                if (bulletUnit.Data.caster is RoleUnit roleUnit)
                {
                   // List<BuffObj> bos = roleUnit.Data.GetBuffById("TeleportBulletPassive", new List<IUnit>(){roleUnit});
                    List<BuffObj> bos = roleUnit.Data.GetBuffById(1, new List<IUnit>(){roleUnit});
                    if (bos.Count <= 0){
                        /*roleUnit.AddBuff(new AddBuffInfo(
                            DesingerTables.Buff.data["TeleportBulletPassive"], bs.caster, bs.caster, 1, 10, true, true, new Dictionary<string, object>(){{"firedBullet", bullet}}
                        ));*/
                    }else{
                        bos[0].buffParam["firedBullet"] = bullet;
                    }
                }
            }
        }
    }
}