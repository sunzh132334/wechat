using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Wechat.Api.Abstracts;
using Wechat.Api.Extensions;
using Wechat.Api.Request.Group;
using Wechat.Api.Request.Message;
using Wechat.Protocol;
using Wechat.Util.Exceptions;
using Wechat.Util.Extensions;
namespace Wechat.Api.Controllers
{
    /// <summary>
    /// 群
    /// </summary>
    public class GroupController : WebchatControllerBase
    {
        /// <summary>
        /// 创建群
        /// </summary>
        /// <param name="greateGroup"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/CreateGroup")]
        public async Task<HttpResponseMessage> CreateGroupAsync(CreateGroup greateGroup)
        {
            ResponseBase<string> response = new ResponseBase<string>();
            if (greateGroup.ToWxIds == null || greateGroup.ToWxIds.Count == 0)
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "用户列表不能为空";
                return await response.ToHttpResponseAsync();
            }
            try
            {
                IList<MMPro.MM.MemberReq> list = new List<MMPro.MM.MemberReq>();
                var memberReqCurrent = new MMPro.MM.MemberReq();
                memberReqCurrent.member = new MMPro.MM.SKBuiltinString();
                memberReqCurrent.member.@string = greateGroup.WxId;
                list.Add(memberReqCurrent);
                foreach (var item in greateGroup.ToWxIds)
                {
                    var memberReq = new MMPro.MM.MemberReq();
                    memberReq.member = new MMPro.MM.SKBuiltinString();
                    memberReq.member.@string = item;
                    list.Add(memberReq);
                }
                var result = wechat.CreateChatRoom(greateGroup.WxId, list.ToArray(), greateGroup.GroupName);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.baseResponse.errMsg.@string ?? "创建失败";
                }
                else
                {
                    response.Data = result.chatRoomName.@string;
                    response.Message = "创建成功";
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


        /// <summary>
        /// 添加群成员
        /// </summary>
        /// <param name="addGroupMember"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/AddGroupMember")]
        public async Task<HttpResponseMessage> AddGroupMember(GroupMember addGroupMember)
        {
            ResponseBase response = new ResponseBase();
            if (addGroupMember.ToWxIds == null || addGroupMember.ToWxIds.Count == 0)
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "用户列表不能为空";
                return await response.ToHttpResponseAsync();
            }
            try
            {
                IList<MMPro.MM.MemberReq> list = new List<MMPro.MM.MemberReq>();
                foreach (var item in addGroupMember.ToWxIds)
                {
                    var memberReq = new MMPro.MM.MemberReq();
                    memberReq.member = new MMPro.MM.SKBuiltinString();
                    memberReq.member.@string = item;
                    list.Add(memberReq);
                }
                var result = wechat.AddChatRoomMember(addGroupMember.WxId, addGroupMember.ChatRoomName, list.ToArray());
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.baseResponse.errMsg.@string ?? "添加失败";
                }
                else
                {
                    response.Message = "添加成功";
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

        /// <summary>
        /// 删除群成员
        /// </summary>
        /// <param name="deleteGroupMember"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/DeleteGroupMember")]
        public async Task<HttpResponseMessage> DeleteGroupMember(GroupMember deleteGroupMember)
        {
            ResponseBase response = new ResponseBase();
            if (deleteGroupMember.ToWxIds == null || deleteGroupMember.ToWxIds.Count == 0)
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "用户列表不能为空";
                return await response.ToHttpResponseAsync();
            }
            try
            {
                IList<MMPro.MM.DelMemberReq> list = new List<MMPro.MM.DelMemberReq>();
                foreach (var item in deleteGroupMember.ToWxIds)
                {
                    var memberReq = new MMPro.MM.DelMemberReq();
                    memberReq.memberName = new MMPro.MM.SKBuiltinString();
                    memberReq.memberName.@string = item;
                    list.Add(memberReq);
                }
                var result = wechat.DelChatRoomMember(deleteGroupMember.WxId, deleteGroupMember.ChatRoomName, list.ToArray());
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.baseResponse.errMsg.@string ?? "删除失败";
                }
                else
                {
                    response.Message = "删除成功";
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
        /// <summary>
        /// 获取群成员
        /// </summary>
        /// <param name="addGroupMember"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/GetGroupMembers")]
        public async Task<HttpResponseMessage> GetGroupMembers(GetGroupMember addGroupMember)
        {
            ResponseBase<MMPro.MM.ChatRoomMemberData> response = new ResponseBase<MMPro.MM.ChatRoomMemberData>();
            try
            {
                var result = wechat.GetChatroomMemberDetail(addGroupMember.WxId, addGroupMember.ChatRoomName);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.baseResponse.errMsg.@string ?? "获取失败";
                }
                else
                {
                    response.Data = result.newChatroomData;
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



        /// <summary>
        /// 获取群成员
        /// </summary>
        /// <param name="quitGroup"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/QuitGroup")]
        public async Task<HttpResponseMessage> QuitGroup(QuitGroup quitGroup)
        {
            ResponseBase response = new ResponseBase();
            try
            {
                var result = wechat.QuitGroup(quitGroup.WxId, quitGroup.ChatRoomName);
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
            return await response.ToHttpResponseAsync();
        }
        /// <summary>
        /// 修改公告
        /// </summary>
        /// <param name="groupAnnouncement"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Group/UpdateGroupAnnouncement")]
        public async Task<HttpResponseMessage> UpdateGroupAnnouncement(GroupAnnouncement groupAnnouncement)
        {
            ResponseBase response = new ResponseBase();
            try
            {
                var result = wechat.setChatRoomAnnouncement(groupAnnouncement.WxId, groupAnnouncement.ChatRoomName, groupAnnouncement.Announcement);
                if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.BaseResponse.ErrMsg.String ?? "修改失败";
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
            return await response.ToHttpResponseAsync();
        }
    }
}