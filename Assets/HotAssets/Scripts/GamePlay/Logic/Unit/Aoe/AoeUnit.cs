using GameFramework;
using HotAssets.Scripts.GamePlay.Logic.Aoe;
using HotAssets.Scripts.GamePlay.Logic.Unit.Component;
using HotAssets.Scripts.GamePlay.Logic.Unit.Core;

namespace HotAssets.Scripts.GamePlay.Logic.Unit.Aoe
{
    /// <summary>
    /// Aoe单位
    /// </summary>
    public class AoeUnit:IUnit
    {
        #region interface
        public IBrian Brian => _brian;
        public IBehaviour Behaviour =>_behaviour;
        public cfg.Game.MoveType MoveType => cfg.Game.MoveType.Ground;
        public fix BodyRadius => 0.25f;
        public bool SmoothMove => false;
        public bool IgnoreBorder => false;
        
        #endregion
        
        private AoeData _data;
        public AoeData Data => _data;
        
        AoeBrian _brian;
        
        AoeBehaviour _behaviour;
        public AoeBehaviour AoeBehaviour => _behaviour;

        public bool HasEntity = false;
        
        public static AoeUnit Create(AoeData data)
        {
            AoeUnit aoeUnit = ReferencePool.Acquire<AoeUnit>();
            aoeUnit._data = data;
            aoeUnit._brian = AoeBrian.CreateBrian(aoeUnit);
            aoeUnit._behaviour = AoeBehaviour.CreateBehaviour(aoeUnit);
            return aoeUnit;
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

        public void LogicUpdate(fix deltaTime)
        {
            if(!HasEntity) return;
            _brian?.LogicUpdate(deltaTime);
            _behaviour?.LogicUpdate(deltaTime);
        }
    }
}