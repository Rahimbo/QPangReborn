namespace Reborn.Utils
{
    public class EncodeHelper
    {
        public static byte[] EncodeShort(short s, bool littleEndian = false)
        {
            var a = (byte)((s >> 8) & 0xFF);
            var b = (byte)(s & 0xFF);

            return littleEndian ? 
                new[] { b, a } : 
                new[] { a, b };
        }

        public static short DecodeShort(byte[] b, bool littleEndian = false)
        {
            if ((b[0] | b[1]) < 0)
                return -1;
            return littleEndian ?
                (short)((b[1] << 8) + (b[0] << 0)) :
                (short)((b[0] << 8) + (b[1] << 0));
        }

        public static byte[] EncodeInteger(int i, bool littleEndian = false)
        {
            var a = (byte)((i >> 24) & 0xFF);
            var b = (byte)((i >> 16) & 0xFF);
            var c = (byte)((i >> 8) & 0xFF);
            var d = (byte)(i & 0xFF);

            return littleEndian ?
                new []{ d, c, b, a } :
                new[] { a, b, c, d };
        }

        public static int DecodeInteger(byte[] b, bool littleEndian = false)
        {
            if ((b[0] | b[1] | b[2] | b[3]) < 0)
                return -1;

            return littleEndian ?
                (b[3] << 24) + (b[2] << 16) + (b[1] << 8) + (b[0] << 0) : 
                (b[0] << 24) + (b[1] << 16) + (b[2] << 8) + (b[3] << 0);
        }

        public static byte[] CreatePadding(int amount)
        {
            byte[] paddingBytes = new byte[amount];
            for (int i = 0; i < amount; i++)
            {
                paddingBytes[i] = 0x00;
            }

            return paddingBytes;
        }
    }
}
