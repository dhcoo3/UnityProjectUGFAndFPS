using Builtin.Scripts.Extension;
using Builtin.Scripts.Game;
using cfg.UI;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using GameFramework;
using HotAssets.Scripts.Common;
using HotAssets.Scripts.Extension;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.UI.Core
{
    /// <summary>
    /// UI业务模块管理，用于统一处理模块入口
    /// </summary>
    public partial class UIModuleManager:Singleton<UIModuleManager>
    {
        private TbGameModule _tbGameModule;
        
        private readonly GameFrameworkLinkedList<IController> m_ModuleControllers = new GameFrameworkLinkedList<IController>();

        private readonly GameFrameworkLinkedList<IModel> m_ModuleModles = new GameFrameworkLinkedList<IModel>();
    
        private readonly GameFrameworkLinkedList<IView> m_ModuleViews = new GameFrameworkLinkedList<IView>();
        
    
        public void Register(IModel moduleModel)
        {
            m_ModuleModles.AddLast(moduleModel);
        }
    
        public void Register(IController moduleController)
        {
            m_ModuleControllers.AddLast(moduleController);
        }
    
        public void Register(IView moduleView)
        {
            m_ModuleViews.AddLast(moduleView);
        }

        public async UniTask RegisterModule()
        {
            _tbGameModule = await AppEntry.DataTable.GetDataTableLuBan<TbGameModule>(cfg.Tables.ui_tbgamemodule);
            RegisterAll();
            InitGameModule();
        }

        public void InitGameModule()
        {
            //需要提前 协议、事件 注册
            foreach (var moduleController in m_ModuleControllers)
            {
                moduleController.RegisterProto();
                moduleController.RegisterEvent();
            }
        }
    
        public void GoToModule(ModuleType uiModule, UIParams parms = null)
        {
            GameModule uiGameModule = _tbGameModule.GetOrDefault(uiModule);
        
            if (uiGameModule == null)
            {
                Log.Warning("不存在模块定义 {0}",uiModule.ToString());
                return;
            }

            if (!CheckModuleOpenCondition(uiModule))
            {
                Log.Warning("模块入口不满足开启条件");
                return;
            }

            if (IsOpenModule(uiModule))
            {
                Log.Warning("模块入口面板已打开");
                return;
            }
         
            AppEntry.UI.OpenUIForm(uiGameModule, parms);
        }

        public bool IsOpenModule(ModuleType uiModule)
        {
            GameModule uiGameModule = _tbGameModule.GetOrDefault(uiModule);
            if (uiGameModule == null)
            {
                Log.Warning("不存在模块定义 {0}",uiModule.ToString());
                return false;
            }
            
            string path = ZString.Format("{0}/{1}", uiGameModule.Id.ToString(), uiGameModule.Entry);
            string uiName = AssetPathUtil.GetUIFormPath(path);
            if (AppEntry.UI.HasUIForm(uiName) || AppEntry.UI.IsLoadingUIForm(uiName))
            {
                return true;
            }
        
            return false;
        }

        /// <summary>
        /// 检查模块开启条件
        /// </summary>
        /// <param name="uiModule"></param>
        /// <returns></returns>
        public bool CheckModuleOpenCondition(ModuleType uiModule)
        {
            return true;
        }
    }
}
