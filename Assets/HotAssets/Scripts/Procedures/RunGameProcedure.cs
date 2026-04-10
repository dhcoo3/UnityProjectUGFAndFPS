using Builtin.Scripts.Event;
using Builtin.Scripts.Game;
using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.Procedures
{
    public class RunGameProcedure : ProcedureBase
    {
        private string _nextScene = string.Empty;
        
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            AppEntry.Event.Subscribe(ChangeSceneArgs.EventId, ChangeSceneHandle);
            base.OnEnter(procedureOwner);
        }
       
        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (_nextScene != string.Empty)
            {
                procedureOwner.SetData<VarString>(ChangeSceneProcedure.PSceneName, _nextScene);
                ChangeState<ChangeSceneProcedure>(procedureOwner);
            }
        }
        
        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            AppEntry.Event.Unsubscribe(ChangeSceneArgs.EventId, ChangeSceneHandle);
            _nextScene = string.Empty;
            base.OnLeave(procedureOwner, isShutdown);
        }
        
        private void ChangeSceneHandle(object sender, GameEventArgs e)
        {
            var arg = (ChangeSceneArgs)e;
            if (string.IsNullOrEmpty(arg.SceneName))
            {
                return;
            }
            _nextScene = arg.SceneName;
        }
    }
}
