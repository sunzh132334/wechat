using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Wechat.Api.Abstracts;
using Wechat.Api.Extensions;
using Wechat.Api.Request.Friend;
using Wechat.Api.Response.Friend;
using Wechat.Protocol;
using Wechat.Util.Exceptions;


namespace Wechat.Api.Controllers
{
    /// <summary>
    /// 好友
    /// </summary>
    public class FriendController : WebchatControllerBase
    {

        private WechatHelper _wechat = null;
        public FriendController()
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
        [Route("api/Friend/GetContractList/{WxId}/{CurrentWxcontactSeq?}")]
        public Task<HttpResponseMessage> GetContractList(string WxId, int CurrentWxcontactSeq = 0)
        {
            ResponseBase<ContractListResponse> response = new ResponseBase<ContractListResponse>();
            try
            {
                var result = _wechat.InitContact(WxId, CurrentWxcontactSeq);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                     response.Success = false;
                    response.Code = "501";
                    response.Message = "获取失败";
                    return response.ToHttpResponseAsync();
                }
                else
                {
                    ContractListResponse contractResponse = new ContractListResponse();
                    contractResponse.Contracts = result.contactUsernameList;
                    contractResponse.CurrentWxcontactSeq = result.currentWxcontactSeq;
                    response.Data = contractResponse;
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
        /// 获取好友详情
        /// </summary>
        /// <param name="WxId"></param>
        /// <param name="CurrentWxcontactSeq"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/GetContractDetail/{WxId}/{SearchWxId}")]
        public Task<HttpResponseMessage> GetContractDetail(string WxId, string SearchWxId)
        {
            ResponseBase<micromsg.ModContact> response = new ResponseBase<micromsg.ModContact>();
            try
            {
                var result = _wechat.GetContactDetail(WxId, SearchWxId);
                if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                     response.Success = false;
                    response.Code = "501";
                    response.Message = "获取失败";
                    return response.ToHttpResponseAsync();
                }
                else
                {
                    response.Data = result.ContactList.FirstOrDefault();
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
        /// 搜索微信用户信息
        /// </summary>
        /// <param name="WxId"></param>
        /// <param name="SearchWxName"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/SearchContract/{WxId}/{SearchWxName}")]
        public Task<HttpResponseMessage> SearchContract(string WxId, string SearchWxName)
        {
            ResponseBase<MMPro.MM.SearchContactResponse> response = new ResponseBase<MMPro.MM.SearchContactResponse>();
            try
            {
                var result = _wechat.SearchContact(WxId, SearchWxName);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                     response.Success = false;
                    response.Code = "501";
                    response.Message = "获取失败";
                    return response.ToHttpResponseAsync();
                }
                else
                {
                    response.Data = result;
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
        /// 添加好友
        /// </summary>
        /// <param name="addFriend"></param> 
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/AddFriend")]
        public Task<HttpResponseMessage> AddFriend(AddFriend addFriend)
        {
            ResponseBase<string> response = new ResponseBase<string>();
            try
            {
                var result = _wechat.VerifyUser(addFriend.WxId, MMPro.MM.VerifyUserOpCode.MM_VERIFYUSER_SENDREQUEST, addFriend.Content, addFriend.AntispamTicket, addFriend.UserNameV1, (byte)addFriend.Origin);
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

    }
}