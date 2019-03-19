using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Common
{
    /// <summary>
    /// 绑定邮箱
    /// </summary>
    public class BindEmail : RequestBase
    {
        /// <summary>
        /// 邮箱
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}