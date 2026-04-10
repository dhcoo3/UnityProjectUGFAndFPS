using GameProto;
using HotAssets.Scripts.UI.Core;

namespace HotAssets.Scripts.UI.Module.Login
{
    public class LoginModel: ModuleModel<LoginModel>,IModel
    {
        public CsRoleLoginRes LoginData { get; set; }
        
        

    }
}