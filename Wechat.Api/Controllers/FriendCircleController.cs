using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Wechat.Api.Abstracts;
using Wechat.Api.Extensions;
using Wechat.Api.Helper;
using Wechat.Api.Request.Common;
using Wechat.Api.Request.FriendCircle;
using Wechat.Api.Response.FriendCircle;
using Wechat.Protocol;
using Wechat.Util.Exceptions;
using Wechat.Util.Extensions;

namespace Wechat.Api.Controllers
{
    /// <summary>
    /// 朋友圈
    /// </summary>
    public class FriendCircleController : WebchatControllerBase
    {
        private WechatHelper _wechat = null;
        public FriendCircleController()
        {
            _wechat = new WechatHelper();
        }

        /// <summary>
        /// 获取特定人朋友圈
        /// </summary>
        /// <param name="bindEmail"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/FriendCircle/GetFriendCircleDetail")]
        public Task<HttpResponseMessage> GetFriendCircleDetail(FriendCircle friendCircle)
        {
            ResponseBase<FriendCircleResponse> response = new ResponseBase<FriendCircleResponse>();
            try
            {
                var result = _wechat.SnsUserPage(friendCircle.FristPageMd5, friendCircle.WxId, friendCircle.ToWxId);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                     response.Success = false;
                    response.Code = "402";
                    response.Message = "获取失败";
                }
                else
                {
                    response.Data = new FriendCircleResponse()
                    {
                        FristPageMd5 = result.fristPageMd5,
                        ObjectList = result.objectList
                    };
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
        /// 获取自己朋友圈列表
        /// </summary>
        /// <param name="friendCircle"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/FriendCircle/GetFriendCircleList")]
        public Task<HttpResponseMessage> GetFriendCircleList(FriendCircleList friendCircleList)
        {
            ResponseBase<FriendCircleResponse> response = new ResponseBase<FriendCircleResponse>();
            try
            {
                var result = _wechat.SnsTimeLine(friendCircleList.WxId, friendCircleList.FristPageMd5);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                     response.Success = false;
                    response.Code = "402";
                    response.Message = "获取失败";
                }
                else
                {
                    response.Data = new FriendCircleResponse()
                    {
                        FristPageMd5 = result.fristPageMd5,
                        ObjectList = result.objectList
                    };
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
        /// 操作朋友圈 1删除朋友圈2设为隐私3设为公开4删除评论5取消点赞
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/FriendCircle/SetFriendCircle")]
        public Task<HttpResponseMessage> SetFriendCircle(SetFriendCircle setFriendCircle)
        {
            ResponseBase response = new ResponseBase();
            try
            {
                var result = _wechat.GetSnsObjectOp(setFriendCircle.Id, setFriendCircle.WxId, setFriendCircle.Type);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                     response.Success = false;
                    response.Code = "402";
                    response.Message = "操作失败";
                }
                else
                {
                    response.Message = "操作成功";
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
        /// 发送朋友圈
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/FriendCircle/SendFriendCircle")]
        public Task<HttpResponseMessage> SendFriendCircle(SendFriendCircle sendFriendCircle)
        {
            ResponseBase<MMPro.MM.SnsObject> response = new ResponseBase<MMPro.MM.SnsObject>();
            try
            {
                string content = null;
                switch (sendFriendCircle.Type)
                {
                    case 0: content = SendSnsConst.GetContentTemplate(sendFriendCircle.Content, sendFriendCircle.Title, sendFriendCircle.ContentUrl, sendFriendCircle.Description); break;
                    case 1: content = SendSnsConst.GetImageTemplate(sendFriendCircle.Content, sendFriendCircle.MediaInfos, sendFriendCircle.Title, sendFriendCircle.ContentUrl, sendFriendCircle.Description); break;
                    case 2: content = SendSnsConst.GetVideoTemplate(sendFriendCircle.Content, sendFriendCircle.MediaInfos, sendFriendCircle.Title, sendFriendCircle.ContentUrl, sendFriendCircle.Description); break;
                    case 3: content = SendSnsConst.GetLinkTemplate(sendFriendCircle.Content, sendFriendCircle.MediaInfos, sendFriendCircle.Title, sendFriendCircle.ContentUrl, sendFriendCircle.Description); break;
                    case 4: content = SendSnsConst.GetImageTemplate3(sendFriendCircle.Content, sendFriendCircle.MediaInfos, sendFriendCircle.Title, sendFriendCircle.ContentUrl, sendFriendCircle.Description); break;
                    case 5: content = SendSnsConst.GetImageTemplate4(sendFriendCircle.Content, sendFriendCircle.MediaInfos, sendFriendCircle.Title, sendFriendCircle.ContentUrl, sendFriendCircle.Description); break;
                    case 6: content = SendSnsConst.GetImageTemplate5(sendFriendCircle.Content, sendFriendCircle.MediaInfos, sendFriendCircle.Title, sendFriendCircle.ContentUrl, sendFriendCircle.Description); break;

                }

                var result = _wechat.SnsPost(sendFriendCircle.WxId, content, sendFriendCircle.BlackList, sendFriendCircle.WithUserList);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                     response.Success = false;
                    response.Code = "402";
                    response.Message = "发送失败";
                }
                else
                {
                    response.Message = "发送成功";
                    response.Data = result.snsObject;
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
        /// 同步朋友圈
        /// </summary>
        /// <param name="wxId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/FriendCircle/SyncFriendCircle/{wxId}")]
        public Task<HttpResponseMessage> SyncFriendCircle(string wxId)
        {

            ResponseBase response = new ResponseBase();
            try
            {
                var result = _wechat.SnsSync(wxId);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                     response.Success = false;
                    response.Code = "402";
                    response.Message = "同步失败";
                }
                else
                {
                    response.Message = "同步成功";
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
        /// 上传朋友圈图片
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/FriendCircle/SendFriendCircleImage")]
        public async Task<HttpResponseMessage> SendFriendCircleImage()
        {
            ResponseBase<IList<SendFriendCircleImage>> response = new ResponseBase<IList<SendFriendCircleImage>>();
            if (!Request.Content.IsMimeMultipartContent())
            {
                 response.Success = false;
                response.Code = "400";
                response.Message = "请表单提交";
                return await response.ToHttpResponseAsync();
            }
            if (!Request.Content.IsMimeMultipartContent())
            {
                 response.Success = false;
                response.Code = "400";
                response.Message = "请表单提交";
                return await response.ToHttpResponseAsync();
            }
            var fileCount = HttpContext.Current.Request.Files.Count;
            if (fileCount == 0)
            {
                 response.Success = false;
                response.Code = "400";
                response.Message = "请上传文件";
            }
            var wxId = HttpContext.Current.Request["WxId"];
            if (string.IsNullOrEmpty(wxId))
            {
                 response.Success = false;
                response.Code = "400";
                response.Message = "WxId不能为空";
                return await response.ToHttpResponseAsync();
            }

            for (int i = 0; i < fileCount; i++)
            {
                var file = HttpContext.Current.Request.Files[i];
                if (file.FileName.IsImage() && file.FileName.IsVideo())
                {
                     response.Success = false;
                    response.Code = "400";
                    response.Message = $"{file.FileName}不是图片文件";
                    return await response.ToHttpResponseAsync();
                }
            }
            IList<SendFriendCircleImage> list = new List<SendFriendCircleImage>();
            try
            {
                for (int i = 0; i < fileCount; i++)
                {
                    var file = HttpContext.Current.Request.Files[i];
                    var result = _wechat.SnsUpload(wxId, file.InputStream);
                    if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
                    {
                        throw new Exception("上传失败");
                    }
                    SendFriendCircleImage sendFriendCircleImage = new SendFriendCircleImage()
                    {
                        ClientId = result.ClientId,
                        BufferUrl = result.BufferUrl,
                        ThumbUrls = result.ThumbUrls,
                        Id = result.Id,
                        Type = result.Type,
                    };
                    list.Add(sendFriendCircleImage);
                    response.Data = list;
                }
                response.Message = "上传成功";
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