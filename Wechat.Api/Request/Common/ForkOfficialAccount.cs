using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Common
{
    /// <summary>
    /// 关注公众号
    /// </summary>
    public class ForkOfficialAccount : RequestBase
    {
        /// <summary>
        /// 公众号AppId
        /// </summary>
        [Required]
        public string AppId { get; set; }
    }
}