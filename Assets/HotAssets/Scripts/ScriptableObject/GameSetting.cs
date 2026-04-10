using UnityEngine;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Builtin.Scripts.Extension;
using HotAssets.Scripts.Extension;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "GameSetting", menuName = "Tool/GameSetting [配置App运行时所需数据表、配置表、流程]")]
public class GameSetting : ScriptableObject
{
    private static GameSetting mInstance = null;

    [Header("多语言表")]
    [SerializeField] string[] mLanguages;
    public string[] Languages => mLanguages;

    [Header("已启用流程列表")]
    [SerializeField] string[] mProcedures;

    public string[] Procedures => mProcedures;

    private void Awake()
    {
        mInstance = this;
    }


#if UNITY_EDITOR
    /// <summary>
    /// 编辑器下获取实例
    /// </summary>
    /// <returns></returns>
    public static GameSetting GetInstanceEditor()
    {
        if (mInstance == null)
        {
            var configAsset = AssetPathUtil.GetGameSettingAsset("GameSetting");
            mInstance = AssetDatabase.LoadAssetAtPath<GameSetting>(configAsset);
        }
        return mInstance;
    }
#endif
    
    /// <summary>
    /// 运行时获取实例
    /// </summary>
    /// <returns></returns>
    public static async UniTask<GameSetting> GetInstanceSync()
    {
        var configAsset = AssetPathUtil.GetGameSettingAsset("GameSetting");
        if (mInstance == null)
        {
            mInstance = await Builtin.Scripts.Game.AppEntry.Resource.LoadAssetAwait<GameSetting>(configAsset);
        }
        return mInstance;
    }

}