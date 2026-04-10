using System.IO;
using AAAGame.ScriptsHotfix.GamePlay.Logic.Frame;
using HotAssets.Scripts.Common.Event;
using HotAssets.Scripts.GamePlay.Logic.Common;
using HotAssets.Scripts.GamePlay.Logic.GameInput;
using HotAssets.Scripts.GamePlay.Logic.ProxyManager;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GamePlay.Logic.Frame
{
    public class FrameProxy:GameProxy
    {
        /// <summary>
        /// 如果是本地模式，不需要同步
        /// </summary>
        public bool IsLocal => _isLocal;
        
        private bool _isLocal = true;
        
        private FakeServer _server;
        private InputProxy _inputProxy;
        private FakeServer _fakeServer;
        public override void Initialize()
        {
            _isLocal = !GamePlayFacade.Instance.IsRoomMode;
            _inputProxy = GetProxy<InputProxy>();
            _fakeServer = new FakeServer();
            //Subscribe(GamePlayEvent.EUpdateFrame,SvrRoomInfoUpdate);
        }

        public override void LogicUpdate(fix deltaTime)
        {
            if (!_isLocal)
            {
                _fakeServer?.LogicUpdate(deltaTime);
            }
           
            base.LogicUpdate(deltaTime);
        }

        private void SvrRoomInfoUpdate(object sender, GameEvent e)
        {
           string frameData = e.GetParam1<string>();
           if(string.IsNullOrEmpty(frameData)) return;
           FrameDataResolve(frameData);
        }

        private void FrameDataResolve(string data)
        {
            using (StringReader reader = new StringReader(data))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        Log.Info($"FrameDataResolve: {line}");
                        _inputProxy.AddInputObj(FrameData.UnPack(line));
                    }
                }
            }
        }
    }
}