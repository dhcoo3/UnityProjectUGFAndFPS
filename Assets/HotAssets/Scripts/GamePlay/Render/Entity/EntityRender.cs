using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GamePlay.Render.Entity
{
    /// <summary>
    /// GameObject + Component
    /// </summary>
    public abstract class EntityRender:EntityLogic
    {
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
        }

        protected override void OnRecycle()
        {
            base.OnRecycle();
        }

        public virtual void LogicUpdate(fix deltaTime)
        {
            
        }
    }
}