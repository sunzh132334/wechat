using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Group
{
    /// <summary>
    /// 群成员
    /// </summary>
    public class GroupMember : RequestBase
    {
        /// <summary>
        /// 群Id
        /// </summary>
        [Required]
        public string ChatRoomName { get; set; }

        /// <summary>
        /// 群成员
        /// </summary>
        [Required]
        public IList<string> ToWxIds { get; set; }
    }
}