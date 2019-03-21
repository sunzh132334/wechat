using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Common
{
    /// <summary>
    /// 修改密码
    /// </summary>
    public class ChangePassword : RequestBase
    {
        /// <summary>
        /// 新密码
        /// </summary>
        public string NewPassword { get; set; }

        /// <summary>
        /// 票据
        /// </summary>
        public string Ticket { get; set; }
    }
}