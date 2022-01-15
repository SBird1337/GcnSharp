using System;
using System.IO;
using System.Reflection;
using System.Xml;
using log4net;
using log4net.Config;
using log4net.Repository;
using log4net.Repository.Hierarchy;

namespace GcnSharp.Logging
{
    public sealed class Logger
    {
        private static Logger instance = null;
        private static readonly object instanceLock = new object();
        private readonly ILog _log = LogManager.GetLogger(typeof(Logger));
        private Logger()
        {
            try
            {
                XmlDocument xmlConfig = new XmlDocument();
                using (FileStream fs = File.OpenRead("log4net.conf"))
                {
                    xmlConfig.Load(fs);
                    ILoggerRepository repo = LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(Hierarchy));
                    XmlConfigurator.Configure(repo, xmlConfig["log4net"]);
                    _log.Info("Logger initialized");
                }
            }
            catch(Exception ex)
            {
                _log.Error("Failed to initialize Logger", ex);
            }
        }

        public static Logger Instance
        {
            get
            {
                lock(instanceLock)
                {
                    if(instance == null)
                    {
                        instance = new Logger();
                    }
                    return instance;
                }
            }
        }

        public void Info(string message)
        {
            _log.Info(message);
        }
        
        public void Error(string message, Exception ex)
        {
            _log.Error(message, ex);
        }

        public void Warning(string message)
        {
            _log.Warn(message);
        }
    }
}