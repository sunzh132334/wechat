using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Common
{
    /// <summary>
    /// 视频
    /// </summary>
    public class BigVideo : RequestBase
    {
        /// <summary>
        /// 接收的微信Id
        /// </summary>
        [Required]
        public string ToWxId { get; set; }

        /// <summary>
        /// 消息Id
        /// </summary>
        [Required]
        public int MsgId { get; set; }

        /// <summary>
        /// 开始位置
        /// </summary>
        [Required]
        public int StartPos { get; set; }

        /// <summary>
        /// 大小
        /// </summary>
        [Required]
        public int DataLength { get; set; }

        /// <summary>
        /// 消息长度
        /// </summary>
        [Required]
        public long LongDataLength { get; set; }
    }
}