using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using static MMPro.MM;

namespace Wechat.Api.Request.FriendCircle
{
    /// <summary>
    /// 设置朋友圈
    /// </summary>
    public class SetFriendCircle : RequestBase
    {
        /// <summary>
        /// 朋友圈Id
        /// </summary>
        [Required]
        public ulong Id { get; set; }

        /// <summary>
        /// 朋友圈类型 1删除朋友圈2设为隐私3设为公开4删除评论5取消点赞
        /// </summary>
        [Required]
        public SnsObjectOpType Type { get; set; }

    }
}