using System.Collections.Generic;
using Reborn.Utils;

namespace Server.Auth.Net
{
    public class Packet
    {
        public Packet(short header)
        {
            Header = header;
            Payload = new List<byte>();
        }

        public short Header { get; }

        private List<byte> Payload { get; }

        public Packet Append(byte value)
        {
            Payload.Add(value);

            return this;
        }

        public Packet Append(IEnumerable<byte> value)
        {
            Payload.AddRange(value);

            return this;
        }

        public byte[] GetPacket()
        {
            var finalPacket = new List<byte>();

            finalPacket.AddRange(EncodeHelper.EncodeShort((short) (Payload.Count + 8), true));      // Packet length.
            finalPacket.AddRange(EncodeHelper.EncodeInteger(5, true));                              // Unknown integer.
            finalPacket.AddRange(EncodeHelper.EncodeShort(Header, true));                           // Packet header.
            finalPacket.AddRange(Payload);

            return finalPacket.ToArray();
        }
    }
}
