using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Filters;
using Wechat.Api.Extensions;
using Wechat.Util.Log;

namespace Wechat.Api.Filters
{

    /// <summary>
    /// WebApi异常过滤器
    /// 可以在Controller或Action上单独使用，也可直接注册为全局过滤器
    /// </summary>
    public class ExceptionAttribute : ExceptionFilterAttribute
    {
        public override async Task OnExceptionAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {

            base.OnException(actionExecutedContext);
            ResponseBase response = new ResponseBase()
            {
                Success = false,
                Code = "500",
                Message = actionExecutedContext.Exception.Message
            };

            Logger.Error(actionExecutedContext.Exception.Message, actionExecutedContext.Exception);
            actionExecutedContext.Response = await response.ToHttpResponseAsync();
        }
    }
}