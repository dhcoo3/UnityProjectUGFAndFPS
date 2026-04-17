using Builtin.Scripts.Game;
using cfg.Fight;
using cfg.UI;
using GameFramework.Event;
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
            EventHelper.Subscribe(FightCreateRoleEventArgs.EventId, ECreateRoleHandler);
            base.RegisterEvent();
        }

        /// <summary>
        /// 响应角色创建事件。
        /// </summary>
        private void ECreateRoleHandler(object sender, GameEventArgs e)
        {
            FightCreateRoleEventArgs args = (FightCreateRoleEventArgs)e;
            RoleUnit roleData = args.RoleUnit;
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
