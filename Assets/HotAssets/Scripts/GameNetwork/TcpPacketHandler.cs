//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2020 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Collections.Generic;
using GameFramework;
using GameFramework.Network;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace AAAGame.ScriptsHotfix.GameNetwork
{
    public class TcpPacketHandler : PacketHandlerBase
    {
        private int _id;

        public override int Id
        {
            get
            {
                return _id;
            }

            set
            {
                _id = value;
            }
        }

        private MessageDescriptor _descriptor;
        
        public GameFrameworkAction<IMessage> _callback;

        public TcpPacketHandler(int id,MessageDescriptor descriptor,GameFrameworkAction<IMessage> callback)
        {
            this._id = id;
            _descriptor = descriptor;
            _callback = callback;
        }
        
        public override void Handle(object sender, Packet packet)
        {
            TcpPacketHeader tcpPacketHeader = packet as TcpPacketHeader;
            if (tcpPacketHeader == null) return;
            IMessage message = _descriptor.Parser.ParseFrom(tcpPacketHeader.MessageBytes);
            _callback.Invoke(message);
        }
    }
}
