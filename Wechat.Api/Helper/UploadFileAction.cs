using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wechat.Protocol;
using Wechat.Util;
using Wechat.Util.Cache;

namespace Wechat.Api.Helper
{

    public class UploadFileAction
    {

        public static string UploadFile(UploadFileObj uploadFileObj)
        {
            string objName = null;
            string mchId = RedisCache.CreateInstance().Get(ConstCacheKey.GetMchIdKey(uploadFileObj.WxId));
            if (string.IsNullOrEmpty(mchId))
            {
                mchId = "00000000000";
            }
            WechatHelper wechatHelper = new WechatHelper();
            //图片
            if (uploadFileObj.MsgType == 3)
            {
                byte[] buffer = wechatHelper.GetMsgBigImg(uploadFileObj.LongDataLength, uploadFileObj.MsgId, uploadFileObj.WxId, uploadFileObj.ToWxId, 0, (int)uploadFileObj.LongDataLength);
                if (buffer != null)
                {
                    objName = FileStorageHelper.GetObjectName(mchId);
                    FileStorageHelper.Upload(buffer, $"{objName}{uploadFileObj.MsgId}.png");
                }
            }
            //语音
            else if (uploadFileObj.MsgType == 34)
            {

                if (uploadFileObj.Buffer != null)
                {
                    objName = FileStorageHelper.GetObjectName(mchId);
                    FileStorageHelper.Upload(uploadFileObj.Buffer, $"{objName}{uploadFileObj.MsgId}.silk");
                }


            }
            //视频
            else if (uploadFileObj.MsgType == 43)
            {
                byte[] buffer = wechatHelper.GetVideo(uploadFileObj.WxId, uploadFileObj.ToWxId, uploadFileObj.MsgId, uploadFileObj.LongDataLength, 0, (int)uploadFileObj.LongDataLength);

                if (buffer != null)
                {
                    objName = FileStorageHelper.GetObjectName(mchId);
                    FileStorageHelper.Upload(buffer, $"{objName}{uploadFileObj.MsgId}.mp4");
                }


            }

            return objName;

        }


    }
}