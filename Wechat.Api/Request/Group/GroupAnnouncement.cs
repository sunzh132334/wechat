using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Group
{
    /// <summary>
    /// 群公告
    /// </summary>
    public class GroupAnnouncement : RequestBase
    {
        /// <summary>
        /// 群Id
        /// </summary>
        [Required]
        public string ChatRoomName { get; set; }
        /// <summary>
        /// 公告
        /// </summary>
        [Required]
        public string Announcement { get; set; }
    }
}