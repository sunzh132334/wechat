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

        /// <summary>
        /// 获取标签列表
        /// </summary>
        /// <param name="wxId">微信Id</param>
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Label/GetLableList/{wxId}")]
        public Task<HttpResponseMessage> GetLableList(string wxId)
        {
            ResponseBase<MMPro.MM.LabelPair[]> response = new ResponseBase<MMPro.MM.LabelPair[]>();
            try
            {
                var result = wechat.GetContactLabelList(wxId);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.baseResponse.errMsg.@string ?? "获取失败";
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
                var result = wechat.AddContactLabel(addLabel.WxId, addLabel.LabelName);
                if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.BaseResponse.ErrMsg.String ?? "添加失败";
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
        /// 批量修改标签
        /// </summary>
        /// <param name="batchUpdateLabel"></param>
        /// <returns></returns>
        [HttpPost()]
        [Route("api/Label/BatchUpdateLabelName")]
        public Task<HttpResponseMessage> BatchUpdateLabelName(BatchUpdateLabel batchUpdateLabel)
        {
            ResponseBase response = new ResponseBase();
            try
            {
                micromsg.UserLabelInfo[] userLabels = new micromsg.UserLabelInfo[batchUpdateLabel.ToWxIds.Count];
                for (int i = 0; i < batchUpdateLabel.ToWxIds.Count; i++)
                {           
                    userLabels[i] = new micromsg.UserLabelInfo();
                    userLabels[i].LabelIDList = batchUpdateLabel.LabelIDList;
                    userLabels[i].UserName = batchUpdateLabel.ToWxIds[i];
               
                }
                var result = wechat.ModifyContactLabelList(batchUpdateLabel.WxId, userLabels);
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
            return response.ToHttpResponseAsync();
        }


        /// <summary>
        /// 修改标签
        /// </summary>
        /// <param name="updateLabel"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> UpdateLabelName(UpdateLabel updateLabel)
        {
            ResponseBase response = new ResponseBase();
            try
            {
                micromsg.UserLabelInfo[] userLabels = new micromsg.UserLabelInfo[1];
                userLabels[0] = new micromsg.UserLabelInfo();
                userLabels[0].LabelIDList = updateLabel.LabelIDList;
                userLabels[0].UserName = updateLabel.ToWxId;
                var result = wechat.ModifyContactLabelList(updateLabel.WxId, userLabels);
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
                var result = wechat.DelContactLabel(deleteLabel.WxId, deleteLabel.LabelIDList);
                if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.BaseResponse.ErrMsg.String ?? "删除失败";
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