using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using GcnSharp.Logging;
using System.Diagnostics;
using GcnSharp.Util;

namespace GcnSharp.Core
{
    public class DolphinClientRequestEventArgs : EventArgs
    {
        public NetworkStream ClockStream {get; set; }
        public NetworkStream DataStream {get; set; }
        public Stopwatch Watch {get; set; }
        public uint Metadata {get; set; }
    }
    public class DolphinServer
    {
        private readonly IPAddress _localBindAddress = IPAddress.Parse("127.0.0.1");
        private TcpListener _clockListener;
        private TcpListener _dataListener;
        private bool _started;
        public event EventHandler<DolphinClientRequestEventArgs> DolphinClientRequest;
        public DolphinServer()
        {
            _started = false;

            //NOTE: Throws Exception if we cannot establish a TCP listener interface
            //FIXME: Handle Exception, downstream?

            _clockListener = new TcpListener(_localBindAddress, Constants.DolphinConstants.CLOCK_PORT);
            _dataListener = new TcpListener(_localBindAddress, Constants.DolphinConstants.DATA_PORT);
        }

        private byte[] GetTimeSlice(Stopwatch sw)
        {
            sw.Stop();
            uint elapsedCycles = (uint)(sw.Elapsed.TotalSeconds * Constants.DolphinConstants.AGB_CORE_FREQUENCY);
            sw.Restart();
            return BitOperation.GetEncodedBytes(elapsedCycles);
        }

        ///Polls the client and returns JOYSTAT
        public byte PollRequest(Stopwatch sw, NetworkStream clockStream, NetworkStream dataStream)
        {
            clockStream.Write(GetTimeSlice(sw));
            clockStream.Flush();
            dataStream.Write(Constants.DolphinConstants.JoyPollMessage);
            dataStream.Flush();
            byte[] buffer = new byte[3];
            dataStream.Read(buffer, 0, 3);
            return buffer[2];
        }

        public DolphinTransResponse TransRequest(Stopwatch sw, NetworkStream clockStream, NetworkStream dataStream)
        {
            clockStream.Write(GetTimeSlice(sw));
            clockStream.Flush();
            dataStream.Write(Constants.DolphinConstants.JoyTransMessage);
            dataStream.Flush();
            byte[] buffer = new byte[5];
            dataStream.Read(buffer, 0, 5);
            DolphinTransResponse response = new DolphinTransResponse(buffer[4], buffer[0], buffer[1], buffer[2], buffer[3]);
            return response;
        }

        /// Sends a JOY_RECV request to the GBA, receives JOYSTAT
        public byte RecvRequest(Stopwatch sw, NetworkStream clcokStream, NetworkStream dataStream, uint dataWord)
        {
            clcokStream.Write(GetTimeSlice(sw));
            clcokStream.Flush();
            DolphinRecvRequest req = DolphinRecvRequest.FromUInt(dataWord);
            dataStream.Write(req.Data);
            dataStream.Flush();
            byte[] buffer = new byte[1];
            dataStream.Read(buffer, 0, 1);
            return buffer[0];
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
                byte joystat = PollRequest(watch, clockStream, dataStream);
                if((joystat & 0x8) > 0)
                {
                    //client wants a conversation, get metadata
                    DolphinTransResponse trans = TransRequest(watch, clockStream, dataStream);
                    uint metadata = BitOperation.GetDecodedUInt(trans.TransData);
                    DolphinClientRequestEventArgs eventArgs = new DolphinClientRequestEventArgs();
                    eventArgs.ClockStream = clockStream;
                    eventArgs.DataStream = dataStream;
                    eventArgs.Metadata = metadata;
                    eventArgs.Watch = watch;
                    OnDolphinClientRequest(eventArgs);
                }
            }
        }

        private void OnDolphinClientRequest(DolphinClientRequestEventArgs e)
        {
            EventHandler<DolphinClientRequestEventArgs> handler = DolphinClientRequest;
            if(handler != null)
            {
                handler(this, e);
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