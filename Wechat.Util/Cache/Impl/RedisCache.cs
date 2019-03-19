//using Newtonsoft.Json;
//using StackExchange.Redis;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Wechat.Util.Cache
//{
//    /// <summary>
//    /// redis缓存
//    /// </summary>
//    public class RedisCache : ICache
//    {
//        private int Default_Timeout = 600;//默认超时时间（单位秒）
//        private string address;
//        private JsonSerializerSettings jsonConfig = new JsonSerializerSettings() { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
//        private ConnectionMultiplexer connectionMultiplexer;
//        private IDatabase database;

//        class CacheObject<T>
//        {
//            public int ExpireTime { get; set; }
//            public bool ForceOutofDate { get; set; }
//            public T Value { get; set; }
//        }

//        private RedisCache()
//        {
//            address = ConfigurationManager.AppSettings["RedisServer"] ?? "172.16.6.93:6379";
//            int.TryParse(ConfigurationManager.AppSettings["RedisServerDbIndex"], out int dbIndexInt);
//            if (address == null || string.IsNullOrWhiteSpace(address.ToString()))
//            {
//                throw new ApplicationException("配置文件中未找到RedisServer的有效配置");
//            }
//            connectionMultiplexer = ConnectionMultiplexer.Connect(address);
//            database = connectionMultiplexer.GetDatabase(dbIndexInt);
//        }

//        private static RedisCache _Singleton = null;
//        private static object Singleton_Lock = new object();
//        public static RedisCache CreateInstance()
//        {
//            if (_Singleton == null) //双if +lock
//            {
//                lock (Singleton_Lock)
//                {
//                    if (_Singleton == null)
//                    {
//                        _Singleton = new RedisCache();
//                    }
//                }
//            }
//            return _Singleton;
//        }

//        /// <summary>
//        /// 连接超时设置
//        /// </summary>
//        public int TimeOut
//        {
//            get
//            {
//                return Default_Timeout;
//            }
//            set
//            {
//                Default_Timeout = value;
//            }
//        }

//        public object Get(string key)
//        {
//            return Get<object>(key);
//        }

//        public T Get<T>(string key)
//        {

//            DateTime begin = DateTime.Now;
//            var cacheValue = database.StringGet(key);
//            DateTime endCache = DateTime.Now;
//            var value = default(T);
//            if (!cacheValue.IsNull)
//            {
//                var cacheObject = JsonConvert.DeserializeObject<CacheObject<T>>(cacheValue, jsonConfig);
//                if (!cacheObject.ForceOutofDate)
//                    database.KeyExpire(key, new TimeSpan(0, 0, cacheObject.ExpireTime));
//                value = cacheObject.Value;
//            }
//            DateTime endJson = DateTime.Now;
//            return value;

//        }

//        public void Add(string key, object data)
//        {
//            var jsonData = GetJsonData(data, TimeOut, false);
//            database.StringSet(key, jsonData);
//        }

//        public void Add(string key, object data, int cacheTime)
//        {
//            var timeSpan = TimeSpan.FromSeconds(cacheTime);
//            var jsonData = GetJsonData(data, TimeOut, true);
//            database.StringSet(key, jsonData, timeSpan);
//        }

//        public void Add(string key, object data, DateTime cacheTime)
//        {
//            var timeSpan = cacheTime - DateTime.Now;
//            var jsonData = GetJsonData(data, TimeOut, true);
//            database.StringSet(key, jsonData, timeSpan);
//        }

//        public void Add<T>(string key, T data)
//        {
//            var jsonData = GetJsonData<T>(data, TimeOut, false);
//            database.StringSet(key, jsonData);
//        }

//        public void Add<T>(string key, T data, int cacheTime)
//        {
//            var timeSpan = TimeSpan.FromSeconds(cacheTime);
//            var jsonData = GetJsonData<T>(data, TimeOut, true);
//            database.StringSet(key, jsonData, timeSpan);
//        }

//        public void Add<T>(string key, T data, DateTime cacheTime)
//        {
//            var timeSpan = cacheTime - DateTime.Now;
//            var jsonData = GetJsonData<T>(data, TimeOut, true);
//            database.StringSet(key, jsonData, timeSpan);        }


//        string GetJsonData(object data, int cacheTime, bool forceOutOfDate)
//        {
//            var cacheObject = new CacheObject<object>() { Value = data, ExpireTime = cacheTime, ForceOutofDate = forceOutOfDate };
//            return JsonConvert.SerializeObject(cacheObject, jsonConfig);//序列化对象
//        }

//        string GetJsonData<T>(T data, int cacheTime, bool forceOutOfDate)
//        {
//            var cacheObject = new CacheObject<T>() { Value = data, ExpireTime = cacheTime, ForceOutofDate = forceOutOfDate };
//            return JsonConvert.SerializeObject(cacheObject, jsonConfig);//序列化对象
//        }
//        /// <summary>
//        /// 删除
//        /// </summary>
//        /// <param name="key"></param>
//        public void Remove(string key)
//        {
//            database.KeyDelete(key);
//        }

//        /// <summary>
//        /// 判断key是否存在
//        /// </summary>
//        public bool Exists(string key)
//        {
//            return database.KeyExists(key);
//        }

//    }
//}
