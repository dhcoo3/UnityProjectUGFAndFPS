using UnityEditor;

namespace Editor.UI.ExUI
{
    public class ExUIConfig
    {   
        static public T LoadAsset<T>(string assetName) where T : UnityEngine.Object
        {
            string[] asset = AssetDatabase.FindAssets(assetName);
            if (asset.Length > 0)
            {
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(asset[0]));
                return obj as T;
            }

            return null;
        }

        /*[MenuItem("Tool/CreateConfig/ExUICreateConfig")]
    public static void CreateUGFUICreateConfig()
    {
        //将对象实例化
        ScriptableObject so = ScriptableObject.CreateInstance(typeof(ExUICreateConfig));

        if (so == null)
        {
            Debug.Log("该对象无效，无法将对象实例化");
            return;
        }
   
        //按指定路径生成配置文件
        AssetDatabase.CreateAsset(so, "Assets/ExUICreateConfig.asset");
    }*/
    }
}
