//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2020 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using AAAGame.ScriptsHotfix.GameNetwork;
using AAAGame.ScriptsHotfix.GameNetwork.MessageHelper;
using Builtin.Scripts.Game;
using GameFramework;
using GameFramework.Event;
using GameFramework.Network;
using HotAssets.Scripts.UI;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GameNetwork
{
    

    public class NetworkChannelHelper : INetworkChannelHelper
    {
        private readonly MemoryStream _cachedStream = new MemoryStream(1024 * 8);
        private INetworkChannel _networkChannel = null;
        
        public int PacketHeaderLength {
            get
            {
                return 8;
            }
        }

        int g_netkey = 126;

        public void Encryption(byte[] pChar, int lenth)
        {
            for(int i=0;i<lenth;++i)
            {
                pChar[i] = (byte)(pChar[i]^g_netkey);
            }  
        }

        public void Decryption(byte[] pChar, int lenth)
        {
            for (int i = 0; i < lenth; ++i)
            {
                pChar[i] = (byte)(pChar[i] ^ g_netkey);
            }
        }

        /// <summary>
        /// 初始化网络频道辅助器。
        /// </summary>
        /// <param name="networkChannel">网络频道。</param>
        public void Initialize(INetworkChannel networkChannel)
        {
            _networkChannel = networkChannel;
            networkChannel.HeartBeatInterval = 5;

            AppEntry.Event.Subscribe(UnityGameFramework.Runtime.NetworkConnectedEventArgs.EventId, OnNetworkConnected);
            AppEntry.Event.Subscribe(UnityGameFramework.Runtime.NetworkClosedEventArgs.EventId, OnNetworkClosed);
            AppEntry.Event.Subscribe(UnityGameFramework.Runtime.NetworkMissHeartBeatEventArgs.EventId, OnNetworkMissHeartBeat);
            AppEntry.Event.Subscribe(UnityGameFramework.Runtime.NetworkErrorEventArgs.EventId, OnNetworkError);
            AppEntry.Event.Subscribe(UnityGameFramework.Runtime.NetworkCustomErrorEventArgs.EventId, OnNetworkCustomError);
        }

        /// <summary>
        /// 关闭并清理网络频道辅助器。
        /// </summary>
        public void Shutdown()
        {
            AppEntry.Event.Unsubscribe(UnityGameFramework.Runtime.NetworkConnectedEventArgs.EventId, OnNetworkConnected);
            AppEntry.Event.Unsubscribe(UnityGameFramework.Runtime.NetworkClosedEventArgs.EventId, OnNetworkClosed);
            AppEntry.Event.Unsubscribe(UnityGameFramework.Runtime.NetworkMissHeartBeatEventArgs.EventId, OnNetworkMissHeartBeat);
            AppEntry.Event.Unsubscribe(UnityGameFramework.Runtime.NetworkErrorEventArgs.EventId, OnNetworkError);
            AppEntry.Event.Unsubscribe(UnityGameFramework.Runtime.NetworkCustomErrorEventArgs.EventId, OnNetworkCustomError);

            _networkChannel = null;
        }

        /// <summary>
        /// 准备进行连接。
        /// </summary>
        public void PrepareForConnecting()
        {
            _networkChannel.Socket.ReceiveBufferSize = 1024 * 64;
            _networkChannel.Socket.SendBufferSize = 1024 * 64;
        }

        /// <summary>
        /// 发送心跳消息包。
        /// </summary>
        /// <returns>是否发送心跳消息包成功。</returns>
        public bool SendHeartBeat()
        {
            //AppEntry.GameNetwork.Send(1, new byte[1]);        
            return true;
        }

        
        private List<byte[]> _byteses = new List<byte[]>() { new byte[4], new byte[4]};
        
        /// <summary>
        /// 序列化消息包。
        /// </summary>
        /// <typeparam name="T">消息包类型。</typeparam>
        /// <param name="packet">要序列化的消息包。</param>
        /// <param name="destination">要序列化的目标流。</param>
        /// <returns>是否序列化成功。</returns>
        public bool Serialize<T>(T packet, Stream destination) where T : Packet
        {                      
            CSHeartBeat csHeartBeat = packet as CSHeartBeat;
            if (csHeartBeat == null) return false;
            
            csHeartBeat.MessageLenth += PacketHeaderLength;
            
            _cachedStream.SetLength(csHeartBeat.MessageLenth);
            _cachedStream.Position = 0;

            this._byteses[0].WriteTo(0, csHeartBeat.MessageID);
            this._byteses[1].WriteTo(0, csHeartBeat.MessageLenth);
              
            int index = 0;

            foreach (var bytes in this._byteses)
            {
                Array.Copy(bytes, 0, _cachedStream.GetBuffer(), index, bytes.Length);
                index += bytes.Length;
            }

            Array.Copy(csHeartBeat.MessageBytes, 0, _cachedStream.GetBuffer(), index, csHeartBeat.MessageBytes.Length);                    

            _cachedStream.WriteTo(destination);
            ReferencePool.Release(packet);
            
            return true;
        }

        /// <summary>
        /// 反序列消息包头。
        /// </summary>
        /// <param name="source">要反序列化的来源流。</param>
        /// <param name="customErrorData">用户自定义错误数据。</param>
        /// <returns></returns>
        public IPacketHeader DeserializePacketHeader(Stream source, out object customErrorData)
        {
            // 注意：此函数并不在主线程调用！      
            customErrorData = null;

            TcpPacketHeader scHeader = ReferencePool.Acquire<TcpPacketHeader>();
            if (source is MemoryStream memoryStream)
            {
                byte[] bytes = memoryStream.GetBuffer();
                scHeader.MessageID = BitConverter.ToInt32(bytes, 0); 
                scHeader.MessageLenth = BitConverter.ToInt32(bytes, 4) - PacketHeaderLength;
                return scHeader;
            }

            return null;
        }

        /// <summary>
        /// 反序列化消息包。
        /// </summary>
        /// <param name="packetHeader">消息包头。</param>
        /// <param name="source">要反序列化的来源流。</param>
        /// <param name="customErrorData">用户自定义错误数据。</param>
        /// <returns>反序列化后的消息包。</returns>
        public Packet DeserializePacket(IPacketHeader packetHeader, Stream source, out object customErrorData)
        {
            // 注意：此函数并不在主线程调用！
            customErrorData = null;

            if (packetHeader is not TcpPacketHeader scPacketHeader)
            {
                Log.Warning("Packet header is invalid.");
                return null;
            }
            byte[] tmpByte = new byte[scPacketHeader.PacketLength];
            var read = source.Read(tmpByte, 0,scPacketHeader.PacketLength);
            scPacketHeader.MessageBytes = tmpByte;  
            return scPacketHeader;
        }
       
        private void OnNetworkConnected(object sender, GameEventArgs e)
        {
            UnityGameFramework.Runtime.NetworkConnectedEventArgs ne = (UnityGameFramework.Runtime.NetworkConnectedEventArgs)e;
            if (ne.NetworkChannel != _networkChannel)
            {
                return;
            }

            GameExtension.GameNetwork.ConnectSuccess();
            Log.Info("Network channel '{0}' connected, local address '{1}', remote address '{2}'.", ne.NetworkChannel.Name, ne.NetworkChannel.Socket.LocalEndPoint.ToString(), ne.NetworkChannel.Socket.RemoteEndPoint.ToString());
        }

        private void OnNetworkClosed(object sender, GameEventArgs e)
        {
            UnityGameFramework.Runtime.NetworkClosedEventArgs ne = (UnityGameFramework.Runtime.NetworkClosedEventArgs)e;
            if (ne.NetworkChannel != _networkChannel)
            {
                return;
            }
            Log.Info("Network channel '{0}' closed.", ne.NetworkChannel.Name);


            GameExtension.GameNetwork.ClosedSuccess();            
        }

        private void OnNetworkMissHeartBeat(object sender, GameEventArgs e)
        {
            UnityGameFramework.Runtime.NetworkMissHeartBeatEventArgs ne = (UnityGameFramework.Runtime.NetworkMissHeartBeatEventArgs)e;
            if (ne.NetworkChannel != _networkChannel)
            {
                return;
            }

            Log.Info("Network channel '{0}' miss heart beat '{1}' times.", ne.NetworkChannel.Name, ne.MissCount.ToString());
            GameExtension.GameNetwork.MissHeartBeat();
        }

        private void OnNetworkError(object sender, GameEventArgs e)
        {
            UnityGameFramework.Runtime.NetworkErrorEventArgs ne = (UnityGameFramework.Runtime.NetworkErrorEventArgs)e;
            if (ne.NetworkChannel != _networkChannel)
            {
                return;
            }

            Log.Info("Network channel '{0}' error, error code is '{1}', error message is '{2}'.", ne.NetworkChannel.Name, ne.ErrorCode.ToString(), ne.ErrorMessage);

            GameExtension.GameNetwork.Reconnect();
        }

        private void OnNetworkCustomError(object sender, GameEventArgs e)
        {
            UnityGameFramework.Runtime.NetworkCustomErrorEventArgs ne = (UnityGameFramework.Runtime.NetworkCustomErrorEventArgs)e;
            if (ne.NetworkChannel != _networkChannel)
            {
                return;
            }
        }
    }
}
