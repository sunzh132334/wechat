using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wechat.Util.Cache
{
    public class CustomerInfoCache
    {
        /// <summary>
        /// UuId
        /// </summary>
        public string Uuid { get; set; }
        /// <summary>
        /// 微信Id
        /// </summary>
        public string WxId { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 绑定邮箱
        /// </summary>
        public string BindEmail { get; set; }

        /// <summary>
        /// 绑定邮箱
        /// </summary>
        public string BindMobile { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 别名
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// 设备信息Xml
        /// </summary>
        public string DeviceInfoXml { get; set; }

        /// <summary>
        /// 状态 -1失效 0:未扫码 1：扫码 2：登陆
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 微信newPass
        /// </summary>
        public string WxNewPass { get; set; }

        /// <summary>
        /// 设备Id
        /// </summary>
        public byte[] DeviceId { get; set; }

        /// <summary>
        /// 手机设备
        /// </summary>
        public string Device { get; set; }
        /// <summary>
        /// 头像Url
        /// </summary>
        public string HeadUrl { get; set; }

        /// <summary>
        /// MUid
        /// </summary>
        public int MUid { get; set; }
        /// <summary>
        /// Cookie
        /// </summary>
        public string Cookie { get; set; }

        /// <summary>
        /// 16位 AEs秘钥
        /// </summary>
        public byte[] AesKey { get; set; }

        /// <summary>
        /// 私钥
        /// </summary>
        public byte[] PriKeyBuf { get; set; } = new byte[328];

        /// <summary>
        /// 公钥
        /// </summary>
        public byte[] PubKeyHUb { get; set; } = new byte[57];

        /// <summary>
        /// 同步消息Key
        /// </summary>
        public byte[] InitSyncKey { get; set; }

        /// <summary>
        /// 同步消息MaxKey
        /// </summary>
        public byte[] MaxSyncKey { get; set; }

        /// <summary>
        /// AuthKey
        /// </summary>
        public byte[] AuthKey { get; set; }

        /// <summary>
        /// 同步
        /// </summary>
        public byte[] Sync { get; set; }


        public BaseRequestCache BaseRequest { get; set; }

        public class BaseRequestCache
        {
            public byte[] sessionKey;

            public int uin;

            public byte[] devicelId;

            public int clientVersion;

            public string osType;

            public int scene;
        }

    }
}