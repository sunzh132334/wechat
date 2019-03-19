using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Wechat.Api.Extensions;
using Wechat.Util.Extensions;

namespace Wechat.Api.Filters
{
    /// <summary>
    /// 参数验证
    /// </summary>
    public class ValidParameterAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (!actionContext.ModelState.IsValid)
            {
                var errors = actionContext.ModelState.Values.Select(x => x.Errors);
                List<string> errormsg = new List<string>();
                //foreach (var item in errors)
                //{
                //    message = item[0].ErrorMessage;
                //    for (int i = 0; i < item.Count; i++)
                //    {                      
                //        errormsg.Add(item[i].ErrorMessage);
                //    }
                //}               
                ResponseBase response = new ResponseBase()
                {
                    Success = false,
                    Code = "400",
                    Message = errors.FirstOrDefault()[0].ErrorMessage
                };
                actionContext.Response = await response.ToHttpResponseAsync();

            }
            await base.OnActionExecutingAsync(actionContext, cancellationToken);
        }
    }
}