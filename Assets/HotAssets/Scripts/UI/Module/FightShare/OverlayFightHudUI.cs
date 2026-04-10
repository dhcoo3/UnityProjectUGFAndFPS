using System;
using Builtin.Scripts.Extension;
using Builtin.Scripts.Game;
using HotAssets.Scripts.Common;
using HotAssets.Scripts.Common.Event;
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
            Subscribe(FightShareConst.Event.EUpdatePos,EUpdatePosHandler);
            Subscribe(FightShareConst.Event.EUpdateHp,EUpdateHpHandler);
            Subscribe(FightShareConst.Event.ERoleDie,ERoleDieHandler);
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
            catch (Exception e)
            {
                Log.Warning("加载RoleHud失败");
            }
        }
        
        private void EUpdateHpHandler(object sender, GameEvent e)
        {
            RoleUnit roleData = e.GetParam1<RoleUnit>();
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

        private void EUpdatePosHandler(object sender, GameEvent e)
        {
            int id = e.GetParam1<int>();
            Vector3 pos = e.GetParam2<Vector3>();
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
        
        private void ERoleDieHandler(object sender, GameEvent e)
        {
         
        }
    }
}
