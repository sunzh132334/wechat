using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Label
{
    /// <summary>
    /// 批量添加
    /// </summary>
    public class BatchUpdateLabel : RequestBase
    {    
        /// <summary>
             /// 配置的微信Id
             /// </summary>
        [Required]
        public IList<string> ToWxIds { get; set; }
        /// <summary>
        /// 标签Id列表
        /// </summary>
        //[Required]
        public string LabelIDList { get; set; }
    }
}