using Builtin.Scripts.Extension;
using Builtin.Scripts.Game;
using cfg.UI;
using Cysharp.Text;
using HotAssets.Scripts.GamePlay.Logic.Common;
using HotAssets.Scripts.UI;
using HotAssets.Scripts.UI.Core;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.Extension
{
    public static class UIComponentExtension
    {
        /// <summary>
        /// 打开指定资源名称的子UI界面。
        /// </summary>
        /// <param name="uiCom">UI组件实例。</param>
        /// <param name="assetName">要打开的UI资源名称。</param>
        /// <param name="uiGroup">UI组名称。</param>
        /// <param name="parms">UI参数，可选，默认为null。</param>
        /// <returns>返回打开的UI表单ID，如果UI正在加载则返回-1。</returns>
        public static int OpenSubUI(this UIComponent uiCom, string assetName, string uiGroup, UIParams parms = null)
        {
            string uiName = AssetPathUtil.GetUIFormPath(assetName);
            if (uiCom.IsLoadingUIForm(uiName))
            {
                if (parms != null) GameExtension.VariablePool.ClearVariables(parms.Id);
                return -1;
            }
            parms ??= UIParams.Create();
            return uiCom.OpenUIForm(uiName,uiGroup , GamePlayDefine.LoadPriority.UI, parms);
        }
        
        /// <summary>
        /// 获取当前顶层的UI界面id(排除子界面)
        /// </summary>
        /// <param name="uiCom"></param>
        /// <returns></returns>
        public static int GetTopUIFormId(this UIComponent uiCom)
        {
            /*var dialogGp = uiCom.GetUIGroup(Const.UIGroup.Dialog.ToString());
            var allUIForms = dialogGp.GetAllUIForms();
            int maxSortOrder = -1;
            int maxOrderIndex = -1;
            for (int i = 0; i < allUIForms.Length; i++)
            {
                var uiBase = (allUIForms[i] as UIForm).Logic as UIFormBase;
                if (uiBase == null || uiBase.Params.IsSubUIForm) continue;

                int curOrder = uiBase.SortOrder;
                if (curOrder >= maxSortOrder)
                {
                    maxSortOrder = curOrder;
                    maxOrderIndex = i;
                }
            }
            if (maxOrderIndex != -1) return allUIForms[maxOrderIndex].SerialId;

            maxSortOrder = -1;
            maxOrderIndex = -1;
            var uiFormGp = uiCom.GetUIGroup(Const.UIGroup.UIForm.ToString());
            allUIForms = uiFormGp.GetAllUIForms();
            for (int i = 0; i < allUIForms.Length; i++)
            {
                var uiBase = (allUIForms[i] as UIForm).Logic as UIFormBase;
                if (uiBase == null || uiBase.Params.IsSubUIForm) continue;

                int curOrder = uiBase.SortOrder;
                if (curOrder >= maxSortOrder)
                {
                    maxSortOrder = curOrder;
                    maxOrderIndex = i;
                }
            }
            if (maxOrderIndex != -1) return allUIForms[maxOrderIndex].SerialId;*/
            return -1;
        }
        
        /// <summary>
        /// 打开指定的UI表单。
        /// </summary>
        /// <param name="uiCom">UI组件实例。</param>
        /// <param name="uiGameModule">要打开的UI模块信息。</param>
        /// <param name="parms">UI参数，可以为null。</param>
        /// <returns>返回操作的序列号，如果小于0则表示操作失败。</returns>
        public static int OpenUIForm(this UIComponent uiCom, GameModule uiGameModule, UIParams parms = null)
        {
            if (uiGameModule == null)
            {
                Log.Error("UIMduleManager::GoToModule uiModule is null.");
                return -1;
            }
        
            string path = ZString.Format("{0}/{1}", uiGameModule.Id.ToString(), uiGameModule.Entry);
            string uiName = AssetPathUtil.GetUIFormPath(path);
            if (uiCom.IsLoadingUIForm(uiName))
            {
                if (parms != null) GameExtension.VariablePool.ClearVariables(parms.Id);
                return -1;
            }
        
            parms ??= UIParams.Create();
            return uiCom.OpenUIForm(uiName,"Layer1" , GamePlayDefine.LoadPriority.UI, parms);
        }
        
        /// <summary>
        /// 打开指定模块类型的UI覆盖层。
        /// </summary>
        /// <param name="uiCom">UI组件实例。</param>
        /// <param name="moduleType">模块类型。</param>
        /// <param name="assetName">资源名称。</param>
        /// <param name="parms">UI参数，可选，默认为null。</param>
        /// <returns>返回打开UI表单的结果，如果UI表单正在加载则返回-1。</returns>
        public static int OpenUIOverlay(this UIComponent uiCom, ModuleType moduleType, string assetName,
            UIParams parms = null)
        {
            string path = $"{moduleType.ToString()}/{assetName}";
            string uiName = AssetPathUtil.GetUIFormPath(path);
            if (uiCom.IsLoadingUIForm(uiName))
            {
                if (parms != null) GameExtension.VariablePool.ClearVariables(parms.Id);
                return -1;
            }
            parms ??= UIParams.Create();
            return uiCom.OpenUIForm(uiName,"Layer2" , GamePlayDefine.LoadPriority.UI, parms);
        }
    }
}