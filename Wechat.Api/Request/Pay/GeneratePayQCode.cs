using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Pay
{
    /// <summary>
    /// 生成支付二维码
    /// </summary>
    public class GeneratePayQCode : RequestBase
    {
        /// <summary>
        /// 描述
        /// </summary>
        public string Name { get; set; } = "转账";

        /// <summary>
        /// 金额
        /// </summary>
        [Required]
        public decimal Money { get; set; }
    }


    public class GeneratePayQCodeReqTest
    {
        public string pay_url { get; set; }

        public int retcode { get; set; }

        public string retmsg { get; set; }


    }
}