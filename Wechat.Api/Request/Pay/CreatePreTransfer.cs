using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Pay
{
    /// <summary>
    /// 创建转账
    /// </summary>
    public class CreatePreTransfer : RequestBase
    {
        /// <summary>
        /// 到账微信Id
        /// </summary>
        [Required]
        public string ToWxId { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        [Required] 
        public decimal Money { get; set; }
        /// <summary>
        /// 转账名称
        /// </summary>
        public string Name { get; set; } = "转账";
    }
}