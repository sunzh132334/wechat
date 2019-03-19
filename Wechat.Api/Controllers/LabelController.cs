using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Wechat.Api.Abstracts;
using Wechat.Api.Extensions;
using Wechat.Api.Request.Label;
using Wechat.Api.Request.Message;
using Wechat.Protocol;
using Wechat.Util.Exceptions;
using Wechat.Util.Extensions;

namespace Wechat.Api.Controllers
{
    /// <summary>
    /// 标签
    /// </summary>
    public class LabelController : WebchatControllerBase
    {

        private WechatHelper _wechat = null;
        public LabelController()
        {
            _wechat = new WechatHelper();
        }

        /// <summary>
        /// 获取标签列表
        /// </summary>
        /// <param name="wxId"></param>
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Label/GetLableList/{wxId}")]
        public Task<HttpResponseMessage> GetLableList(string wxId)
        {
            ResponseBase<MMPro.MM.LabelPair[]> response = new ResponseBase<MMPro.MM.LabelPair[]>();
            try
            {
                var result = _wechat.GetContactLabelList(wxId);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                     response.Success = false;
                    response.Code = "501";
                    response.Message = "获取失败";
                }
                else
                {
                    response.Data = result.labelPairList;
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
        /// 添加标签
        /// </summary>
        /// <param name="addLabel"></param>
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Label/AddLabelName")]
        public Task<HttpResponseMessage> AddLabelName(AddLabel addLabel)
        {
            ResponseBase<IList<micromsg.LabelPair>> response = new ResponseBase<IList<micromsg.LabelPair>>();
            try
            {
                var result = _wechat.AddContactLabel(addLabel.WxId, addLabel.LabelName);
                if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                     response.Success = false;
                    response.Code = "501";
                    response.Message = "添加失败";
                }
                else
                {
                    response.Data = result.LabelPairList;
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
            return response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 修改标签
        /// </summary>
        /// <param name="updateLabel"></param>
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Label/UpdateLabelName")]
        public Task<HttpResponseMessage> UpdateLabelName(UpdateLabel updateLabel)
        {
            ResponseBase response = new ResponseBase();
            try
            {
                micromsg.UserLabelInfo[] userLabels = new micromsg.UserLabelInfo[1];
                userLabels[0] = new micromsg.UserLabelInfo();
                userLabels[0].LabelIDList = updateLabel.LabelIDList;
                userLabels[0].UserName = updateLabel.ToWxId;
                var result = _wechat.ModifyContactLabelList(updateLabel.WxId, userLabels);
                if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                     response.Success = false;
                    response.Code = "501";
                    response.Message = "修改失败";
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
        /// 删除标签
        /// </summary>
        /// <param name="deleteLabel"></param>
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Label/DeleteLabelName")]
        public Task<HttpResponseMessage> DeleteLabelName(DeleteLabel deleteLabel)
        {
            ResponseBase response = new ResponseBase();
            try
            {   
                var result = _wechat.DelContactLabel(deleteLabel.WxId, deleteLabel.LabelIDList);
                if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                     response.Success = false;
                    response.Code = "501";
                    response.Message = "删除失败";
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
            return response.ToHttpResponseAsync();
        }
    }
}