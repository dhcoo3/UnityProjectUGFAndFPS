using HotAssets.Scripts.GamePlay.Logic.Effect;
using HotAssets.Scripts.GamePlay.Render.Entity;
using HotAssets.Scripts.GamePlay.Render.RenderManager;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GamePlay.Render.Effect
{
    public class EffectEntity:EntityRender
    {
        private int _effectId;
        
        private fix _elapseTime = 0;
        
        private fix _recycleTime = 0;
        
        private bool _preDestroyed = false;
        
        private EffectRender _effectRender;
        
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
            _effectRender = GameRenderManager.Instance.GetRender<EffectRender>();
            
            /*AssetInfo assetInfo = (AssetInfo)userData;
            EffectData effectData = assetInfo.UserData as EffectData;
           
            if (effectData == null)
            {
                Log.Error("effectData is invalid.");
                return;
            }
            
            _tmpVector3.x = effectData.CreatePosition.x;
            _tmpVector3.y = effectData.CreatePosition.y;
            _tmpVector3.z = effectData.CreatePosition.z;
            
            Position = _tmpVector3;
            
            _effectId = effectData.Id;
            
            if (effectData.RecycleTime > 0)
            {
                _recycleTime = effectData.RecycleTime;
            }

            _preDestroyed = false;
            
            Log.Info("EffectEntity:OnShow");*/
            base.OnShow(userData);
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            _elapseTime += elapseSeconds;
            if (!_preDestroyed && _recycleTime > 0 && _elapseTime > _recycleTime)
            {
                _preDestroyed = true;
                _effectRender.StopRenderRole(_effectId);
            }
            base.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            _preDestroyed = false;
            _elapseTime = 0;
            _recycleTime = 0;
            base.OnHide(isShutdown, userData);
        }

        protected override void OnRecycle()
        {
            base.OnRecycle();
        }
    }
}