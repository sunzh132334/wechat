using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Wechat.Api.Filters;

namespace Wechat.Api
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
        
            //webapi 配置
            GlobalConfiguration.Configure(WebApiConfig.Register);
            //RouteConfig.RegisterRoutes(RouteTable.Routes);
            //注册错误异常
            GlobalConfiguration.Configuration.Filters.Add(new ExceptionAttribute());
            //注册参数验证
            GlobalConfiguration.Configuration.Filters.Add(new ValidParameterAttribute());
            //注册认证
            //GlobalConfiguration.Configuration.Filters.Add(new AuthenticationAttribute());

        }
    }
}
