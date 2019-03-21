using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Favor
{
    /// <summary>
    /// 同步收藏
    /// </summary>
    public class FavSync : RequestBase
    {
        /// <summary>
        /// 二进制流
        /// </summary>
        public byte[] KeyBuf { get; set; }
    }
}