using cfg.UI;
using Cysharp.Threading.Tasks;
using GameFramework.Fsm;
using GameFramework.Procedure;
using HotAssets.Scripts.UI.Core;

namespace HotAssets.Scripts.Procedures
{
    public class LoginProcedure : ProcedureBase
    {
        private bool _isReady = false;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            InitAsync(procedureOwner).Forget();
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds,
            float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (_isReady)
            {
                ChangeState<RunGameProcedure>(procedureOwner);
            }
        }
        
        /// <summary>
        /// 异步初始化：等待模块注册完成后再跳转模块
        /// </summary>
        private async UniTaskVoid InitAsync(IFsm<IProcedureManager> procedureOwner)
        {
            await UIModuleManager.Instance.RegisterModule();
            UIModuleManager.Instance.GoToModule(ModuleType.Login);
            _isReady = true;
        }
    }
}
