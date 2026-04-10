using GameFramework;

namespace HotAssets.Scripts.GamePlay.Logic.Unit.Core
{
    public interface IBrian:IReference
    {
        fix FaceDegree { get; }
        
        fix MoveDegree { get;}
        
        void LogicUpdate(fix deltaTime);
    }
}