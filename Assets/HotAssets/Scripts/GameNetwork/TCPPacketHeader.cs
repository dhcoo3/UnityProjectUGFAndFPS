//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2020 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using GameFramework.Network;

namespace AAAGame.ScriptsHotfix.GameNetwork
{
    public class TcpPacketHeader : Packet, IPacketHeader
    {
        public TcpPacketHeader()
        {
            MessageBytes = new byte[1024 * 8];
        }

        public int PacketLength
        {
            get
            {
                return MessageLenth;
            }
        }

        public override int Id
        {
            get
            {
                return MessageID;
            }
        }

        public Int32 MessageID;
        public Int32 MessageLenth;
        public byte[] MessageBytes;

        public override void Clear()
        {
            MessageBytes = new byte[1024 * 8];
        }
    }
}
