using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Favor
{
    /// <summary>
    /// 删除收藏
    /// </summary>
    public class DelFavItem : RequestBase
    {
        /// <summary>
        /// 收藏id列表
        /// </summary>
        [Required]
        public uint[] favIds { get; set; }

    }
}