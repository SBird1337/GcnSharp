using System;

namespace GcnSharp.Util
{
    public static class BitOperation
    {
        public static byte[] GetEncodedBytes(uint value)
        {
            byte[] data = BitConverter.GetBytes(value);
            if(!BitConverter.IsLittleEndian)
                Array.Reverse(data);
            return data;
        }

        public static uint GetDecodedUInt(byte[] encoded)
        {
            byte[] copy = new byte[4];
            encoded.CopyTo(copy, 0);
            if(!BitConverter.IsLittleEndian)
            {
                Array.Reverse(copy);
            }
            return BitConverter.ToUInt32(copy);
        }
    }
}