using System.Collections.Generic;
using Builtin.Scripts.Game;
using HotAssets.Scripts.Common;
using HotAssets.Scripts.GamePlay;
using HotAssets.Scripts.GamePlay.Logic.Player;
using HotAssets.Scripts.UI;

namespace HotAssets.Scripts.Bridge
{
    /// <summary>
    /// UI向战斗层通信
    /// </summary>
    public class UIToGamePlayerBridge:Singleton<UIToGamePlayerBridge>
    {
        /// <summary>
        /// 单人模式
        /// </summary>
        public void GameStart()
        {
            AppEntry.UI.CloseAllLoadingUIForms();
            AppEntry.UI.CloseAllLoadedUIForms();
           
            List<PlayerData> list = new List<PlayerData>();
            PlayerData playerData = PlayerData.Create(GameExtension.UserId, "测试玩家");
            playerData.HeroIds = new List<int>(){1000001};
            list.Add(playerData);
            
            GamePlayFacade.Instance.Run(false,list,1,cfg.Fight.DungeonType.Main);
        }
        
        /*
        /// <summary>
        /// 房间模式
        /// </summary>
        /// <param name="room"></param>
        public void GameStartByRoom(RelayRoom room)
        {
            GF.UI.CloseAllLoadingUIForms();
            GF.UI.CloseAllLoadedUIForms();

            List<PlayerData> list = new List<PlayerData>();
            foreach (var (key,data) in room.Players)
            {
                PlayerData playerData = PlayerData.Create(data.ID, data.Name);
                playerData.HeroIds = new List<int>(){1000001};
                playerData.Pos = (int)data.TransportId;
                list.Add(playerData);
            }
            
            GamePlayFacade.Instance.Run(true,list,1,cfg.Fight.DungeonType.Main);
        }*/
    }
}