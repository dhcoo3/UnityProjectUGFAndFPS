using HotAssets.Scripts.Extension;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;
using HotAssets.Scripts.GamePlay.Render.Entity;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GamePlay.Render.Map
{
    public class MapGridEntity:EntityRender
    {
        private Vector3 _tmpVector3 = Vector3.zero;
        
        protected override void OnShow(object userData)
        {
            EntityParams entityParams = (EntityParams)userData;
            _tmpVector3.x = entityParams.Position.Value.x;
            _tmpVector3.y = entityParams.Position.Value.y;
            _tmpVector3.z =entityParams.Position.Value.z;
            transform.position = _tmpVector3;
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = (int)MapRenderLayer.Grid;
            base.OnShow(userData);
        }

        public override void LogicUpdate(fix deltaTime)
        {
            base.LogicUpdate(deltaTime);
        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            base.OnHide(isShutdown, userData);
        }

        protected override void OnRecycle()
        {
            base.OnRecycle();
        }
    }
}