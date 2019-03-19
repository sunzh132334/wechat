using micromsg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
 

namespace Wechat.Api.Request.FriendCircle
{
    public class SendFriendCircleImage
    {
       
        public string ClientId { get; set; }
 
        public SnsBufferUrl BufferUrl { get; set; } 
 
 
        public List<SnsBufferUrl> ThumbUrls { get; set; }
       
        public ulong Id { get; set; }
 
        public uint Type { get; set; }
    }
}