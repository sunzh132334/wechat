using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.FriendCircle
{
    /// <summary>
    /// 朋友圈列表
    /// </summary>
    public class FriendCircleList : RequestBase
    {
        /// <summary>
        /// 分页Md5
        /// </summary> 
        public string FristPageMd5 { get; set; }
    }
}