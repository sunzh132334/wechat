using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Wechat.Util.Log
{
    public class Logger
    {        

        static Logger()
        {       
            string path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "log4net.config");
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(path));
        
        }

        public static ILog GetLog<T>()
        {
            ILog log = LogManager.GetLogger(typeof(T));
            return log;
        }
       


    }
}
