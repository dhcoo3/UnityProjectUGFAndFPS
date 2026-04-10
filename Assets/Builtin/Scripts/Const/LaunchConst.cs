namespace Builtin.Scripts.Const
{
    public class LaunchConst
    {
        public static readonly string AotdllDir = "AotDlls";///AOT dll 相对于Resources目录
        public readonly static string AOTDllsKey = "password";//AOT dll加密解密key
        public readonly static string HotFixDLLDir = "HotAssets/HotfixDlls";
        public readonly static string VersionFile = "version.json";
        
        public static class Setting
        {
            /// <summary>
            /// 语言国际化
            /// </summary>
            public static readonly string Language = "Setting.Language";
        }
    }
}