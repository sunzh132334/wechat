using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.FriendCircle
{
    /// <summary>
    /// 发送朋友圈
    /// </summary>
    public class SendFriendCircle : RequestBase
    {
        /// <summary>
        /// 0:文字 1：图片 2视频 3：链接
        /// </summary>
        [Required]
        public int Type { get; set; }
        /// <summary>
        /// 黑名单wxId用户列表
        /// </summary>
        public IList<string> BlackList { get; set; }

        /// <summary>
        /// 标记的Wxid列表
        /// </summary>
        public IList<string> WithUserList { get; set; }

        /// <summary>
        /// media列表
        /// </summary>
        public IList<MediaInfo> MediaInfos { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 内容的链接
        /// </summary>
        public string ContentUrl { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }


        /// <summary>
        /// 内容
        /// </summary>     
        public string Content { get; set; }
    }

    public class MediaInfo
    {

        /// <summary>
        /// 数据链接
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 图片链接
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// 高度
        /// </summary> 
        public decimal Width { get; set; } = 300;
        /// <summary>
        /// 宽度
        /// </summary>
        public decimal Height { get; set; } = 300;
        /// <summary>
        /// 总大小
        /// </summary>
        public decimal TotalSize { get; set; }


    }
}