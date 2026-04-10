using GameFramework;
using HotAssets.Scripts.GamePlay.Logic.Bullet;
using HotAssets.Scripts.GamePlay.Logic.Unit.Component;
using HotAssets.Scripts.GamePlay.Logic.Unit.Core;

namespace HotAssets.Scripts.GamePlay.Logic.Unit.Bullet
{
    public class BulletUnit:IUnit
    {
        #region interface
        public IBrian Brian => _brian;
        public IBehaviour Behaviour =>_behaviour;
        public cfg.Game.MoveType MoveType => _data.model.moveType;
        public fix BodyRadius => 0.25f;
        public bool SmoothMove => _data?.model?.smoothMove ?? false;
        public bool IgnoreBorder => false;
        
        #endregion
        
        BulletBrian _brian;
        
        BulletBehaviour _behaviour;
        
        private BulletData _data;
        public BulletData Data => _data;
        
        public bool HasEntity = false;
        
        public static BulletUnit Create(BulletData bulletData)
        {
            BulletUnit bullet = ReferencePool.Acquire<BulletUnit>();
            bullet._data = bulletData;
            bullet._brian = BulletBrian.CreateBrian(bullet);
            bullet._behaviour = BulletBehaviour.CreateBehaviour(bullet);
            return bullet;
        }

        public void Clear()
        {
            HasEntity = false;
            ReferencePool.Release(_behaviour);
            ReferencePool.Release(_brian);
            ReferencePool.Release(_data);
            _behaviour = null;
            _brian = null;
            _data = null;
        }

        public bool IsDeath()
        {
            return false;
        }

        public void LogicUpdate(fix fixedDeltaTime)
        {
            if(!HasEntity) return;
            _brian?.LogicUpdate(fixedDeltaTime);
            _behaviour?.LogicUpdate(fixedDeltaTime);
        }
    }
}