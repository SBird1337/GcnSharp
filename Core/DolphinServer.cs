using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using GcnSharp.Logging;
using System.Diagnostics;

namespace GcnSharp.Core
{
    public class DolphinServer
    {
        private readonly IPAddress _localBindAddress = IPAddress.Parse("127.0.0.1");
        private TcpListener _clockListener;
        private TcpListener _dataListener;
        private bool _started;

        private byte[] GetEncodedBytes(uint value)
        {
            byte[] data = BitConverter.GetBytes(value);
            if(!BitConverter.IsLittleEndian)
                Array.Reverse(data);
            return data;
        }
        public DolphinServer()
        {
            _started = false;

            //NOTE: Throws Exception if we cannot establish a TCP listener interface
            //FIXME: Handle Exception, downstream?

            _clockListener = new TcpListener(_localBindAddress, Constants.DolphinConstants.CLOCK_PORT);
            _dataListener = new TcpListener(_localBindAddress, Constants.DolphinConstants.DATA_PORT);
        }

        private void HandleConnection(object args)
        {
            Array argArray = new object[2];
            argArray = (Array)args;
            TcpClient clockClient = argArray.GetValue(0) as TcpClient;
            TcpClient dataClient = argArray.GetValue(1) as TcpClient;
            NetworkStream clockStream = clockClient.GetStream();
            NetworkStream dataStream = dataClient.GetStream();
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while(true)
            {
                //handle command fifo?
                watch.Stop();
                uint elapsedCycles = (uint)(watch.Elapsed.TotalSeconds * Constants.DolphinConstants.AGB_CORE_FREQUENCY);
                watch.Restart();
                clockStream.Write(GetEncodedBytes(elapsedCycles));
                clockStream.Flush();
                dataStream.Write(Constants.DolphinConstants.JoyPollMessage);
                dataStream.Flush();
                byte[] buffer = new byte[3];
                dataStream.Read(buffer, 0, 3);
                // 0 and 1 are used by the dolphin protocol, 2 is REG_JOYSTAT
                byte joystat = buffer[2];
                if((joystat & 0x8) > 0)
                {
                    //client wants to send!
                    watch.Stop();
                    elapsedCycles = (uint)(watch.Elapsed.TotalSeconds * Constants.DolphinConstants.AGB_CORE_FREQUENCY);
                    watch.Restart();
                    clockStream.Write(GetEncodedBytes(elapsedCycles));
                    clockStream.Flush();
                    dataStream.Write(Constants.DolphinConstants.JoyTransMessage);
                    dataStream.Flush();
                    buffer = new byte[5];
                    dataStream.Read(buffer, 0, 5);
                    Logger.Instance.Info(string.Join(",", buffer));
                }
            }
        }

        ///This is a blocking wait for incoming TCP connections on the main thread
        public void StartAccept()
        {
            if(!_started)
            {
                _clockListener.Start();
                _dataListener.Start();
                Logger.Instance.Info("WAITING FOR CONNECTION");
                TcpClient clockClient = _clockListener.AcceptTcpClient();         //FIXME: async?
                Logger.Instance.Info("CLOCK PORT CONNECTED");
                TcpClient dataClient = _dataListener.AcceptTcpClient();           //FIXME: async?
                Logger.Instance.Info("DATA PORT CONNECTED");
                Thread connectionThread = new Thread(new ParameterizedThreadStart(HandleConnection));
                connectionThread.Start(new object[] {clockClient, dataClient});
                while(true)
                {
                    //main thread
                }
                
            }
        }
    }
}