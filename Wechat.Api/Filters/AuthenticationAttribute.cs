using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Wechat.Api.Extensions;
using Wechat.Api.Request;
using Wechat.Util.Cache;

namespace Wechat.Api.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AuthenticationAttribute : AuthorizationFilterAttribute
    {
        public override async Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var requestBase = await actionContext.Request.Content.ReadAsAsync<RequestBase>();
            if (requestBase == null)
            {
                ResponseBase response = new ResponseBase()
                {
                    Success = false,
                    Code = "400",
                    Message = "参数WxId不存在"
                };
                actionContext.Response = await response.ToHttpResponseAsync();
            }
            else
            {
                var customerInfoCache = CacheHelper.CreateInstance().Get<CustomerInfoCache>(ConstCacheKey.GetWxIdKey(requestBase.WxId));
                if (customerInfoCache == null)
                {
                    ResponseBase response = new ResponseBase()
                    {
                        Success = false,
                        Code = "401",
                        Message = "缓存失效,请重新生成二维码登录"
                    };
                    actionContext.Response = await response.ToHttpResponseAsync();
                }
            }


            base.OnAuthorization(actionContext);
        }
    }
}