using System;
using Builtin.Scripts.Extension;
using Builtin.Scripts.Game;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.Fsm;
using GameFramework.Procedure;
using HotAssets.Scripts.GamePlay.Logic.Fight;
using HotAssets.Scripts.GamePlay.Logic.ProxyManager;
using HotAssets.Scripts.UI.Module.FightMainChapter;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.Procedures
{
    public class GameOverProcedure : ProcedureBase
    {
        private IFsm<IProcedureManager> procedure;
        private bool isWin;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            procedure = procedureOwner;
            isWin = procedure.GetData<VarBoolean>("IsWin");

            Log.Info("进入结算流程，结果:{0}", isWin ? "胜利" : "失败");
            ShowGameOverUIForm(0.5f);
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            if (!isShutdown)
            {
                AppEntry.UI.CloseAllLoadingUIForms();
                AppEntry.UI.CloseAllLoadedUIForms();
                AppEntry.Entity.HideAllLoadingEntities();
                AppEntry.Entity.HideAllLoadedEntities();
            }

            base.OnLeave(procedureOwner, isShutdown);
        }

        private void ShowGameOverUIForm(float delay)
        {
            AppEntry.UI.OpenUIForm(AssetPathUtil.GetUIFormPath("FightMainChapter/FightOverPanel"), "Layer1");
            ShowGameOverUIFormAsync(delay).Forget();
        }

        internal void BackHome()
        {
            FightMainChapterController.Instance.ExitFight();

            FightProxy fightProxy = GameProxyManger.Instance.GetProxy<FightProxy>();
            fightProxy?.ExitFight();

            procedure.SetData<VarString>(ChangeSceneProcedure.PSceneName, "Home/Home");
            ChangeState<ChangeSceneProcedure>(procedure);
        }

        private async UniTaskVoid ShowGameOverUIFormAsync(float delay)
        {
            if (delay > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(delay));
            }

            BackHome();
        }
    }
}
