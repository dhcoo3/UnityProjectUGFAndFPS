using System.Collections.Generic;
using Builtin.Scripts.Extension;
using Builtin.Scripts.Game;
using cfg;
using GameFramework;
using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using GameFramework.Resource;
using HotAssets.Scripts.Extension;
using HotAssets.Scripts.UI;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.Procedures
{
    public class PreloadProcedure : ProcedureBase
    {
        private int totalProgress;
        private int loadedProgress;
        private float smoothProgress;
        private bool preloadAllCompleted;
        private float progressSmoothSpeed = 10f;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            AppEntry.BuiltinView.ShowLoadingProgress();
            Log.Info("进入HybridCLR热更流程! 预加载游戏数据...");

            AppEntry.Event.Subscribe(LoadDictionarySuccessEventArgs.EventId, OnLoadDictionarySuccess);
            AppEntry.Event.Subscribe(LoadDictionaryFailureEventArgs.EventId, OnLoadDictionaryFailure);

            PreloadAndInitData();
        }


        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            AppEntry.Event.Unsubscribe(LoadDictionarySuccessEventArgs.EventId, OnLoadDictionarySuccess);
            AppEntry.Event.Unsubscribe(LoadDictionaryFailureEventArgs.EventId, OnLoadDictionaryFailure);
            AppEntry.BuiltinView.HideLoadingProgress();
            base.OnLeave(procedureOwner, isShutdown);
        }


        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds,
            float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (totalProgress <= 0 || preloadAllCompleted) return;

            smoothProgress = Mathf.Lerp(smoothProgress, loadedProgress / (float)totalProgress,
                elapseSeconds * progressSmoothSpeed);

            AppEntry.BuiltinView.SetLoadingProgress(smoothProgress);

            //预加载完成 切换场景
            if (loadedProgress >= totalProgress && smoothProgress >= 0.99f)
            {
                preloadAllCompleted = true;
                Log.Info("预加载完成, 进入游戏场景.");
                procedureOwner.SetData<VarString>(ChangeSceneProcedure.PSceneName, "Login/Login");
                ChangeState<ChangeSceneProcedure>(procedureOwner);
            }
        }

        /// <summary>
        /// 预加载数据表、游戏配置,以及初始化游戏数据
        /// </summary>
        private async void PreloadAndInitData()
        {
            preloadAllCompleted = false;
            smoothProgress = 0;
            loadedProgress = 0;
            totalProgress = 1;
            CreateMainEntryExtension();
        }

        private async void LoadConfigsAndDataTables()
        {
            LocalLanguage();
        }

        private void CreateMainEntryExtension()
        {
            AppEntry.Resource.LoadAsset(AssetPathUtil.GetEntity("Core/GameExtension"),typeof(GameObject),
                new LoadAssetCallbacks(OnLoadMainEntryExtensionSuccess, OnLoadMainEntryExtensionFailed));
        }

        /// <summary>
        /// 初始化语言
        /// </summary>
        private async void LocalLanguage()
        {
            GameFramework.Localization.Language language = AppEntry.Setting.GetLanguage();

            if (language == GameFramework.Localization.Language.Unspecified)
            {
#if UNITY_EDITOR
                language = AppEntry.Base.EditorLanguage;
#else
                language = AppEntry.Localization.SystemLanguage;//默认语言跟随用户操作系统语言
#endif
            }

            AppEntry.Setting.SetLanguage(language, false);

            Log.Info(Utility.Text.Format("初始化游戏设置. 游戏语言:{0},系统语言:{1}", language, AppEntry.Localization.SystemLanguage));

            AppEntry.Localization.RemoveAllRawStrings();

            TbLocalization tbLocalization = await AppEntry.DataTable.GetDataTableLuBan<TbLocalization>(Tables.tblocalization);

            ///把多语言表的数据导入到组件
            Dictionary<string, string> dic = new Dictionary<string, string>(tbLocalization.DataList.Count);

            foreach (var data in tbLocalization.DataList)
            {
                dic.Add(data.Id, data.ChineseSimplified);
            }

            AppEntry.Localization.ReadData(AssetPathUtil.GetDataTablePath(Tables.tblocalization, true), dic);
        }

        private void OnLoadMainEntryExtensionSuccess(string assetName, object asset, float duration, object userData)
        {
            var mainEntryExtPfb = asset as GameObject;
            if (null != GameObject.Instantiate(mainEntryExtPfb, Vector3.zero, Quaternion.identity,
                    AppEntry.Base.transform))
            {
                Log.Info("MainEntry框架扩展成功!");
                loadedProgress++;
                LoadConfigsAndDataTables();
            }
        }

        private void OnLoadMainEntryExtensionFailed(string assetName, LoadResourceStatus status, string errorMessage,
            object userData)
        {
            Log.Error(Utility.Text.Format("MainEntry框架扩展加载失败:{0}, Error:{1}", assetName, errorMessage));
        }

        /// <summary>
        /// 加载本地化文件成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoadDictionarySuccess(object sender, GameEventArgs e)
        {
            LoadDictionarySuccessEventArgs ne = (LoadDictionarySuccessEventArgs)e;
            loadedProgress++;
            Log.Info("Load Dictionary '{0}' OK.", ne.DictionaryAssetName);
        }

        /// <summary>
        /// 加载本地化文件失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoadDictionaryFailure(object sender, GameEventArgs e)
        {
            LoadDictionaryFailureEventArgs ne = (LoadDictionaryFailureEventArgs)e;
            Log.Error("Can not load dictionary '{0}' from '{1}' with error message '{2}'.", ne.DictionaryAssetName,
                ne.DictionaryAssetName, ne.ErrorMessage);
        }
    }
}