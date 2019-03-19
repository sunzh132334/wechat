using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static MMPro.MM;

namespace Wechat.Api.Response.FriendCircle
{
    public class FriendCircleResponse
    {
        /// <summary>
        /// 分页的Md5
        /// </summary>
        public string FristPageMd5 { get; set; }


        /// <summary>
        /// 朋友圈详情
        /// </summary>
        public SnsObject[] ObjectList { get; set; }
    }
}