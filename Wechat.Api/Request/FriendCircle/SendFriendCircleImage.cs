using micromsg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Wechat.Api.Request.FriendCircle
{
    public class SendFriendCircleImage
    {
        /// <summary>
        /// 客户端Id
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// bufferurl
        /// </summary>
        public SnsBufferUrl BufferUrl { get; set; }

        /// <summary>
        /// ThumbUrls
        /// </summary>
        public List<SnsBufferUrl> ThumbUrls { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public uint Type { get; set; }
    }
}