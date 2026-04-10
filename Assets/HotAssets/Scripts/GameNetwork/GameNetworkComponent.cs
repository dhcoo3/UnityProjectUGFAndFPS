using System.Net;
using AAAGame.ScriptsHotfix.GameNetwork;
using Builtin.Scripts.Game;
using GameFramework;
using GameProto;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace HotAssets.Scripts.GameNetwork
{
    public class GameNetworkComponent:GameFrameworkComponent
    {
        private GameFramework.Network.INetworkChannel m_Channel = null;

        /// <summary>
        /// 心跳丢失多少次后掉线
        /// </summary>
        private readonly int _missHeartMaxCount = 20;

        /// <summary>
        /// 自动重连间隔,每隔多少秒自动重连1次
        /// </summary>
        private readonly int _autoReconnectInterval = 3;

        /// <summary>
        /// 自动重连总次数
        /// </summary>
        private int _autoReconnectCount = 0;

        /// <summary>
        /// 是否自动重连
        /// </summary>
        [HideInInspector]
        public bool IsOpenAutoReconnect = true;

        /// <summary>
        /// 关闭网络
        /// </summary>
        public bool CloseNetwork = false;

        /// <summary>
        /// 当前连接IP
        /// </summary>
        private string _curLinkIP = "";

        /// <summary>
        /// 当前连接端口
        /// </summary>
        private int _curLinkPort = 0;

        /// <summary>
        /// 当前连接的名字
        /// </summary>
        private string _curNetworkChannelName = "";

       /// <summary>
       /// 发送心跳的时间
       /// </summary>
        private float _sendHeartTime = 0;

        /// <summary>
        /// 接收心跳的时间
        /// </summary>
        private float _receiveHeartTime = 0;     

        /// <summary>
        /// 下一次重连时间
        /// </summary>
        private float _nextLinkTime = 0;

        private GameFrameworkAction<string> _connectCallBackCSharp;
        private GameFrameworkAction _missHeartBeatCallBackCSharp;
        private GameFrameworkAction<string> _closedCallBackCSharp;
        private GameFrameworkAction<int> _autoReconnectCSharp;
               
        [HideInInspector]
        public bool InPurchase = false;

        private bool _showReconnectTips = false;

        private void Update()
        {
            if (CloseNetwork)
            {
                if (m_Channel.Connected)
                {
                    StopNetworkChannel(_curNetworkChannelName);
                    Reconnect();
                }
                else
                {
                    OpenReconnectTips();
                }              
            }
            else
            {
                if (IsOpenAutoReconnect && m_Channel != null && m_Channel.Connected == false && _nextLinkTime != 0)
                {
                    if (Time.time > _nextLinkTime)
                    {
                        _nextLinkTime = 0;
                        _autoReconnectCount++;
                        Connect(_curNetworkChannelName, _curLinkIP, _curLinkPort);

                        if (_autoReconnectCSharp != null)
                        {
                            _autoReconnectCSharp.Invoke(_autoReconnectCount);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 设置连接成功回调（C#）
        /// </summary>
        /// <param name="function"></param>
        public void SetConnectCallBack(GameFrameworkAction<string> function)
        {
            _connectCallBackCSharp = function;
        }

        /// <summary>
        /// 设置心跳掉失回调，每次(C#） 
        /// </summary>
        /// <param name="function"></param>
        public void SetMissHeartBeatCallBack(GameFrameworkAction function)
        {
            _missHeartBeatCallBackCSharp = function;
        }

        /// <summary>
        /// 设置链接关闭回调(C#） 
        /// </summary>
        /// <param name="function"></param>
        public void SetClosedCallBack(GameFrameworkAction<string> function)
        {
            _closedCallBackCSharp = function;
        }

        /// <summary>
        /// 设置自动重连回调，每次（C#）
        /// </summary>
        /// <param name="function"></param>
        public void SetAutoReconnectCallBack(GameFrameworkAction<int> function)
        {
            _autoReconnectCSharp = function;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="networkChannelName">自定义连接渠道名称</param>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口</param>
        public void Connect(string networkChannelName, string ip,int port)
        {
            if (m_Channel is { Connected: true }) return;

            _curNetworkChannelName = networkChannelName;
            _curLinkIP = ip;
            _curLinkPort = port;

            NetworkChannelHelper networkChannelHelper = new NetworkChannelHelper();
            m_Channel = AppEntry.Network.CreateNetworkChannel(networkChannelName, GameFramework.Network.ServiceType.Tcp, networkChannelHelper);
            m_Channel.Connect(IPAddress.Parse(ip), port);
        }

        /// <summary>
        /// 连接成功
        /// </summary>
        public void ConnectSuccess()
        {
            Log.Info("ConnectSuccess");
            _autoReconnectCount = 0;  
            _connectCallBackCSharp?.Invoke(m_Channel.Name);
        }

        /// <summary>
        /// 心跳掉失
        /// </summary>
        public void MissHeartBeat()
        {
            Log.Info("MissHeartBeat:{0} {1}", m_Channel.MissHeartBeatCount, _missHeartMaxCount);

            _missHeartBeatCallBackCSharp?.Invoke();
           
            if (m_Channel.MissHeartBeatCount < _missHeartMaxCount)
            {
                return;
            }

            if(InPurchase)//购买商品时，后台丢失心跳不掉线
            {
                return;
            }

            m_Channel.Close();
           
            IsOpenAutoReconnect = true;
            Reconnect();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Closed()
        {
            Log.Info("Network Closed");

            if (m_Channel != null)
            {
                m_Channel.Close();           
            }            
        }

        /// <summary>
        /// 关闭成功
        /// </summary>
        public void ClosedSuccess()
        {
            Log.Info("ClosedSuccess");  
            _closedCallBackCSharp?.Invoke(m_Channel.Name);
        }

        /// <summary>
        /// 开始重连
        /// </summary>
        public void Reconnect()
        {
            Log.Info("Reconnect Network");
            _nextLinkTime = Time.time + _autoReconnectInterval;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="message"></param>
        public void Send(int messageId,IMessage message)
        {        
            if (m_Channel is { Connected: true })
            {
                if(messageId == 1)
                {
                    _sendHeartTime = Time.time;
                }

                CSHeartBeat pack =  GameFramework.ReferencePool.Acquire<CSHeartBeat>();
                pack.MessageID = messageId;                 
                pack.MessageBytes = message.ToByteArray();
                pack.MessageLenth = pack.MessageBytes.Length;
                Log.Info("CS MessageID:{0}  {1}", messageId, pack.MessageLenth);
                m_Channel.Send(pack);
            }
        }

        /// <summary>
        /// 注册proto回调
        /// </summary>
        /// <param name="messageDescriptor">Proto解析类</param>
        /// <param name="messageId">消息ID</param>
        /// <param name="gameFrameworkAction">回调处理</param>
        public void RegisterProto(MessageDescriptor messageDescriptor,CsMsgType messageId, GameFrameworkAction<IMessage> gameFrameworkAction)
        {
            TcpPacketHandler packetHeader = new TcpPacketHandler((int)messageId,messageDescriptor,gameFrameworkAction);
            m_Channel.RegisterHandler(packetHeader);
        }

        /// <summary>
        /// 关闭当前连接
        /// </summary>
        /// <param name="networkChannelName"></param>
        public void StopNetworkChannel(string networkChannelName)
        {
            if (m_Channel != null && m_Channel.Name == networkChannelName)
            {
                Log.Info("StopNetworkChannel:" + networkChannelName);
                if (m_Channel.Connected)
                {               
                    m_Channel.Close();
                }
            }
        }

        /// <summary>
        /// 是否与服务器连接
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            if(m_Channel != null)
            {
                return m_Channel.Connected;
            }

            return false;
        }

        /// <summary>
        /// 获取链接名称
        /// </summary>
        /// <returns></returns>
        public string GetChannelName()
        {
            if(m_Channel != null)
            {
                return m_Channel.Name;
            }

            return "";
        }
        
        /// <summary>
        /// 获取网络延时，毫秒
        /// </summary>
        /// <returns></returns>
        public float GetNetworkDelay()
        {
            float time = 0;

            if(_sendHeartTime < _receiveHeartTime)
            {
                //正常计算一次心跳来
                time = (_receiveHeartTime*1000)- (_sendHeartTime * 1000);
            }
            else
            {
                //心跳发送后还没有接收到返回                
                if((Time.time*1000) -  (_sendHeartTime*1000) >= 1000)
                {
                    //大于1000毫秒没必要在继续计算
                    time = 1000;
                }
                else
                {
                    time =(Time.time * 1000) - (_sendHeartTime * 1000);
                }
            }

            time = time > 1000 ? 1000 : time;

            return Mathf.Ceil(time);
        }

        /// <summary>
        /// 打开是否重新连接提示
        /// </summary>
        public void OpenReconnectTips() 
        {
            /*if (!m_ShowReconnectTips)
            {
                GF.UI.OpenNetworkAwaitTips(false);
                IsOpenAutoReconnect = false;
                m_ShowReconnectTips = true;

                GF.UI.OpenDialog(new DialogParams
                {
                    Mode = 1,
                    Title = "",
                    Message = GF.Localization.GetString("Network.IsReconnection"),
                    ConfirmText = GF.Localization.GetString("Network.Reconnection"),
                    OnClickConfirm = delegate (object userData) {
                        GF.UI.CloseNativeDialog();
                        GF.UI.OpenNetworkAwaitTips(true);
                        m_ShowReconnectTips = false;
                        IsOpenAutoReconnect = true;
                        Reconnect();
                    },
                    CancelText = GF.Localization.GetString("Game.QuitGame"),
                    OnClickCancel = delegate (object userData) {
                        GF.QuitGame();
                    },
                });
            }   */        
        }    
        
        public void OpenNetworkAwaitTips(bool isShow)
        {
            //GF.UI.OpenNetworkAwaitTips(isShow);
        }
    }
}

