using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Group
{
    /// <summary>
    /// 退出群
    /// </summary>
    public class QuitGroup : RequestBase
    {
        /// <summary>
        /// 群Id
        /// </summary>
        [Required]
        public string ChatRoomName { get; set; }
    }
}