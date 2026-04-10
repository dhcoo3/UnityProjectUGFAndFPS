using Builtin.Scripts.Extension;
using Builtin.Scripts.Game;
using GameFramework;
using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using HotAssets.Scripts.Common.Event;
using HotAssets.Scripts.GamePlay.Logic.Common;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.Procedures
{
    public class ChangeSceneProcedure : ProcedureBase
    {
        /// <summary>
        /// 要加载的场景资源名,相对于场景目录
        /// </summary>
        internal const string PSceneName = "SceneName";
        private bool _loadSceneOver = false;
        private string _nextScene = string.Empty;
        private AAAGameEventHelper _aaaGameEventHelper;
        
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            _loadSceneOver = false;
            _aaaGameEventHelper = ReferencePool.Acquire<AAAGameEventHelper>();
            AppEntry.Event.Subscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
            AppEntry.Event.Subscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);
            AppEntry.Event.Subscribe(LoadSceneUpdateEventArgs.EventId, OnLoadSceneUpdate);
        
            // 停止所有声音
            AppEntry.Sound.StopAllLoadingSounds();
            AppEntry.Sound.StopAllLoadedSounds();

            // 隐藏所有实体
            AppEntry.Entity.HideAllLoadingEntities();
            AppEntry.Entity.HideAllLoadedEntities();

            // 卸载所有场景
            string[] loadedSceneAssetNames = AppEntry.Scene.GetLoadedSceneAssetNames();
            for (int i = 0; i < loadedSceneAssetNames.Length; i++)
            {
                AppEntry.Scene.UnloadScene(loadedSceneAssetNames[i]);
            }

            // 还原游戏速度
            AppEntry.Base.ResetNormalGameSpeed();

            if (!procedureOwner.HasData(PSceneName))
            {
                throw new GameFrameworkException("未设置要加载的场景资源名!");
            }
        
            _nextScene = procedureOwner.GetData<VarString>(PSceneName);
            procedureOwner.RemoveData(PSceneName);
        
            AppEntry.Scene.LoadScene(AssetPathUtil.GetScenePath(_nextScene), this);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (!_loadSceneOver)
            {
                return;
            }

            //场景加载完成,根据不同场景切换对应Procedure
            switch (_nextScene)
            {
                case "Login/Login":
                    ChangeState<LoginProcedure>(procedureOwner);
                    break;
                default:
                    ChangeState<RunGameProcedure>(procedureOwner);
                    break;
            }
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            AppEntry.Event.Unsubscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
            AppEntry.Event.Unsubscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);
            AppEntry.Event.Unsubscribe(LoadSceneUpdateEventArgs.EventId, OnLoadSceneUpdate);
            ReferencePool.Release(_aaaGameEventHelper);
            base.OnLeave(procedureOwner, isShutdown);
        }
    
        private void OnLoadSceneUpdate(object sender, GameEventArgs e)
        {
            var arg = (LoadSceneUpdateEventArgs)e;
            if (arg.UserData != this)
            {
                return;
            }
            Log.Info("场景加载进度:{0}, {1}", arg.Progress, arg.SceneAssetName);
            //TODO 显示场景加载进度
            _aaaGameEventHelper.Fire(GamePlayEvent.ELoadSceneUpdate,arg.Progress,arg.SceneAssetName);
        }

        private void OnLoadSceneSuccess(object sender, GameEventArgs e)
        {
            var arg = (LoadSceneSuccessEventArgs)e;
            if (arg.UserData != this)
            {
                return;
            }
            Log.Info("场景加载成功:{0}", arg.SceneAssetName);
            _loadSceneOver = true;
            _aaaGameEventHelper.Fire(GamePlayEvent.ELoadSceneSuccess);
        }
    
        //加载场景资源失败 重启游戏框架
        private void OnLoadSceneFailure(object sender, GameEventArgs e)
        {
            var arg = (LoadSceneFailureEventArgs)e;
            if (arg.UserData != this)
            {
                return;
            }

            Log.Error("加载场景失败！", arg.SceneAssetName);
            //AppEntry.Shutdown(ShutdownType.Restart);
            _aaaGameEventHelper.Fire(GamePlayEvent.ELoadSceneFailure);
        }
    }
}
