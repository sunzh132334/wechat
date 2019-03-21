using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Wechat.Api.Extensions;
using Wechat.Api.Response.Login;
using Wechat.Protocol;
using Wechat.Util.Exceptions;

namespace Wechat.Api.Controllers
{
    /// <summary>
    /// 登陆
    /// </summary> 
    public class LoginController : ApiController
    {
        private WechatHelper _wechat = null;
        /// <summary>
        /// 构造
        /// </summary>
        public LoginController()
        {
            _wechat = new WechatHelper();
        }

        /// <summary>
        /// 获取登陆二维码
        /// </summary>
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Login/GetQrCode")]
        public Task<HttpResponseMessage> GetQrCode()
        {
            ResponseBase<QrCodeResponse> response = new ResponseBase<QrCodeResponse>();
            try
            {
                var result = _wechat.GetLoginQRcode();
                if (result != null && result.baseResponse.ret == MMPro.MM.RetConst.MM_OK)
                {
                    QrCodeResponse qrCodeResponse = new QrCodeResponse();
                    qrCodeResponse.QrBase64 = $"data:img/jpg;base64,{Convert.ToBase64String(result.qRCode.src)}";
                    qrCodeResponse.Uuid = result.uuid;
                    qrCodeResponse.ExpiredTime = DateTime.Now.AddSeconds(result.expiredTime);
                    response.Data = qrCodeResponse;
                }
                else
                {
                     response.Success = false;
                    response.Code = "501";
                    response.Message = "获取二维码失败";
                }
            }
            catch (ExpiredException ex)
            {
                 response.Success = false;
                response.Code = "401";
                response.Message = ex.Message;
            }
            catch (Exception ex)
            {
                 response.Success = false;
                response.Code = "500";
                response.Message = ex.Message;
            }
            return response.ToHttpResponseAsync();
        }



        /// <summary>
        /// 检查是否登陆
        /// </summary>
        /// <param name="uuid">UUid</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/CheckLogin/{Uuid}")]
        public Task<HttpResponseMessage> CheckLogin(string uuid)
        {
            ResponseBase<CheckLoginResponse> response = new ResponseBase<CheckLoginResponse>();
            try
            {
                var result = _wechat.CheckLoginQRCode(uuid);
                CheckLoginResponse checkLoginResponse = new CheckLoginResponse();
                checkLoginResponse.State = result.State;
                checkLoginResponse.Uuid = result.Uuid;
                checkLoginResponse.WxId = result.WxId;
                checkLoginResponse.NickName = result.NickName;
                checkLoginResponse.Device = result.Device;
                checkLoginResponse.HeadUrl = result.HeadUrl;
                checkLoginResponse.Mobile = result.BindMobile;
                checkLoginResponse.Email = result.BindEmail;
                checkLoginResponse.Alias = result.Alias;
                response.Data = checkLoginResponse;
            }
            catch (ExpiredException ex)
            {
                 response.Success = false;
                response.Code = "401";
                response.Message = ex.Message;
            }
            catch (Exception ex)
            {
                 response.Success = false;
                response.Code = "500";
                response.Message = ex.Message;
            }
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <param name="wxId">微信Id</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/LogOut/{wxId}")]
        public Task<HttpResponseMessage> LogOut(string wxId)
        {
            ResponseBase<InitResponse> response = new ResponseBase<InitResponse>();
            try
            {
                var result = _wechat.logOut(wxId);
                if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.BaseResponse.ErrMsg.String ?? "退出失败";
                }
                else
                {
                    response.Message = "退出成功";
                }
            }
            catch (ExpiredException ex)
            {
                response.Success = false;
                response.Code = "401";
                response.Message = ex.Message;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Code = "500";
                response.Message = ex.Message;
            }
            return response.ToHttpResponseAsync();
        }
        /// <summary>
        /// 初始化用户信息
        /// </summary>
        /// <param name="wxId">微信Id</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/NewInit/{wxId}")]
        public Task<HttpResponseMessage> NewInit(string wxId)
        {
            ResponseBase<InitResponse> response = new ResponseBase<InitResponse>();
            try
            {
                var result = _wechat.Init(wxId);
                response.Data = result;
            }
            catch (ExpiredException ex)
            {
                 response.Success = false;
                response.Code = "401";
                response.Message = ex.Message;
            }
            catch (Exception ex)
            {
                 response.Success = false;
                response.Code = "500";
                response.Message = ex.Message;
            }
            return response.ToHttpResponseAsync();
        }




    }
}