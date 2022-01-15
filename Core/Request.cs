using GcnSharp.Util;

namespace GcnSharp.Core
{
    public class DolphinRecvRequest
    {
        private readonly byte[] _data = new byte[5];
        public byte[] Data { get => _data; }

        private DolphinRecvRequest() {}

        public static DolphinRecvRequest FromUInt(uint value)
        {
            DolphinRecvRequest req = new DolphinRecvRequest();
            byte[] buffer = BitOperation.GetEncodedBytes(value);
            req.Data[0] = Constants.DolphinConstants.JoyRecvMessage[0];
            buffer.CopyTo(req.Data, 1);
            return req;
        }

    }
}