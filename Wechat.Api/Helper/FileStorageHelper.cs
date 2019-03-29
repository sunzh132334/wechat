using Aliyun.OSS;
using Aliyun.OSS.Common;
using Aliyun.OSS.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Wechat.Api.Helper
{
    /// <summary>
    /// 文件存储
    /// </summary>
    public class FileStorageHelper
    {
         

        static string accessKeyId = "LTAIAAsIQLWukENf";
        static string accessKeySecret = "5R3nTfp0ZdWmH9EqFJrmJnK782N0Rd";
        static string bucketName = "oss-xiake-scrm";
        static string endpoint = "http://oss-cn-shenzhen.aliyuncs.com/";
        static int maxErrorRetry = 3;
        static int connectionTimeout = 300;


        /// <summary>
        /// 获取objectName
        /// </summary>
        /// <param name="MchId"></param>
        /// <returns></returns>
        public static string GetObjectName(string MchId)
        {
            return $"wechat_message/{MchId}/wechat_message/";
        }
        /// <summary>
        /// 创建对象
        /// </summary>
        /// <returns></returns>
        public static OssClient CreateOssClient()
        {
            var conf = new ClientConfiguration();
            conf.MaxErrorRetry = maxErrorRetry;
            conf.ConnectionTimeout = connectionTimeout;
            OssClient ossClient = new OssClient(endpoint, accessKeyId, accessKeySecret);
            return ossClient;
        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="objectName"></param>
        /// <returns></returns>
        public static byte[] DownloadToBuffer(string objectName)
        {
            byte[] buffer = null;
            try
            {
                var client = CreateOssClient();
                // 下载文件到流。OssObject 包含了文件的各种信息，如文件所在的存储空间、文件名、元信息以及一个输入流。
                var obj = client.GetObject(bucketName, objectName);
                using (var requestStream = obj.Content)
                {
                    buffer = new byte[requestStream.Length];
                    requestStream.Read(buffer, 0, buffer.Length);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Get object failed. {0}", ex.Message);
            }
            return buffer;
        }
        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="objectName"></param>
        /// <returns></returns>
        public static Stream DownloadToStream(string objectName)
        {
            Stream sm = null;
            try
            {
                var client = CreateOssClient();
                // 下载文件到流。OssObject 包含了文件的各种信息，如文件所在的存储空间、文件名、元信息以及一个输入流。
                var obj = client.GetObject(bucketName, objectName);
                sm = obj.Content;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Get object failed. {0}", ex.Message);
            }
            return sm;
        }
        /// <summary>
        /// 异步下载
        /// </summary> 
        /// <param name="objectName"></param>
        /// <returns></returns>
        public static async Task<byte[]> DownloadToBufferAsync(string objectName)
        {
            byte[] buffer = null;
            try
            {
                var client = CreateOssClient();
                // 下载文件到流。OssObject 包含了文件的各种信息，如文件所在的存储空间、文件名、元信息以及一个输入流。
                var obj = client.GetObject(bucketName, objectName);
                using (var requestStream = obj.Content)
                {
                    buffer = new byte[requestStream.Length];
                    await requestStream.ReadAsync(buffer, 0, buffer.Length);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Get object failed. {0}", ex.Message);
            }
            return buffer;
        }

        /// <summary>
        /// 上传
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sm"></param>
        /// <param name="objectName"></param>
        public static void Upload(string fileName, Stream sm, string objectName)
        {
            try
            {
                // 上传文件。
                var client = CreateOssClient();
            
                var result = client.PutObject(bucketName, objectName, sm);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Get object failed. {0}", ex.Message);
            }

        }

        /// <summary>
        /// 上传
        /// </summary>    
        /// <param name="buffer"></param>
        /// <param name="objectName"></param>
        public static void Upload( byte[] buffer, string objectName)
        {
            try
            {
                // 上传文件。
                var client = CreateOssClient();

                var result = client.PutObject(bucketName, objectName, new MemoryStream(buffer));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Get object failed. {0}", ex.Message);
            }

        }
    }
}