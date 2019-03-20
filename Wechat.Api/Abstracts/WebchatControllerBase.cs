using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Wechat.Protocol;

namespace Wechat.Api.Abstracts
{
    /// <summary>
    /// 微信基类
    /// </summary>
    public abstract class WebchatControllerBase : ApiController
    {

        protected WechatHelper wechat = null;
        public WebchatControllerBase()
        {
            wechat = new WechatHelper();
        }


    }
}