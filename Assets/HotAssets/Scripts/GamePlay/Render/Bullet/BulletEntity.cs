using HotAssets.Scripts.Extension;
using HotAssets.Scripts.GamePlay.Logic.Unit.Bullet;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;
using HotAssets.Scripts.GamePlay.Render.Entity;
using HotAssets.Scripts.GamePlay.Render.Map;
using TuanjieAI.Assistant.Schema;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GamePlay.Render.Bullet
{
    public class BulletEntity:EntityRender
    {
        public int BulletId = 0;
        
        public BulletUnit BulletUnit { get; private set; }
        
        private Vector3 _tmpVector3 = Vector3.zero;
        
        public Vector3 Position
        {
            get
            {
                return transform.position;
            }
            set
            {
                transform.position = value;
            }
        }
        
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
        }

        protected override void OnShow(object userData)
        {
            EntityParams entityParams = (EntityParams)userData;
            BulletUnit = entityParams.Unit as BulletUnit;
           
            if (BulletUnit == null)
            {
                Log.Error("RoleData is invalid.");
                return;
            }

            BulletId = BulletUnit.Data.BulletId;
            
            _tmpVector3.x =  BulletUnit.Behaviour.Position.x;
            _tmpVector3.y =  BulletUnit.Behaviour.Position.y;
            _tmpVector3.z =  BulletUnit.Behaviour.Position.z;
            
            Position = _tmpVector3;
            transform.rotation = BulletUnit.Behaviour.Rotation;
            
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer)
            {
                spriteRenderer.sortingOrder = (int)MapRenderLayer.Bullet;
            }
            
            BulletUnit.HasEntity = true;
            entityParams.OnShowCallback.Invoke(this);
            base.OnShow(userData);
        }

        public override void LogicUpdate(fix deltaTime)
        {
            if (BulletUnit == null) return;
            //将逻辑层坐标同步到渲染层
            _tmpVector3.x =  BulletUnit.Behaviour.Position.x;
            _tmpVector3.y =  BulletUnit.Behaviour.Position.y;
            _tmpVector3.z =  BulletUnit.Behaviour.Position.z;
            
            transform.position = _tmpVector3;
            transform.rotation = BulletUnit.Behaviour.Rotation;
            base.LogicUpdate(deltaTime);
        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            BulletUnit = null;
            base.OnHide(isShutdown, userData);
        }

        protected override void OnRecycle()
        {
            BulletUnit = null;
            base.OnRecycle();
        }
    }
}