using cfg.Fight;
using cfg.UI;
using HotAssets.Scripts.Bridge;
using HotAssets.Scripts.UI.Core;
using HotAssets.Scripts.UI.Module.FightShare;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.UI.Module.FightMainChapter
{
    #if ENABLE_OBFUZ
    [Obfuz.ObfuzIgnore]
    #endif
    public class FightMainChapterController : ModuleController<FightMainChapterController>, IController
    {
        public override void RegisterProto()
        {
        }

        public override void RegisterEvent()
        {
        
        }

        public void EnterFight()
        {
           UIToGamePlayerBridge.Instance.GameStart();
           UIModuleManager.Instance.GoToModule(ModuleType.FightMainChapter);
        }

        public void ExitFight()
        {
            ResetFightState();
        }

        public void InitFightFSM()
        {
            ResetFightState();
        }

        private void ResetFightState()
        {
            FightShareModel.Instance.FightStart = false;
            FightShareModel.Instance.FightDungeonType = DungeonType.Main;
            FightShareModel.Instance.FightUiRoles.Clear();
            Log.Info("战斗章节UI状态已重置");
        }
    }
}
