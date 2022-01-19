namespace GcnSharp.Core
{
    public class DolphinTransResponse
    {
        private byte[] _transData = new byte[4];
        public byte RegJoyStat {get; set; }
        public byte[] TransData { get => _transData; }

        public DolphinTransResponse(byte joystat, byte v0, byte v1, byte v2, byte v3)
        {
            RegJoyStat = joystat;
            _transData[0] = v0;
            _transData[1] = v1;
            _transData[2] = v2;
            _transData[3] = v3;
        }
    }
}