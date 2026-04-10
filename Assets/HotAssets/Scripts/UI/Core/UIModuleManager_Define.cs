using HotAssets.Scripts.Common;
using HotAssets.Scripts.UI.Module.FightShare;
using HotAssets.Scripts.UI.Module.Login;

namespace HotAssets.Scripts.UI.Core
{
    /// <summary>
    /// UI业务模块管理，用于统一处理模块入口
    /// </summary>
    public partial class UIModuleManager:Singleton<UIModuleManager>
    {
        private void RegisterAll()
        {
            Register(LoginController.Instance);
            Register(FightShareController.Instance);
        }
    }
}
