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


        /// <summary>
        /// 获取大图片
        /// </summary>   
        /// <param name="bigImage"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Common/GetBigImage")]
        public Task<HttpResponseMessage> GetBigImage(BigImage bigImage)
        {
            ResponseBase response = new ResponseBase();
            try
            {
                var buffer = wechat.GetMsgBigImg(bigImage.LongDataLength, bigImage.MsgId, bigImage.WxId, bigImage.ToWxId, 0);
                if (buffer == null)
                {
                    buffer.ToHttpImageResponseAsync();
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
                var result = wechat.ShakeReport(sharkItOff.WxId, sharkItOff.Latitude, sharkItOff.Longitude);
                if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "402";
                    response.Message = result.BaseResponse.ErrMsg.String ?? "未找到";
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
                var result = wechat.LbsLBSFind(peopleNearby.WxId, peopleNearby.Latitude, peopleNearby.Longitude);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "402";
                    response.Message = result.baseResponse.errMsg.@string ?? "未找到";

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
                var result = wechat.BindEmail(bindEmail.WxId, bindEmail.Email);
                if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "402";
                    response.Message = result.BaseResponse.ErrMsg.String ?? "绑定失败";

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
                var result = wechat.SyncInit(wxId);
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


        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="wxId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Common/ChangePassword")]
        public Task<HttpResponseMessage> ChangePassword(ChangePassword changePassword)
        {
            ResponseBase response = new ResponseBase();
            try
            {
                var result = wechat.NewSetPasswd(changePassword.WxId, changePassword.NewPassword, changePassword.Ticket);
                if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result?.BaseResponse?.ErrMsg?.String ?? "修改失败";
                    return response.ToHttpResponseAsync();
                }
                else
                {
                    response.Message = "修改成功";
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
        /// 关注公众号
        /// </summary>
        /// <param name="forkOfficialAccount"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Common/ForkOfficialAccountsMessage")]
        public Task<HttpResponseMessage> ForkOfficialAccountMessage(ForkOfficialAccount forkOfficialAccount)
        {
            ResponseBase<string> response = new ResponseBase<string>();
            try
            {
                var result = wechat.VerifyUser(forkOfficialAccount.WxId, MMPro.MM.VerifyUserOpCode.MM_VERIFYUSER_ADDCONTACT, "", "", "", 0);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result?.baseResponse?.errMsg?.@string;
                    return response.ToHttpResponseAsync();
                }
                else
                {
                    response.Data = result.userName;
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
        /// 上传手机联系人
        /// </summary>
        /// <param name="uploadContrat"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Common/UploadContrats")]
        public async Task<HttpResponseMessage> UploadContrats(UploadContrat uploadContrat)
        {
            ResponseBase response = new ResponseBase();
            if (uploadContrat == null || uploadContrat.PhoneNos == null || uploadContrat.PhoneNos.Count == 0)
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "上传手机号码不能为空";
                return await response.ToHttpResponseAsync();
            }
            try
            {
                micromsg.Mobile[] mobiles = new micromsg.Mobile[uploadContrat.PhoneNos.Count];
                for (int i = 0; i < uploadContrat.PhoneNos.Count; i++)
                {
                    micromsg.Mobile mobile = new micromsg.Mobile();
                    mobile.v = uploadContrat.PhoneNos[i];
                    mobiles[i] = mobile;
                }
                var result = wechat.UploadMContact(uploadContrat.WxId, uploadContrat.CurrentPhoneNo, mobiles);
                if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result?.BaseResponse?.ErrMsg?.String ?? "上传失败";
                    return await response.ToHttpResponseAsync();
                }
                else
                {
                    response.Message = "上传成功";
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
            return await response.ToHttpResponseAsync();
        }
    }
}