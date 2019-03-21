using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Wechat.Api.Abstracts;
using Wechat.Api.Extensions;
using Wechat.Api.Request.Message;
using Wechat.Protocol;
using Wechat.Util.Exceptions;
using Wechat.Util.Extensions;

namespace Wechat.Api.Controllers
{
    /// <summary>
    /// 消息服务
    /// </summary>
    public class MessageController : WebchatControllerBase
    {

        /// <summary>
        /// 发送文本信息
        /// </summary>
        /// <param name="txtMessage"></param>
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Message/SendTxtMessage")]
        public Task<HttpResponseMessage> SendTxtMessage(TxtMessage txtMessage)
        {
            ResponseBase<MMPro.MM.NewSendMsgRespone> response = new ResponseBase<MMPro.MM.NewSendMsgRespone>();
            try
            {
                var result = wechat.SendNewMsg(txtMessage.WxId, txtMessage.ToWxId, txtMessage.Content);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.baseResponse.errMsg.@string ?? "发送失败";
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
        /// 发送声音消息
        /// </summary> 
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Message/SendVoiceMessage")]
        public async Task<HttpResponseMessage> SendVoiceMessage()
        {
            ResponseBase<MMPro.MM.UploadVoiceResponse> response = new ResponseBase<MMPro.MM.UploadVoiceResponse>();
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
                return await response.ToHttpResponseAsync();
            }
            var file = HttpContext.Current.Request.Files[0];
            if (file.FileName.IsVoice())
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "请上传声音文件";
                return await response.ToHttpResponseAsync();
            }
            var wxId = HttpContext.Current.Request["WxId"];
            var toWxId = HttpContext.Current.Request["ToWxId"];
            var voiceSecond = HttpContext.Current.Request["VoiceSecond"];
            if (string.IsNullOrEmpty(wxId))
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "WxId不能为空";
                return await response.ToHttpResponseAsync();
            }
            if (string.IsNullOrEmpty(toWxId))
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "ToWxId不能为空";
                return await response.ToHttpResponseAsync();
            }
            if (string.IsNullOrEmpty(voiceSecond))
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "VoiceSecond不能为空";
                return await response.ToHttpResponseAsync();
            }
            try
            {
                byte[] buffer = await file.InputStream.ToBufferAsync();

                var result = wechat.SendVoiceMessage(wxId, toWxId, buffer, file.FileName.GetVoiceType(), Convert.ToInt32(string.IsNullOrEmpty(voiceSecond) ? "1" : voiceSecond) * 100);


                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.baseResponse.errMsg.@string ?? "发送失败";
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
            return await response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 发送图片消息
        /// </summary> 
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Message/SendImageMessage")]
        public async Task<HttpResponseMessage> SendImageMessage()
        {
            ResponseBase<MMPro.MM.UploadMsgImgResponse> response = new ResponseBase<MMPro.MM.UploadMsgImgResponse>();
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
            var file = HttpContext.Current.Request.Files[0];
            if (file.FileName.IsImage())
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "请上传图片文件";
                return await response.ToHttpResponseAsync();
            }
            var wxId = HttpContext.Current.Request["WxId"];
            var toWxId = HttpContext.Current.Request["ToWxId"];
            if (string.IsNullOrEmpty(wxId))
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "WxId不能为空";
                return await response.ToHttpResponseAsync();
            }
            if (string.IsNullOrEmpty(toWxId))
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "ToWxId不能为空";
                return await response.ToHttpResponseAsync();
            }
            try
            {

                byte[] buffer = await file.InputStream.ToBufferAsync();

                var result = wechat.SendImageMessage(wxId, toWxId, buffer);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.baseResponse.errMsg.@string;
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
            return await response.ToHttpResponseAsync();
        }



        /// <summary>
        /// 发送视频消息
        /// </summary>       
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Message/SendVideoMessage")]
        public async Task<HttpResponseMessage> SendVideoMessage()
        {
            ResponseBase<micromsg.UploadVideoResponse> response = new ResponseBase<micromsg.UploadVideoResponse>();
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
            var file = HttpContext.Current.Request.Files[0];
            if (file.FileName.IsVideo())
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "请上传视频文件";
                return await response.ToHttpResponseAsync();
            }
            var wxId = HttpContext.Current.Request["WxId"];
            var toWxId = HttpContext.Current.Request["ToWxId"];
            if (string.IsNullOrEmpty(wxId))
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "WxId不能为空";
                return await response.ToHttpResponseAsync();
            }
            if (string.IsNullOrEmpty(toWxId))
            {
                response.Success = false;
                response.Code = "400";
                response.Message = "ToWxId不能为空";
                return await response.ToHttpResponseAsync();
            }
            try
            {

                byte[] buffer = await file.InputStream.ToBufferAsync();

                var result = wechat.SendVideoMessage(wxId, toWxId, buffer);
                if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.BaseResponse.ErrMsg.String ?? "发送失败";
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
            return await response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 发送App消息
        /// </summary>
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Message/AppMessage")]
        public async Task<HttpResponseMessage> SendAppMessage(AppMessage appMessage)
        {
            ResponseBase<micromsg.SendAppMsgResponse> response = new ResponseBase<micromsg.SendAppMsgResponse>();
            try
            {
                string dataUrl = string.IsNullOrEmpty(appMessage.DataUrl) ? appMessage.Url : appMessage.DataUrl;
                string appMessageFormat = $"<appmsg appid=\"{appMessage.AppId}\" sdkver=\"0\"><title>{appMessage.Title}</title><des>{appMessage.Desc}</des><type>{appMessage.Type}</type><showtype>0</showtype><soundtype>0</soundtype><contentattr>0</contentattr><url>{appMessage.Url}</url><lowurl>{appMessage.Url}</lowurl><dataurl>{dataUrl}</dataurl><lowdataurl>{dataUrl}</lowdataurl> <thumburl>{appMessage.ThumbUrl}</thumburl></appmsg>";

                var result = wechat.SendAppMsg(appMessageFormat, appMessage.ToWxId, appMessage.WxId);
                if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.BaseResponse.ErrMsg.String ?? "发送失败";
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
            return await response.ToHttpResponseAsync();
        }



        /// <summary>
        /// 发送分享消息
        /// </summary>
        /// <param name="appMessage"></param>
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Message/SendShareMessage")]
        public async Task<HttpResponseMessage> SendShareMessage(ShareMessage appMessage)
        {
            ResponseBase<micromsg.SendAppMsgResponse> response = new ResponseBase<micromsg.SendAppMsgResponse>();
            try
            {
                string dataUrl = string.IsNullOrEmpty(appMessage.DataUrl) ? appMessage.Url : appMessage.DataUrl;
                string appMessageFormat = $"<appmsg  sdkver=\"0\"><title>{appMessage.Title}</title><des>{appMessage.Desc}</des><type>{appMessage.Type}</type><showtype>0</showtype><soundtype>0</soundtype><contentattr>0</contentattr><url>{appMessage.Url}</url><lowurl>{appMessage.Url}</lowurl><dataurl>{dataUrl}</dataurl><lowdataurl>{dataUrl}</lowdataurl> <thumburl>{appMessage.ThumbUrl}</thumburl></appmsg>";

                var result = wechat.SendAppMsg(appMessageFormat, appMessage.ToWxId, appMessage.WxId);
                if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.BaseResponse.ErrMsg.String ?? "发送失败";
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
            return await response.ToHttpResponseAsync();
        }



        /// <summary>
        /// 发送卡片消息
        /// </summary>
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Message/SendCardMessage")]
        public async Task<HttpResponseMessage> SendCardMessage(CardMessage cardMessage)
        {
            ResponseBase<MMPro.MM.NewSendMsgRespone> response = new ResponseBase<MMPro.MM.NewSendMsgRespone>();
            try
            {
                cardMessage.CardNickName = string.IsNullOrEmpty(cardMessage.CardNickName) ? cardMessage.CardWxId : cardMessage.CardNickName;
                string appMessageFormat = $"<?xml version=\"1.0\"?>\n<msg bigheadimgurl=\"\" smallheadimgurl=\"\" username=\"{cardMessage.CardWxId}\" nickname=\"{cardMessage.CardNickName}\" fullpy=\"\" shortpy=\"\" alias=\"{cardMessage.CardAlias}\" imagestatus=\"0\" scene=\"17\" province=\"\" city=\"\" sign=\"\" sex=\"2\" certflag=\"0\" certinfo=\"\" brandIconUrl=\"\" brandHomeUrl=\"\" brandSubscriptConfigUrl=\"\" brandFlags=\"0\" regionCode=\"CN\" />\n";

                var result = wechat.SendNewMsg(cardMessage.WxId, cardMessage.ToWxId, appMessageFormat, 42);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.baseResponse.errMsg.@string ?? "发送失败";
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
            return await response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 发送位置信息
        /// </summary>
        /// <param name="cardMessage"></param>
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Message/SendLocationMessage")]
        public async Task<HttpResponseMessage> SendLocationMessage(LocationMessage cardMessage)
        {
            ResponseBase<MMPro.MM.NewSendMsgRespone> response = new ResponseBase<MMPro.MM.NewSendMsgRespone>();
            try
            {
                string appMessageFormat = $"<?xml version=\"1.0\"?>\n<msg>\n\t<location x=\"{cardMessage.Latitude}\" y=\"{cardMessage.Longitude}\" scale=\"16\" label=\"{cardMessage.Name}\" maptype=\"0\" poiname=\"[位置]{cardMessage.Name}\" poiid=\"\" />\n</msg>";

                var result = wechat.SendNewMsg(cardMessage.WxId, cardMessage.ToWxId, appMessageFormat, 48);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.baseResponse.errMsg.@string ?? "发送失败";
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
            return await response.ToHttpResponseAsync();
        }

    }
}