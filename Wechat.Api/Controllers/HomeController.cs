using System.Web.Http;

namespace Wechat.Api.Controllers
{
    public class IndexController : ApiController
    {
        // GET api/values
        public string Get()
        {
            return "微信接口,接口文档地址：https://172.16.4.44:8088/svn/Arts/侠客SCRM/xiake-scrm/xiake-scrm/trunk/wechat/说明文档 ";
        }

    }
}
