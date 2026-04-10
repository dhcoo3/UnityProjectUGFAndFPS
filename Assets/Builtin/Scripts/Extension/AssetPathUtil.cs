using GameFramework;

namespace Builtin.Scripts.Extension
{
    public static class AssetPathUtil
    {
        public static string GetCombinePath(params string[] args)
        {
            return Utility.Path.GetRegularPath(System.IO.Path.Combine(args));
        }
        
        public static string GetHotfixDll(string dllName)
        {
            return Utility.Text.Format("Assets/HotAssets/HotfixDlls/{0}.bytes", dllName);
        }
        
        public static string GetEntity(string v)
        {
            return Utility.Text.Format("Assets/HotAssets/Entity/{0}.prefab", v);
        }
        
        public static string GetScenePath(string name)
        {
            return Utility.Text.Format("Assets/HotAssets/Scene/SceneAsset/{0}.scene", name);
        }
        
        public static string GetGameSettingAsset(string v)
        {
            return Utility.Text.Format("Assets/HotAssets/Setting/{0}.asset", v);
        }
        
        public static string GetDataTablePath(string name, bool useBytes)
        {
            return Utility.Text.Format("Assets/HotAssets/DataTable/{0}.{1}", name, useBytes ? "bytes": "json");
        }
        
        public static string GetUIFormPath(string v)
        {
            return Utility.Text.Format("Assets/HotAssets/UI/Prefabs/{0}.prefab", v);
        }
        
        public static string GetSceneCollisionData(string name, bool useBytes)
        {
            return Utility.Text.Format("Assets/HotAssets/Scene/SceneAsset/Map_{0}.{1}", name, useBytes ? "bytes": "json");
        }
        
        public static string GetEffect(string name)
        {
            return Utility.Text.Format("Assets/HotAssets/Effect/Prefabs/{0}.prefab", name);
        }

        public static string GetBullet(string name)
        {
            return Utility.Text.Format("Bullet/Prefabs/{0}", name);
        }
        
        public static string GetSoundPath(string name)
        {
            return Utility.Text.Format("Assets/HotAssets/Sound/{0}.wav", name);
        }
    }
}