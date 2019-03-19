using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Message
{
    /// <summary>
    /// 名片消息
    /// </summary>
    public class CardMessage : RequestBase
    {
        /// <summary>
        /// 接收的微信ID
        /// </summary>
        [Required]
        public string ToWxId { get; set; }

        /// <summary>
        /// 发送的微信Id
        /// </summary>
        [Required]
        public string CardWxId { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        [Required]
        public string CardNickName { get; set; }

        /// <summary>
        /// 别名
        /// </summary>
        public string CardAlias { get; set; }
    }
}