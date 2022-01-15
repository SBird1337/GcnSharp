using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using GcnSharp.Core;

namespace GcnSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            DolphinServer server = new DolphinServer();
            server.StartAccept();
        }
    }
}
