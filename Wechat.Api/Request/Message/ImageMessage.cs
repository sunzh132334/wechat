using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Message
{
    /// <summary>
    /// 图片消息
    /// </summary>
    public class ImageMessage : RequestBase
    {
        /// <summary>
        /// 接收的微信ID
        /// </summary>
        [Required]
        public string ToWxId { get; set; }

        ///// <summary>
        ///// 声音文件
        ///// </summary>
        //[Required]
        //public FileInfo File { get; set; }
    }
}