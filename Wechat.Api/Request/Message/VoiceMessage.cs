using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Message
{
    /// <summary>
    /// 声音消息
    /// </summary>
    public class VoiceMessage : RequestBase
    {
        /// <summary>
        /// 发送的微信ID
        /// </summary>
        [Required]
        public IList<string> ToWxIds { get; set; }
        /// <summary>
        /// 声音秒数
        /// </summary>
        [Required]
        public int VoiceSecond { get; set; }

        /// <summary>
        /// oss ObjectName
        /// </summary>
        [Required]
        public string ObjectName { get; set; }
        /// <summary>
        /// 文件名
        /// </summary>
        [Required]
        public string FileName { get; set; }
    }
}