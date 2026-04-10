using GameFramework;
using HotAssets.Scripts.Common;
using HotAssets.Scripts.GamePlay.Logic.Bullet;
using HotAssets.Scripts.GamePlay.Logic.Common;
using HotAssets.Scripts.GamePlay.Logic.Player;

namespace HotAssets.Scripts.GamePlay.Logic.Role
{
    public class RoleHero:RoleData
    {
        public static RoleHero Create(PlayerData playerData,int id,cfg.Entity.Hero entityData)
        {
            RoleHero roleData = ReferencePool.Acquire<RoleHero>();
            roleData._roleType = GamePlayDefine.RoleType.Hero;
            roleData.RoleId = id;
            roleData.Name = entityData.Name;
            roleData.AssetPath = entityData.Asset;
            roleData.AnimId = entityData.AnimGroup;
            roleData.WallJumpForceY = MathUtils.Convert(entityData.WallJumpForceY);
            roleData.WallJumpForceX = MathUtils.Convert(entityData.WallJumpForceX);
            roleData.WallJumpDuration = MathUtils.Convert(entityData.WallJumpDuration);
            roleData.BaseProp = new ChaProperty(entityData.MoveSpeed,
                entityData.Hp,
                entityData.Ammo,
                entityData.Attack,
                entityData.ActionSpeed,
                MathUtils.Convert(entityData.BodyRadius),
                MathUtils.Convert(entityData.HitRadius),
                BulletDefine.MoveType.ground,
                0,
                MathUtils.Convert(entityData.WallSlideSpeed)
            );
            roleData.Prop += roleData.BaseProp;
            roleData.Resource = new ChaResource(entityData.Hp, entityData.Ammo, entityData.Stamina);
            roleData.OperateId = playerData.Id;
            return roleData;
        }
    }
}