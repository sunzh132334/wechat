using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Pay
{
    /// <summary>
    /// 确认转账
    /// </summary>
    public class ConfirmTransfer : RequestBase
    {
        /// <summary>
        /// 银行类型
        /// </summary>
        [Required]
        public string BankType { get; set; }

        /// <summary>
        /// 绑定卡号的Id
        /// </summary>
        [Required]
        public string BindSerial { get; set; }


        /// <summary>
        /// 请求的Key
        /// </summary>
        [Required]
        public string ReqKey { get; set; }
        /// <summary>
        /// 支付密码
        /// </summary>
        [Required]
        public string PayPassword { get; set; }
    }
}