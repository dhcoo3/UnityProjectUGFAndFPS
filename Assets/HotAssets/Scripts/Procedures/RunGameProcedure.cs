using Builtin.Scripts.Event;
using Builtin.Scripts.Game;
using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using HotAssets.Scripts.Common.Event;
using HotAssets.Scripts.GamePlay.Logic.Common;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.Procedures
{
    public class RunGameProcedure : ProcedureBase
    {
        private string _nextScene = string.Empty;
        private bool _gameOverRequested;
        private bool _isWin;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            AppEntry.Event.Subscribe(ChangeSceneArgs.EventId, ChangeSceneHandle);
            AppEntry.Event.Subscribe(GamePlayEvent.EGameOver, OnGameOverHandle);
            base.OnEnter(procedureOwner);
            _gameOverRequested = false;
            _isWin = false;
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (_gameOverRequested)
            {
                procedureOwner.SetData<VarBoolean>("IsWin", _isWin);
                ChangeState<GameOverProcedure>(procedureOwner);
                _gameOverRequested = false;
                return;
            }

            if (_nextScene != string.Empty)
            {
                procedureOwner.SetData<VarString>(ChangeSceneProcedure.PSceneName, _nextScene);
                ChangeState<ChangeSceneProcedure>(procedureOwner);
            }
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            AppEntry.Event.Unsubscribe(ChangeSceneArgs.EventId, ChangeSceneHandle);
            AppEntry.Event.Unsubscribe(GamePlayEvent.EGameOver, OnGameOverHandle);
            _nextScene = string.Empty;
            _gameOverRequested = false;
            _isWin = false;
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

        private void OnGameOverHandle(object sender, GameEventArgs e)
        {
            var arg = (GameEvent)e;
            _isWin = arg.GetParam1<bool>();
            _gameOverRequested = true;
            _nextScene = string.Empty;
        }
    }
}
