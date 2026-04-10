using cfg.UI;
using HotAssets.Scripts.Bridge;
using HotAssets.Scripts.UI.Core;

namespace HotAssets.Scripts.UI.Module.FightMainChapter
{
    #if ENABLE_OBFUZ
    [Obfuz.ObfuzIgnore]
    #endif
    public class FightMainChapterController : ModuleController<FightMainChapterController>,IController
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

        }

        public void InitFightFSM()
        {
           
        }
    }
}
