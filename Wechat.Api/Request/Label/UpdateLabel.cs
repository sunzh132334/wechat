using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Label
{
    /// <summary>
    /// 修改标签
    /// </summary>
    public class UpdateLabel : RequestBase
    {   
        /// <summary>
         /// 配置的微信Id
         /// </summary>
        [Required]
        public string ToWxId { get; set; }
        /// <summary>
        /// 标签Id列表
        /// </summary>
        //[Required]
        public string LabelIDList { get; set; }

    
    }
}