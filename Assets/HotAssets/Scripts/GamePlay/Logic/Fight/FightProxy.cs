using System.Collections.Generic;
using HotAssets.Scripts.GamePlay;
using HotAssets.Scripts.GamePlay.Logic.Common;
using HotAssets.Scripts.GamePlay.Logic.Dungeon;
using HotAssets.Scripts.GamePlay.Logic.Dungeon.Main;
using HotAssets.Scripts.GamePlay.Logic.Player;
using HotAssets.Scripts.GamePlay.Logic.ProxyManager;
using HotAssets.Scripts.GamePlay.Logic.Unit;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GamePlay.Logic.Fight
{
    public class FightProxy : GameProxy
    {
        private FightLoadingProxy _fightLoadingProxy;
        private IDungeon _currentDungeon;
        private PlayerProxy _currentPlayer;
        private bool _fightEnded;

        public override void Initialize()
        {
            _fightLoadingProxy = GetProxy<FightLoadingProxy>();
            _currentPlayer = GetProxy<PlayerProxy>();
            _fightEnded = false;
        }

        public void EnterFight(List<PlayerData> players, int characterId, cfg.Fight.DungeonType dungeonType)
        {
            _fightEnded = false;
            _currentDungeon = dungeonType switch
            {
                cfg.Fight.DungeonType.Main => GetProxy<MainDungeonProxy>(),
                _ => null
            };

            if (_currentDungeon == null)
            {
                Log.Error("不存在副本类型");
                return;
            }

            _currentPlayer.IniPlayers(players);
            _currentDungeon.InitDungeon(characterId);
        }

        public void EndFight()
        {
            EndFight(true);
        }

        public void EndFight(bool isWin)
        {
            if (_fightEnded)
            {
                return;
            }

            _fightEnded = true;
            Fire(GamePlayEvent.EGameOver, isWin);
            GamePlayFacade.Instance.End();
        }

        public void ExitFight()
        {
            GamePlayFacade.Instance.End();
            _currentDungeon = null;
            _currentPlayer = null;
            _fightEnded = false;
        }
    }
}
