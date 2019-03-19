using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Common
{
    /// <summary>
    /// 摇一摇
    /// </summary>
    public class SharkItOff : RequestBase
    {
        /// <summary>
        /// 经度
        /// </summary>
        [Required]
        public float Longitude { get; set; }

        /// <summary>
        /// 纬度
        /// </summary>
        [Required]
        public float Latitude { get; set; }
    }
}