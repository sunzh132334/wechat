using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Wechat.Api.Abstracts;
using Wechat.Api.Extensions;
using Wechat.Api.Helper;
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
            ResponseBase<IList<MMPro.MM.NewSendMsgRespone>> response = new ResponseBase<IList<MMPro.MM.NewSendMsgRespone>>();
            try
            {
                IList<MMPro.MM.NewSendMsgRespone> list = new List<MMPro.MM.NewSendMsgRespone>();
                foreach (var item in txtMessage.ToWxIds)
                {
                    var result = wechat.SendNewMsg(txtMessage.WxId, item, txtMessage.Content);
                    list.Add(result);
                }

                response.Data = list;
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
        public async Task<HttpResponseMessage> SendVoiceMessage(VoiceMessage voiceMessage)
        {
            ResponseBase<IList<MMPro.MM.UploadVoiceResponse>> response = new ResponseBase<IList<MMPro.MM.UploadVoiceResponse>>();
            try
            {
                IList<MMPro.MM.UploadVoiceResponse> list = new List<MMPro.MM.UploadVoiceResponse>();
                byte[] buffer = await FileStorageHelper.DownloadToBufferAsync(voiceMessage.ObjectName);
                foreach (var item in voiceMessage.ToWxIds)
                {
                    var result = wechat.SendVoiceMessage(voiceMessage.WxId, item, buffer, voiceMessage.FileName.GetVoiceType(), voiceMessage.VoiceSecond * 100);
                    list.Add(result);
                }
                response.Data = list;
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
        public async Task<HttpResponseMessage> SendImageMessage(ImageMessage imageMessage)
        {
            ResponseBase<IList<MMPro.MM.UploadMsgImgResponse>> response = new ResponseBase<IList<MMPro.MM.UploadMsgImgResponse>>();
            try
            {
                IList<MMPro.MM.UploadMsgImgResponse> list = new List<MMPro.MM.UploadMsgImgResponse>();
                byte[] buffer = await FileStorageHelper.DownloadToBufferAsync(imageMessage.ObjectName);
                foreach (var item in imageMessage.ToWxIds)
                {
                    var result = wechat.SendImageMessage(imageMessage.WxId, item, buffer);
                    list.Add(result);
                }
                response.Data = list;
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
        public async Task<HttpResponseMessage> SendVideoMessage(VideoMessage videoMessage)
        {
            ResponseBase<IList<micromsg.UploadVideoResponse>> response = new ResponseBase<IList<micromsg.UploadVideoResponse>>();
            try
            {
                IList<micromsg.UploadVideoResponse> list = new List<micromsg.UploadVideoResponse>();
                byte[] buffer = await FileStorageHelper.DownloadToBufferAsync(videoMessage.ObjectName);
                foreach (var item in videoMessage.ToWxIds)
                {
                    var result = wechat.SendVideoMessage(videoMessage.WxId, item, buffer);
                    list.Add(result);
                }
                response.Data = list;
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
        [Route("api/Message/SendAppMessage")]
        public async Task<HttpResponseMessage> SendAppMessage(AppMessage appMessage)
        {
            ResponseBase<IList<micromsg.SendAppMsgResponse>> response = new ResponseBase<IList<micromsg.SendAppMsgResponse>>();
            try
            {
                IList<micromsg.SendAppMsgResponse> list = new List<micromsg.SendAppMsgResponse>();
                string dataUrl = string.IsNullOrEmpty(appMessage.DataUrl) ? appMessage.Url : appMessage.DataUrl;
                string appMessageFormat = $"<appmsg appid=\"{appMessage.AppId}\" sdkver=\"0\"><title>{appMessage.Title}</title><des>{appMessage.Desc}</des><type>{appMessage.Type}</type><showtype>0</showtype><soundtype>0</soundtype><contentattr>0</contentattr><url>{appMessage.Url}</url><lowurl>{appMessage.Url}</lowurl><dataurl>{dataUrl}</dataurl><lowdataurl>{dataUrl}</lowdataurl> <thumburl>{appMessage.ThumbUrl}</thumburl></appmsg>";
                foreach (var item in appMessage.ToWxIds)
                {
                    var result = wechat.SendAppMsg(appMessageFormat, item, appMessage.WxId);
                    list.Add(result);
                }
                response.Data = list;
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
            ResponseBase<IList<micromsg.SendAppMsgResponse>> response = new ResponseBase<IList<micromsg.SendAppMsgResponse>>();
            try
            {
                IList<micromsg.SendAppMsgResponse> list = new List<micromsg.SendAppMsgResponse>();
                string dataUrl = string.IsNullOrEmpty(appMessage.DataUrl) ? appMessage.Url : appMessage.DataUrl;
                string appMessageFormat = $"<appmsg  sdkver=\"0\"><title>{appMessage.Title}</title><des>{appMessage.Desc}</des><type>{appMessage.Type}</type><showtype>0</showtype><soundtype>0</soundtype><contentattr>0</contentattr><url>{appMessage.Url}</url><lowurl>{appMessage.Url}</lowurl><dataurl>{dataUrl}</dataurl><lowdataurl>{dataUrl}</lowdataurl> <thumburl>{appMessage.ThumbUrl}</thumburl></appmsg>";
                foreach (var item in appMessage.ToWxIds)
                {
                    var result = wechat.SendAppMsg(appMessageFormat, item, appMessage.WxId);
                    list.Add(result);
                }
                response.Data = list;
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
            ResponseBase<IList<MMPro.MM.NewSendMsgRespone>> response = new ResponseBase<IList<MMPro.MM.NewSendMsgRespone>>();
            try
            {
                IList<MMPro.MM.NewSendMsgRespone> list = new List<MMPro.MM.NewSendMsgRespone>();
                cardMessage.CardNickName = string.IsNullOrEmpty(cardMessage.CardNickName) ? cardMessage.CardWxId : cardMessage.CardNickName;
                string appMessageFormat = $"<?xml version=\"1.0\"?>\n<msg bigheadimgurl=\"\" smallheadimgurl=\"\" username=\"{cardMessage.CardWxId}\" nickname=\"{cardMessage.CardNickName}\" fullpy=\"\" shortpy=\"\" alias=\"{cardMessage.CardAlias}\" imagestatus=\"0\" scene=\"17\" province=\"\" city=\"\" sign=\"\" sex=\"2\" certflag=\"0\" certinfo=\"\" brandIconUrl=\"\" brandHomeUrl=\"\" brandSubscriptConfigUrl=\"\" brandFlags=\"0\" regionCode=\"CN\" />\n";
                foreach (var item in cardMessage.ToWxIds)
                {
                    var result = wechat.SendNewMsg(cardMessage.WxId, item, appMessageFormat, 42);
                    list.Add(result);
                }
                response.Data = list;
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
            ResponseBase<IList<MMPro.MM.NewSendMsgRespone>> response = new ResponseBase<IList<MMPro.MM.NewSendMsgRespone>>();
            try
            {
                IList<MMPro.MM.NewSendMsgRespone> list = new List<MMPro.MM.NewSendMsgRespone>();
                string appMessageFormat = $"<?xml version=\"1.0\"?>\n<msg>\n\t<location x=\"{cardMessage.Latitude}\" y=\"{cardMessage.Longitude}\" scale=\"16\" label=\"{cardMessage.Name}\" maptype=\"0\" poiname=\"[位置]{cardMessage.Name}\" poiid=\"\" />\n</msg>";
                foreach (var item in cardMessage.ToWxIds)
                {
                    var result = wechat.SendNewMsg(cardMessage.WxId, item, appMessageFormat, 48);
                    list.Add(result);
                }
                response.Data = list;
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