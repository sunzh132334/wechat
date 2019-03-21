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

        /// <summary>
        /// 获取好友联系人
        /// </summary>
        /// <param name="WxId">微信Id</param>
        /// <param name="CurrentWxcontactSeq">排序</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/GetContractList/{WxId}/{CurrentWxcontactSeq?}")]
        public Task<HttpResponseMessage> GetContractList(string WxId, int CurrentWxcontactSeq = 0)
        {
            ResponseBase<ContractListResponse> response = new ResponseBase<ContractListResponse>();
            try
            {
                var result = wechat.InitContact(WxId, CurrentWxcontactSeq);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.baseResponse.errMsg.@string ?? "获取失败";
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
        /// <param name="WxId">微信Id</param>
        /// <param name="SearchWxId">搜索的微信Id</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/GetContractDetail/{WxId}/{SearchWxId}")]
        public Task<HttpResponseMessage> GetContractDetail(string WxId, string SearchWxId)
        {
            ResponseBase<micromsg.ModContact> response = new ResponseBase<micromsg.ModContact>();
            try
            {
                var result = wechat.GetContactDetail(WxId, SearchWxId);
                if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.BaseResponse.ErrMsg.String ?? "获取失败";
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
        /// <param name="WxId">微信Id</param>
        /// <param name="SearchWxName">虽有微信的名称</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/SearchContract/{WxId}/{SearchWxName}")]
        public Task<HttpResponseMessage> SearchContract(string WxId, string SearchWxName)
        {
            ResponseBase<MMPro.MM.SearchContactResponse> response = new ResponseBase<MMPro.MM.SearchContactResponse>();
            try
            {
                var result = wechat.SearchContact(WxId, SearchWxName);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.baseResponse.errMsg.@string ?? "获取失败";
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
                var result = wechat.VerifyUser(addFriend.WxId, MMPro.MM.VerifyUserOpCode.MM_VERIFYUSER_ADDCONTACT, addFriend.Content, addFriend.AntispamTicket, addFriend.UserNameV1, (byte)addFriend.Origin);
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
        /// 添加好友
        /// </summary>
        /// <param name="addFriend"></param> 
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/AddFriendRequest")]
        public Task<HttpResponseMessage> AddFriendRequest(AddFriend addFriend)
        {
            ResponseBase<string> response = new ResponseBase<string>();
            try
            {
                var result = wechat.VerifyUser(addFriend.WxId, MMPro.MM.VerifyUserOpCode.MM_VERIFYUSER_SENDREQUEST, addFriend.Content, addFriend.AntispamTicket, addFriend.UserNameV1, (byte)addFriend.Origin);
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
        /// 批量添加好友
        /// </summary>
        /// <param name="addFriendList"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/AddFriendRequestList")]
        public Task<HttpResponseMessage> AddFriendRequestList(AddFriendList addFriendList)
        {
            ResponseBase<string> response = new ResponseBase<string>();
            if (addFriendList == null || addFriendList.Friends == null || addFriendList.Friends.Count == 0)
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "添加好友信息不能为空";
                return response.ToHttpResponseAsync();
            }
            try
            {
                MMPro.MM.VerifyUser[] verifyUser_ = new MMPro.MM.VerifyUser[addFriendList.Friends.Count];
                for (int i = 0; i < addFriendList.Friends.Count; i++)
                {
                    MMPro.MM.VerifyUser user = new MMPro.MM.VerifyUser();
                    user.value = addFriendList.Friends[i].UserNameV1;
                    user.antispamTicket = addFriendList.Friends[i].AntispamTicket;
                    user.friendFlag = 0;
                    user.scanQrcodeFromScene = 0;
                    verifyUser_[i] = user;
                }

                var result = wechat.VerifyUserList(addFriendList.WxId, MMPro.MM.VerifyUserOpCode.MM_VERIFYUSER_SENDREQUEST, addFriendList.Content, verifyUser_, (byte)addFriendList.Origin);
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
        /// 通过好友验证
        /// </summary>
        /// <param name="passFriendVerify"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/PassFriendVerify")]
        public Task<HttpResponseMessage> PassFriendVerify(FriendVerify passFriendVerify)
        {
            ResponseBase<string> response = new ResponseBase<string>();
            try
            {
                var result = wechat.VerifyUser(passFriendVerify.WxId, MMPro.MM.VerifyUserOpCode.MM_VERIFYUSER_VERIFYOK, passFriendVerify.Content, passFriendVerify.AntispamTicket, passFriendVerify.UserNameV1, (byte)passFriendVerify.Origin);
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
        /// 拒绝好友验证
        /// </summary>
        /// <param name="rejectFriendVerify"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Friend/RejectFriendVerify")]
        public Task<HttpResponseMessage> RejectFriendVerify(FriendVerify rejectFriendVerify)
        {
            ResponseBase<string> response = new ResponseBase<string>();
            try
            {
                var result = wechat.VerifyUser(rejectFriendVerify.WxId, MMPro.MM.VerifyUserOpCode.MM_VERIFYUSER_VERIFYREJECT, rejectFriendVerify.Content, rejectFriendVerify.AntispamTicket, rejectFriendVerify.UserNameV1, (byte)rejectFriendVerify.Origin);
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