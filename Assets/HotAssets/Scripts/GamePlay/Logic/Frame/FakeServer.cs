using System.Collections.Generic;
using System.Text;
using AAAGame.ScriptsHotfix.GamePlay.Logic.Frame;
using GameFramework;
using HotAssets.Scripts.Common.Event;
using HotAssets.Scripts.GamePlay.Logic.Common;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GamePlay.Logic.Frame
{
    public class FakeServer
    {
        private List<FrameData> _waitSendFrameDatas = new List<FrameData>();
        private const float FrameInterval = 0.1f;
        private fix _frameInterval = FrameInterval;
        StringBuilder _sbTemp = new StringBuilder();
        private readonly AAAGameEventHelper _aaaGameEventHelper = ReferencePool.Acquire<AAAGameEventHelper>();

        public FakeServer()
        {
            _aaaGameEventHelper.SubscribeCommon(GamePlayEvent.ESvrRoomPlayerInfoUpdate,SvrRoomPlayerInfoUpdate);
        }

        public void LogicUpdate(fix deltaTime)
        {
            //if(!GF.Room.IsMaster()) return;
            _frameInterval -= deltaTime;
            if (_frameInterval <= 0.01f)
            {
                ServerSendFrameData();
                _frameInterval = FrameInterval;
            }
        }
        
        private void SvrRoomPlayerInfoUpdate(object sender, GameEvent e)
        {
            /*RelayPlayer relayPlayer = e.GetParam1<RelayPlayer>();
            if (relayPlayer == null) return;

            if (relayPlayer.Properties.TryGetValue(RoomComponent.InputDataKey, out string input))
            {
                if (string.IsNullOrEmpty(input)) return;
                ServerInputResolve(input);
            }*/
        }
        
        /// <summary>
        /// 服务器解析客户端输入，并保存
        /// </summary>
        /// <param name="obj"></param>
        private void ServerInputResolve(string frameDataString)
        {
            /*Log.Info($"ServerInputResolve: {frameDataString}");
            string[] str = frameDataString.Split('|');
            FrameData frameData = FrameData.Create(GF.Room.GetRoomNewFrameIndex());
            frameData.Horizontal = float.Parse(str[0]);
            frameData.Vertica = float.Parse(str[1]);
            _waitSendFrameDatas.Add(frameData);*/
        }
        
        /// <summary>
        /// 服务器等待间隔X秒后,同步帧数据给客户端
        /// </summary>
        /// <param name="obj"></param>
        public void ServerSendFrameData()
        {
            /*if(_waitSendFrameDatas.Count == 0) return;
            _sbTemp.Clear();
            for (int i = 0; i < _waitSendFrameDatas.Count; i++)
            {
                _sbTemp.AppendLine(_waitSendFrameDatas[i].Pack());
                ReferencePool.Release(_waitSendFrameDatas[i]);
            }
            _waitSendFrameDatas.Clear();
            GF.Room.SendRoomInfoUpdate(RoomComponent.UnprocessedFrameDataKey, _sbTemp.ToString());*/
        }
    }
}