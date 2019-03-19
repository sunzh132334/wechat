using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Message
{
    /// <summary>
    /// 地址位置信息
    /// </summary>
    public class LocationMessage : RequestBase
    {
        /// <summary>
        /// 接收的微信ID
        /// </summary>
        [Required]
        public string ToWxId { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        [Required]
        public string Longitude { get; set; }

        /// <summary>
        /// 纬度
        /// </summary>
        [Required]
        public string Latitude { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [Required]
        public string Name { get; set; }
    }
}