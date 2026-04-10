using System.IO;
using Builtin.Scripts.Extension;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;

namespace Editor
{
    public static class BuildConst
    {
        public const string SharedAssetBundleName = "SharedAssets";//AssetBundle分包共用资源
        internal static string AssetBundleOutputPath => AssetPathUtil.GetCombinePath(Directory.GetParent(Application.dataPath).FullName, "AB");
        
        public const string HotfixAssembly = "Assets/HotAssets/Scripts/Hotfix.asmdef";
        public const string BuiltinAssembly = "Assets/Builtin/Scripts/Builtin.asmdef";
        
        [ResourceEditorConfigPathAttribute]
        public static string ResourceEditorConfigPath = "Assets/Plugins/UnityGameFramework/Configs/ResourceEditor.xml";
        [ResourceBuilderConfigPathAttribute]
        public static string BuilderEditorConfigPath = "Assets/Plugins/UnityGameFramework/Configs/ResourceBuilder.xml";
        [ResourceCollectionConfigPathAttribute]
        public static string CollectionEditorConfigPath = "Assets/Plugins/UnityGameFramework/Configs/ResourceCollection.xml";
    }
}
