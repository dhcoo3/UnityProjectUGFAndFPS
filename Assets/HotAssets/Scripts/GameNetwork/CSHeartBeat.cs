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
    //[Serializable, ProtoContract(Name = @"CSHeartBeat")]
    public class CSHeartBeat :Packet, IPacketHeader
    {
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
            MessageID = 0;
            MessageLenth = 0;
            MessageBytes = null;
        }
    }
}
