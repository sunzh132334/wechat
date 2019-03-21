using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Common
{
    /// <summary>
    /// 上传通讯录
    /// </summary>
    public class UploadContrat : RequestBase
    {
        /// <summary>
        /// 当前手机号码
        /// </summary>
        public string CurrentPhoneNo { get; set; }
        /// <summary>
        /// 上传手机号码列表
        /// </summary>
        [Required]
        public IList<string> PhoneNos { get; set; }
    }
}