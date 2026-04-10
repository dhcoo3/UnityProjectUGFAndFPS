using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.Procedures
{
    public class GameOverProcedure : ProcedureBase
    {
        IFsm<IProcedureManager> procedure;
        private bool isWin;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            this.procedure = procedureOwner;
            isWin = this.procedure.GetData<VarBoolean>("IsWin");

            ShowGameOverUIForm(2);
        }
        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            if (!isShutdown)
            {
                Builtin.Scripts.Game.AppEntry.UI.CloseAllLoadingUIForms();
                Builtin.Scripts.Game.AppEntry.UI.CloseAllLoadedUIForms();
                Builtin.Scripts.Game.AppEntry.Entity.HideAllLoadingEntities();
                Builtin.Scripts.Game.AppEntry.Entity.HideAllLoadedEntities();
            }
            base.OnLeave(procedureOwner, isShutdown);
        }

        private void ShowGameOverUIForm(float delay)
        {
        
        }

        internal void BackHome()
        {
       
        }
    }
}
