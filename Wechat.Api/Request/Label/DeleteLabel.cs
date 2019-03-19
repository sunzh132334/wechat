using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Label
{
    /// <summary>
    /// 删除标签
    /// </summary>
    public class DeleteLabel : RequestBase
    {   
         
        /// <summary>
        /// 标签名称
        /// </summary>
        [Required]
        public string LabelIDList { get; set; }


    }
}