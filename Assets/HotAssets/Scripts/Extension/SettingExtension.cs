using Builtin.Scripts.Const;
using Builtin.Scripts.Game;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.Extension
{
    public static class SettingExtension
    {
        public static GameFramework.Localization.Language GetLanguage(this SettingComponent com)
        {
            string lan = com.GetString(LaunchConst.Setting.Language, string.Empty);
            if (string.IsNullOrEmpty(lan))
            {
                return GameFramework.Localization.Language.Unspecified;
            }

            if (!System.Enum.TryParse(lan, out GameFramework.Localization.Language language))
            {
                language = GameFramework.Localization.Language.English;
            }
            return language;
        }
        
        /// <summary>
        /// 设置语言
        /// </summary>
        /// <param name="com"></param>
        /// <param name="lan"></param>
        public static void SetLanguage(this SettingComponent com, GameFramework.Localization.Language lan, bool saveSetting = true)
        {
            AppEntry.Localization.Language = lan;
            com.SetString(LaunchConst.Setting.Language, lan.ToString());
        }
    }
}