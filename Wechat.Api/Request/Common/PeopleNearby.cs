using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wechat.Api.Request.Common
{
    /// <summary>
    /// 附近的人
    /// </summary>
    public class PeopleNearby : RequestBase
    {
        /// <summary>
        /// 经度
        /// </summary>
        public float Longitude { get; set; }

        /// <summary>
        /// 纬度
        /// </summary>
        public float Latitude { get; set; }


    }
}