using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request
{
    /// <summary>
    /// 请求基类
    /// </summary>
    public class RequestBase
    {
        /// <summary>
        /// 微信Id
        /// </summary>
        [Required]
        public virtual string WxId { get; set; }


    }
}