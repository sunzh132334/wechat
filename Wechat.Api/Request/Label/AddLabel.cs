using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Label
{
    public class AddLabel : RequestBase
    {
     
        /// <summary>
        /// 标签名称
        /// </summary>
        [Required]
        public string LabelName { get; set; }
 
    }
}