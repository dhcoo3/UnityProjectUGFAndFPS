using GameFramework;
using HotAssets.Scripts.Common;
using HotAssets.Scripts.GamePlay.Logic.Bullet;
using HotAssets.Scripts.GamePlay.Logic.Common;

namespace HotAssets.Scripts.GamePlay.Logic.Role
{
    public class RoleMonster:RoleData
    {
        /// <summary>
        /// 巡逻中心X（绝对坐标，已根据 PatrolMode 换算好）
        /// </summary>
        public fix PatrolCenterX;

        /// <summary>
        /// 巡逻半程（单侧距离）
        /// </summary>
        public fix PatrolHalfRange;

        /// <summary>
        /// 创建怪物数据
        /// </summary>
        /// <param name="id">单位唯一id</param>
        /// <param name="cfg">怪物配置</param>
        /// <param name="patrolCenterX">巡逻中心X，绝对坐标</param>
        /// <param name="patrolHalfRange">巡逻半程</param>
        public static RoleMonster Create(int id, cfg.Entity.Monster cfg, fix patrolCenterX = default, fix patrolHalfRange = default)
        {
            RoleMonster roleData = ReferencePool.Acquire<RoleMonster>();
            roleData._roleType = GamePlayDefine.RoleType.Monster;
            roleData.Side = 1;
            roleData.RoleId = id;
            roleData.Name = cfg.Name;
            roleData.AssetPath = cfg.Asset;

            roleData.BaseProp = new ChaProperty(cfg.MoveSpeed,
                cfg.Hp,
                cfg.Ammo,
                cfg.Attack,
                cfg.ActionSpeed,
                MathUtils.Convert(cfg.BodyRadius),
                MathUtils.Convert(cfg.HitRadius),
                BulletDefine.MoveType.ground,
                cfg.SearchRange);

            roleData.Prop += roleData.BaseProp;

            roleData.Resource = new ChaResource(cfg.Hp, cfg.Ammo, cfg.Stamina);
            roleData.AIId = cfg.AI;
            roleData.AnimId = cfg.AnimGroup;
            roleData.PatrolCenterX = patrolCenterX;
            roleData.PatrolHalfRange = patrolHalfRange;
            return roleData;
        }
    }
}