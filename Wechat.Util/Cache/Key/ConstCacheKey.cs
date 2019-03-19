using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wechat.Util.Cache
{
    /// <summary>
    /// 缓存键
    /// </summary>
    public class ConstCacheKey
    {
        /// <summary>
        /// 获取UUid
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public static string GetUuidKey(string uuid)
        {
            return $"Uuid_{uuid}";
        }

        /// <summary>
        /// 获取微信Id
        /// </summary>
        /// <param name="wxId"></param>
        /// <returns></returns>
        public static string GetWxIdKey(string wxId)
        {
            return $"WxId_{wxId}";
        }
    }
}