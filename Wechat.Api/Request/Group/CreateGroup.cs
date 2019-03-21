using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Group
{
    /// <summary>
    /// 创建群
    /// </summary>
    public class CreateGroup : RequestBase
    {
        /// <summary>
        /// 群名称
        /// </summary>
        [Required]
        public string GroupName { get; set; }
        /// <summary>
        /// 群成员
        /// </summary>
        [Required]
        public IList<string> ToWxIds { get; set; }
    }
}