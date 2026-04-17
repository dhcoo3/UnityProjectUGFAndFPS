using System.Collections.Generic;
using HotAssets.Scripts.Common;
using HotAssets.Scripts.GamePlay.Logic.Fight;
using HotAssets.Scripts.GamePlay.Logic.GameInput;
using HotAssets.Scripts.GamePlay.Logic.Player;
using HotAssets.Scripts.GamePlay.Logic.ProxyManager;
using HotAssets.Scripts.GamePlay.Render.RenderManager;
using HotAssets.Scripts.UI;

namespace HotAssets.Scripts.GamePlay
{
    public class GamePlayFacade:Singleton<GamePlayFacade>
    {
        private FixRandom _fixRandom;
        public int CallRandomTimes { get; private set; }

        public bool IsRoomMode;

        private InputCollector _inputCollector;
        
        /// <summary>
        /// 玩法总入口
        /// </summary>
        /// <param name="isRoom">是否为房间模式</param>
        /// <param name="players">参与的玩家数据，包含装备、属性等</param>
        /// <param name="characterId">要参与的关卡ID</param>
        /// <param name="dungeonType">玩法类型</param>
        public void Run(bool isRoom,List<PlayerData> players,int characterId,cfg.Fight.DungeonType dungeonType)
        {
            IsRoomMode = isRoom;
            
            SetRandomSeed();
            
            GameExtension.UpdateActions.Add(Update);
            
            GameProxyManger.Instance.Register(dungeonType);
            GameRenderManager.Instance.Register();
            
            GameProxyManger.Instance.Initialize();
            GameRenderManager.Instance.Initialize();

            _inputCollector = new InputCollector();
            _inputCollector.Initialize();

            FightProxy fightProxy = GameProxyManger.Instance.GetProxy<FightProxy>();
            fightProxy.EnterFight(players, characterId, dungeonType);
        }

        public void End()
        {
            if (_inputCollector != null)
            {
                _inputCollector.Clear();
                _inputCollector = null;
            }

            if (GameProxyManger.Instance != null)
            {
                GameProxyManger.Instance.Clear();
            }

            if (GameRenderManager.Instance != null)
            {
                GameRenderManager.Instance.Clear();
            }

            GameExtension.UpdateActions.Remove(Update);
        }
       
        public void Update(float fixedDeltaTime)
        {
            // 第0步：采集原始输入，确保瞬时按键（GetKeyDown）在逻辑帧同帧消费
            _inputCollector.Collect();
            //更新逻辑
            GameProxyManger.Instance.LogicUpdate(fixedDeltaTime);
            //更新渲染
            GameRenderManager.Instance.LogicUpdate(fixedDeltaTime);
        }
        
        public void SetRandomSeed()
        {
            int seed = 0;
            
            if (IsRoomMode)
            {
                //seed = GF.Room.GetRandomSeed();
            }
            else
            {
                seed = UnityEngine.Random.Range(10000, int.MaxValue);
            }
                
            if (_fixRandom == null)
            {
                _fixRandom = new FixRandom(seed);
            }
            else
            {
                _fixRandom.Reinitialise(seed);
            }
        }

        public FixRandom Random
        {
            get
            {
                CallRandomTimes++;
                //Debug.Log(" =====================frameIndex: " +  _frameIndex + " CallRandomTimes: " + CallRandomTimes);
                return _fixRandom;
            }
        }

    }
}
