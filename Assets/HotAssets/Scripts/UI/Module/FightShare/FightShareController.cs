using Builtin.Scripts.Game;
using cfg.Fight;
using cfg.UI;
using HotAssets.Scripts.Common.Event;
using HotAssets.Scripts.Extension;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;
using HotAssets.Scripts.UI.Core;
using HotAssets.Scripts.UI.Module.FightMainChapter;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.UI.Module.FightShare
{
    #if ENABLE_OBFUZ
    [Obfuz.ObfuzIgnore]
    #endif
    public class FightShareController : ModuleController<FightShareController>,IController
    {
        public override void RegisterEvent()
        {
            Subscribe(FightShareConst.Event.ECreateRole,ECreateRoleHandler);
            base.RegisterEvent();
        }

        private void ECreateRoleHandler(object sender, GameEvent e)
        {
            RoleUnit roleData = e.GetParam1<RoleUnit>();
            FightShareModel.Instance.AddNewRole(roleData);
        }

        public void EnterFight(DungeonType dungeonType)
        {
            if (FightShareModel.Instance.FightStart)
            {
                Log.Warning("战斗已经开始,不能再次进入：当前 {0}  请求进入 {1}", FightShareModel.Instance.FightDungeonType,dungeonType);
                return;
            }

            FightShareModel.Instance.FightStart = true;
            FightShareModel.Instance.FightDungeonType = dungeonType;
            
            switch (dungeonType)
            {
                case DungeonType.Main:
                    FightMainChapterController.Instance.EnterFight();
                    break;
            }

            AppEntry.UI.OpenUIOverlay(ModuleType.FightCommon, FightShareConst.OverlayFightHudUI);
            AppEntry.UI.OpenUIOverlay(ModuleType.FightCommon, FightShareConst.OverlayFightDamageUI);
        }
    }
}
