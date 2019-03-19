using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.FriendCircle
{
    /// <summary>
    /// 朋友圈
    /// </summary>
    public class FriendCircle : RequestBase
    {
        /// <summary>
        /// 分页Md5 默认为空
        /// </summary>  
        public string FristPageMd5 { get; set; }


        /// <summary>
        /// 查询人的微信朋友圈
        /// </summary>
        [Required]
        public string ToWxId { get; set; }
    }
}