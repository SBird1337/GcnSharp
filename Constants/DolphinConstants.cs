

using System;

namespace GcnSharp.Constants
{
    public static class DolphinConstants
    {
        public const Int32 CLOCK_PORT = 49420;
        public const Int32 DATA_PORT = 54970;
        public const uint AGB_CORE_FREQUENCY = 16777216;

        public enum JoyCommand : byte
        {
            JOY_POLL = 0x00,
            JOY_TRANS = 0x14,
            JOY_RECV = 0x15,
            JOY_RESET = 0xFF
        }

        private static readonly byte[] _joyPollMessage = {(byte)JoyCommand.JOY_POLL};
        private static readonly byte[] _joyTransMessage = {(byte)JoyCommand.JOY_TRANS};
        private static readonly byte[] _joyRecvMessage = {(byte)JoyCommand.JOY_RECV};
        private static readonly byte[] _joyResetMessage = {(byte)JoyCommand.JOY_RESET};

        public static byte[] JoyPollMessage { get => _joyPollMessage; }
        public static byte[] JoyTransMessage { get => _joyTransMessage; }
        public static byte[] JoyRecvMessage { get => _joyRecvMessage; }
        public static byte[] JoyResetMessage { get => _joyResetMessage; }

    }
}