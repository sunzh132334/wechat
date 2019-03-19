using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static MMPro.MM;

namespace Wechat.Api.Response.Login
{
    public class QrCodeResponse
    {
        /// <summary>
        /// base64图片
        /// </summary>
        public string QrBase64 { get; set; }

        /// <summary>
        /// 二维码标识
        /// </summary>
        public string Uuid { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ExpiredTime { get; set; }

      


    }
}