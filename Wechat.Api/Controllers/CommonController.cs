using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Wechat.Api.Abstracts;
using Wechat.Api.Extensions;
using Wechat.Api.Request.Common;
using Wechat.Protocol;
using Wechat.Util.Exceptions;
namespace Wechat.Api.Controllers
{
    public class CommonController : WebchatControllerBase
    {
        private WechatHelper _wechat = null;
        public CommonController()
        {
            _wechat = new WechatHelper();
        }

        /// <summary>
        /// 获取好友联系人
        /// </summary>
        /// <param name="WxId"></param>
        /// <param name="CurrentWxcontactSeq"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Common/GetBigImage")]
        public Task<HttpResponseMessage> GetBigImage(BigImage bigImage)
        {
            ResponseBase response = new ResponseBase();
            try
            {
                var buffer = _wechat.GetMsgBigImg(bigImage.LongDataLength, bigImage.MsgId, bigImage.WxId, bigImage.ToWxId, 0);
                if (buffer == null)
                {
                    buffer.ToHttpResponseAsync();
                }
                else
                {
                     response.Success = false;
                    response.Code = "402";
                    response.Message = "图片未找到";
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
        /// 摇一摇
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Common/GetSharkItOff")]
        public Task<HttpResponseMessage> GetSharkItOff(SharkItOff sharkItOff)
        {
            ResponseBase<IList<micromsg.ShakeGetItem>> response = new ResponseBase<IList<micromsg.ShakeGetItem>>();
            try
            {
                var result = _wechat.ShakeReport(sharkItOff.WxId, sharkItOff.Latitude, sharkItOff.Longitude);
                if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                     response.Success = false;
                    response.Code = "402";
                    response.Message = "未找到";
                }
                else
                {
                    response.Data = result.ShakeGetList;

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
        /// 附近的人
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Common/GetPeopleNearby")]
        public Task<HttpResponseMessage> GetPeopleNearby(PeopleNearby peopleNearby)
        {
            ResponseBase<MMPro.MM.LBsContactInfo[]> response = new ResponseBase<MMPro.MM.LBsContactInfo[]>();
            try
            {
                var result = _wechat.LbsLBSFind(peopleNearby.WxId, peopleNearby.Latitude, peopleNearby.Longitude);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                     response.Success = false;
                    response.Code = "402";
                    response.Message = "未找到";

                }
                else
                {
                    response.Data = result.contactList;
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
        /// 绑定邮箱
        /// </summary>
        /// <param name="bindEmail"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Common/BindEmail")]
        public Task<HttpResponseMessage> BindEmail(BindEmail bindEmail)
        {
            ResponseBase<MMPro.MM.LBsContactInfo[]> response = new ResponseBase<MMPro.MM.LBsContactInfo[]>();
            try
            {
                var result = _wechat.BindEmail(bindEmail.WxId, bindEmail.Email);
                if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                     response.Success = false;
                    response.Code = "402";
                    response.Message = "绑定失败";

                }
                else
                {
                    response.Message = "绑定成功";
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
        /// 同步微信消息
        /// </summary>
        /// <param name="wxId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Common/SyncMessage/{wxId}")]
        public Task<HttpResponseMessage> SyncMessage(string wxId)
        {
            ResponseBase<InitResponse> response = new ResponseBase<InitResponse>();
            try
            {
                var result = _wechat.SyncInit(wxId);
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