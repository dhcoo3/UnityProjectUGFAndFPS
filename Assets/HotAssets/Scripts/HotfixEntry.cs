using GameFramework;
using GameFramework.Fsm;
using GameFramework.Procedure;
using System;
using HotAssets.Scripts.Extension;
using HotAssets.Scripts.Procedures;
using UnityGameFramework.Runtime;
/// <summary>
/// 热更逻辑入口
/// </summary>
public class HotfixEntry
{
    public static async void StartHotfixLogic(bool enableHotfix)
    {
        Log.Info("进入热更逻辑脚本");
        Log.Info<bool>("Hotfix Enable:{0}", enableHotfix);
        ResourceExtension.SubscribeEvent();

        Builtin.Scripts.Game.AppEntry.Fsm.DestroyFsm<IProcedureManager>();
        var fsmManager = GameFrameworkEntry.GetModule<IFsmManager>();
        var procManager = GameFrameworkEntry.GetModule<IProcedureManager>();
        var appConfig = await GameSetting.GetInstanceSync();
        
        ProcedureBase[] procedures = new ProcedureBase[appConfig.Procedures.Length];
        if (appConfig.Procedures.Length == 0)
        {
            Log.Error("没有流程，请添加流程后再初始化状态机");
            return;
        }
        
        for (int i = 0; i < appConfig.Procedures.Length; i++)
        {
            procedures[i] = Activator.CreateInstance(Type.GetType(appConfig.Procedures[i])) as ProcedureBase;
        }
        procManager.Initialize(fsmManager, procedures);
        procManager.StartProcedure<PreloadProcedure>();
    }
}
