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
        private static Logger _Singleton = null;
        private static object Singleton_Lock = new object();
        private static string path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Log");
        private static string[] errTpyes = { "Debug", "Info", "Warn", "Error" };
        static Logger()
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            foreach (var item in errTpyes)
            {
                var typePath = Path.Combine(path, item);
                if (!Directory.Exists(typePath))
                {
                    Directory.CreateDirectory(typePath);
                }
            }
        }

        private Logger()
        {
        }

        public static Logger CreateInstance()
        {
            if (_Singleton == null) //双if +lock
            {
                lock (Singleton_Lock)
                {
                    if (_Singleton == null)
                    {
                        _Singleton = new Logger();
                    }
                }
            }
            return _Singleton;
        }

        private static string GetCurrentTime()
        {
            return DateTime.Now.ToString("yyyyMMdd");
        }


        private static string GetContentFormat(string Type, string Content, Exception ex = null)
        {
            return $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {Type}\n Url:{HttpContext.Current?.Request?.Url}\n {Content}\n {ex?.Message}\n{ex?.InnerException}";
        }

        private static void WriteLog(string logPath, string Content)
        {
            File.AppendAllText(logPath, Content);
        }

        public static void Debug(string Content, Exception ex = null)
        {
            WriteLog(Path.Combine(path, "Debug", $"{GetCurrentTime()}.log"), GetContentFormat("Debug", Content, ex));
        }

        public static void Info(string Content, Exception ex = null)
        {
            WriteLog(Path.Combine(path, "Info", $"{GetCurrentTime()}.log"), GetContentFormat("Info", Content, ex));
        }



        public static void Warn(string Content, Exception ex = null)
        {
            WriteLog(Path.Combine(path, "Warn", $"{GetCurrentTime()}.log"), GetContentFormat("Warn", Content, ex));

        }


        public static void Error(string Content, Exception ex)
        {
            WriteLog(Path.Combine(path, "Error", $"{GetCurrentTime()}.log"), GetContentFormat("Error", Content, ex));
        }


        public static Task DebugAsync(string Content, Exception ex = null)
        {
            return Task.Run(() =>
            {
                Debug(Content, ex);
            });
        }

        public static Task InfoAsync(string Content, Exception ex = null)
        {
            return Task.Run(() =>
            {
                Info(Content, ex);
            });
        }



        public static Task WarnAsync(string Content, Exception ex = null)
        {
            return Task.Run(() =>
            {
                Warn(Content, ex);
            });

        }


        public static Task ErrorAsync(string Content, Exception ex)
        {
            return Task.Run(() =>
            {
                Error(Content, ex);
            });
        }


    }
}
