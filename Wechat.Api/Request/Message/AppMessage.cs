using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Message
{
    /// <summary>
    /// App消息
    /// </summary>
    public class AppMessage : RequestBase
    {
        /// <summary>
        /// 接收的微信ID
        /// </summary>
        [Required]
        public string ToWxId { get; set; }
        /// <summary>
        /// appId
        /// </summary>
        [Required]
        public string AppId { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [Required]
        public string Desc { get; set; }
        /// <summary>
        /// app类型 3：音乐  4：小app  5：大app
        /// </summary>
        [Required]
        public int Type { get; set; }


        public int ShowType { get; set; } = 0;
        /// <summary>
        /// 链接
        /// </summary>
        [Required]
        public string Url { get; set; }

        /// <summary>
        /// 数据Url
        /// </summary>
        public string DataUrl { get; set; }

        /// <summary>
        /// 图片地址
        /// </summary>
        [Required]
        public string ThumbUrl { get; set; }


    }

}
 