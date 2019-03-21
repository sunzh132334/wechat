using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Favor
{
    /// <summary>
    /// 添加收藏
    /// </summary>
    public class AddFavItem : RequestBase
    {
        /// <summary>
        /// 组装的xml数据
        /// <favitem type='1'><desc>我通过了你的朋友验证请求，现在我们可以开始聊天了</desc><source sourcetype='1' sourceid='2252832101216037513'><fromusr>wxid_ccl6h2zrd7rl12</fromusr><tousr>wxid_xqyjnvihzqyn12</tousr><msgid>2252832101216037513</msgid></source></favitem>
        /// </summary>
        [Required]
        public string Object { get; set; }
        /// <summary>
        /// 来源Id
        /// </summary>
        [Required]
        public string SourceId { get; set; }
    }
}