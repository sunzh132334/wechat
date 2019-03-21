using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.FriendCircle
{
    /// <summary>
    /// 回复朋友圈 （评论 点赞）
    /// </summary>
    public class FriendCircleComment : RequestBase
    {
        /// <summary>
        /// 朋友圈Id
        /// </summary>
        [Required]
        public string Id { get; set; }
        /// <summary>
        /// 发送微信Id
        /// </summary>
        //[Required]
        //public string ToWxId { get; set; }

        /// <summary>
        /// 类型 1点赞 2：文本 3:消息 4：with 5陌生人点赞
        /// </summary>
        [Required]
        public int Type { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 回复评论Id
        /// </summary>
        public int ReplyCommnetId { get; set; }


    }
}