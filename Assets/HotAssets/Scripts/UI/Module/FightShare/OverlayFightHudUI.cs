using System;
using Builtin.Scripts.Extension;
using Builtin.Scripts.Game;
using GameFramework.Event;
using HotAssets.Scripts.Common;
using HotAssets.Scripts.Extension;
using HotAssets.Scripts.GamePlay.Logic.Unit.Role;
using HotAssets.Scripts.UI.Tool.Component;
using TuanjieAI.Assistant.Schema;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.UI.Module.FightShare
{
    #if ENABLE_OBFUZ
    [Obfuz.ObfuzIgnore]
    #endif
    public class OverlayFightHudUI : ExPanel
    {
        #region Auto Create
        #endregion
        
        protected override void RegisterUI()
        {
            #region Auto Bind
            #endregion
        }

        private GameObject RoleHudPrefab;
        private FightShareModel _fighShareModel;
        
        protected override void RegisterEvent()
        {
            EventHelper.Subscribe(FightUpdatePosEventArgs.EventId, EUpdatePosHandler);
            EventHelper.Subscribe(FightUpdateHpEventArgs.EventId, EUpdateHpHandler);
            EventHelper.Subscribe(FightRoleDieEventArgs.EventId, ERoleDieHandler);
        }

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
        }

        protected override void OnOpen(object userData)
        {
            _fighShareModel = FightShareModel.Instance;
            LoadRoleHudUI();
            base.OnOpen(userData);
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            AppEntry.Resource.UnloadAsset(RoleHudPrefab);
            base.OnClose(isShutdown, userData);
        }

        private async void LoadRoleHudUI()
        {
            try
            {
                if (RoleHudPrefab == null)
                {
                    string roleHudPath = AssetPathUtil.GetUIFormPath("FightCommon/RoleHud");
                    var asset = await AppEntry.Resource.LoadAssetAwait<GameObject>(roleHudPath);
                    RoleHudPrefab = asset;
                }
            }
            catch (Exception)
            {
                Log.Warning("加载RoleHud失败");
            }
        }
        
        /// <summary>
        /// 响应血量刷新事件。
        /// </summary>
        private void EUpdateHpHandler(object sender, GameEventArgs e)
        {
            FightUpdateHpEventArgs args = (FightUpdateHpEventArgs)e;
            RoleUnit roleData = args.RoleUnit;
            if (_fighShareModel.FightUiRoles.TryGetValue(roleData.RoleId, out FightUIRole fightUIRole))
            {
                if (fightUIRole.Hud == null)
                {
                    fightUIRole.Hud = new FightUIRoleHud(fightUIRole);
                    fightUIRole.Hud.RegisterUI(GameObject.Instantiate(RoleHudPrefab,transform));
                }

                fightUIRole.Hud.UpdateHudHp(roleData);
            }
        }

        /// <summary>
        /// 响应位置刷新事件。
        /// </summary>
        private void EUpdatePosHandler(object sender, GameEventArgs e)
        {
            FightUpdatePosEventArgs args = (FightUpdatePosEventArgs)e;
            int id = args.RoleId;
            Vector3 pos = args.Position;
            if (_fighShareModel.FightUiRoles.TryGetValue(id, out FightUIRole fightUIRole))
            {
                if (fightUIRole.Hud == null)
                {
                    fightUIRole.Hud = new FightUIRoleHud(fightUIRole);
                    fightUIRole.Hud.RegisterUI(GameObject.Instantiate(RoleHudPrefab,transform));
                }

                Vector2 uiPos = Utils.WorldToUIPoint(pos);
                fightUIRole.Hud.UpdateHudPos(uiPos.x, uiPos.y + 100);
            }
        }
        
        /// <summary>
        /// 响应角色死亡事件。
        /// </summary>
        private void ERoleDieHandler(object sender, GameEventArgs e)
        {
         
        }
    }
}
