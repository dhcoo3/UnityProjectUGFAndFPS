using HotAssets.Scripts.GamePlay.Logic.Common;
using HotAssets.Scripts.GamePlay.Logic.ProxyManager;
using HotAssets.Scripts.GamePlay.Logic.Unit;

namespace HotAssets.Scripts.GamePlay.Logic.Fight
{
    public class FightLoadingProxy:GameProxy
    {
        private UnitProxy _unitProxy;
        
        private float _roleProgress;
        private float _sceneProgress;
        private int _prefabMaxProgress;

        private bool _isLoading;

        public override void Initialize()
        {
            _isLoading = true;
            _roleProgress = 0;
            _sceneProgress = 0;
            _unitProxy = GetProxy<UnitProxy>();
        }

        public override void Clear()
        {
            _isLoading = true;
            _roleProgress = 0;
            _sceneProgress = 0;
            base.Clear();
        }
        
        public void SetRoleProgress(float currentProgress)
        {
            _roleProgress = currentProgress;
            CheckLoadFinished();
        }
        
        public void SetSceneProgress(float currentProgress)
        {
            _sceneProgress = currentProgress;
            CheckLoadFinished();
            if (_sceneProgress >= 1)
            {
                _unitProxy.InitRole();
            }
        }

        private void CheckLoadFinished()
        {
            if(_roleProgress < 1 || _sceneProgress < 1) return;
            if(!_isLoading) return;
            _isLoading = false;
            Fire(GamePlayEvent.EFightLoadingFinish);
        }
    }
}