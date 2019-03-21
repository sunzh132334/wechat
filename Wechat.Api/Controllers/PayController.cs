using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Wechat.Api.Abstracts;
using Wechat.Api.Extensions;
using Wechat.Api.Request;
using Wechat.Api.Request.Pay;
using Wechat.Util.Exceptions;
using Wechat.Util.Extensions;

namespace Wechat.Api.Controllers
{
    /// <summary>
    /// 支付
    /// </summary>
    public class PayController : WebchatControllerBase
    {
        /// <summary>
        /// 获取银行卡信息
        /// </summary>
        /// <param name="wxId">微信Id</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Pay/GetBandCardList/{wxId}")]
        public async Task<HttpResponseMessage> GetBandCardList(string wxId)
        {
            ResponseBase<MMPro.MM.TenPayResponse> response = new ResponseBase<MMPro.MM.TenPayResponse>();
            try
            {
                var result = wechat.TenPay(wxId, MMPro.MM.enMMTenPayCgiCmd.MMTENPAY_CGICMD_BIND_QUERY_NEW);
                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.baseResponse.errMsg.@string ?? "获取失败";
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
        /// 创建转账
        /// </summary>
        /// <param name="createPreTransfer"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Pay/CreatePreTransfer")]
        public async Task<HttpResponseMessage> CreatePreTransfer(CreatePreTransfer createPreTransfer)
        {
            ResponseBase<MMPro.MM.TenPayResponse, string> response = new ResponseBase<MMPro.MM.TenPayResponse, string>();
            try
            {
                string tenpayUrl = $"delay_confirm_flag=0&desc={createPreTransfer.Name}&fee={(int)(createPreTransfer.Money * 100) }&fee_type=CNY&pay_scene=31&receiver_name={createPreTransfer.ToWxId}&scene=31&transfer_scene=2";
                var sign = Wechat.Protocol.Util.WCPaySignDES3Encode(tenpayUrl, Guid.NewGuid().ToString().Replace("-", "").ToUpper());
                tenpayUrl += $"&WCPaySign={sign}";
                var result = wechat.TenPay(createPreTransfer.WxId, MMPro.MM.enMMTenPayCgiCmd.MMTENPAY_CGICMD_GEN_PRE_TRANSFER, tenpayUrl);

                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.baseResponse.errMsg.@string ?? "创建失败";
                }
                else
                {
                    response.Data = result;
                    response.Result = sign;
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
        /// 确认转账
        /// </summary>
        /// <param name="confirmTransfer"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Pay/ConfirmTransfer")]
        public async Task<HttpResponseMessage> ConfirmTransfer(ConfirmTransfer confirmTransfer)
        {
            ResponseBase<MMPro.MM.TenPayResponse, string> response = new ResponseBase<MMPro.MM.TenPayResponse, string>();
            try
            {
                string tenpayUrl = $"auto_deduct_flag=0&bank_type={confirmTransfer.BankType}&bind_serial={confirmTransfer.BindSerial}&busi_sms_flag=0&flag=3&passwd={confirmTransfer.PayPassword}&pay_scene=37&req_key={confirmTransfer.ReqKey}&use_touch=0";

                var sign = Wechat.Protocol.Util.WCPaySignDES3Encode(tenpayUrl, Guid.NewGuid().ToString().Replace("-", "").ToUpper());
                tenpayUrl += $"&WCPaySign={sign}";
                var result = wechat.TenPay(confirmTransfer.WxId, MMPro.MM.enMMTenPayCgiCmd.MMTENPAY_CGICMD_AUTHEN, tenpayUrl);

                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.baseResponse.errMsg.@string ?? "确认失败";
                }
                else
                {
                    response.Data = result;
                    response.Result = sign;
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
        /// 生成自定义支付二维码
        /// </summary>
        /// <param name="generatePayQCode"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Pay/GeneratePayQCode")]
        public async Task<HttpResponseMessage> GeneratePayQCode(GeneratePayQCode generatePayQCode)
        {
            ResponseBase response = new ResponseBase();
            try
            {
                //string payloadJson = "{\"CgiCmd\":0,\"ReqKey\":\"" + ReqKey + "\",\"PassWord\":\"123456\"}";
                string tenpayUrl = $"delay_confirm_flag=0&desc={generatePayQCode.Name}&fee={(int)(generatePayQCode.Money * 100)}&fee_type=CNY&pay_scene=31&receiver_name={generatePayQCode.WxId}&scene=31&transfer_scene=2";
                var sign = Wechat.Protocol.Util.WCPaySignDES3Encode(tenpayUrl, Guid.NewGuid().ToString().Replace("-", "").ToUpper());
                tenpayUrl += $"&WCPaySign={sign}";
                var result = wechat.TenPay(generatePayQCode.WxId, MMPro.MM.enMMTenPayCgiCmd.MMTENPAY_CGICMD_GET_FIXED_AMOUNT_QRCODE, tenpayUrl);

                if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = result.baseResponse.errMsg.@string ?? "生成失败";
                }
                else
                {
                    var reqText = result.reqText.buffer.ToObj<GeneratePayQCodeReqTest>();
                    if (reqText.retcode == 0)
                    {
                        string url = reqText.pay_url;
                        var image = url.CreateQRCode();                  
                        return await image.ToHttpImageResponseAsync();
                    }
                    else
                    {
                        response.Success = false;
                        response.Code = "501";
                        response.Message = reqText.retmsg ?? "生成失败";
                    }

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