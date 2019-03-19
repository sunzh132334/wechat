using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace Wechat.Api.Response
{
    public class HttpResponseMessageHelper
    {
        public static HttpResponseMessage Get(HttpStatusCode StatusCode, HttpContent Content, IDictionary<string, string> Headers = null, IEnumerable<CookieHeaderValue> Cookies = null)
        {
            var response = CreateHttpResponseMessage();
            response.Content = Content;
            response.StatusCode = StatusCode;
            if (Headers != null)
            {
                foreach (var item in Headers)
                {
                    response.Headers.Add(item.Key, item.Value);
                }
            }
            if (Cookies != null)
            {
                response.Headers.AddCookies(Cookies);
            }
            return response;

        }

        public static HttpResponseMessage GetJsonOk(object obj)
        {
            var jsonStr = JsonConvert.SerializeObject(obj);
            var response = CreateHttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
            return response;
        }

        public static HttpResponseMessage GetJsonOk(string jsonStr)
        { 
            var response = CreateHttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
            return response;
        }

        public static HttpResponseMessage GetFileOk(FileStream file)
        {
            var response = CreateHttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(file);
            var contentType = MimeMapping.GetMimeMapping(Path.GetExtension(file.Name));
            if (string.IsNullOrEmpty(contentType))
            {
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            }
            else
            {
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            }
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = file.Name
            };
            return response;


        }

        public static HttpResponseMessage GetImageOk(byte[] content)
        {
            var response = CreateHttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(content);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            return response;
        }

        public static HttpResponseMessage GetImageOk(Stream content)
        {
            var response = CreateHttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(content);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            return response;
        }

        public static HttpResponseMessage GetNotFound()
        {
            var response = CreateHttpResponseMessage(HttpStatusCode.NotFound);
            return response;
        }

        public static HttpResponseMessage GetClientError()
        {
            var response = CreateHttpResponseMessage(HttpStatusCode.BadRequest);
            return response;
        }

        public static HttpResponseMessage GetServerError()
        {
            var response = CreateHttpResponseMessage(HttpStatusCode.InternalServerError);
            return response;
        }

        public static HttpResponseMessage GetUnauthorized()
        {
            var response = CreateHttpResponseMessage(HttpStatusCode.Unauthorized);
            return response;

        }

        private static HttpResponseMessage CreateHttpResponseMessage(HttpStatusCode StatusCode = HttpStatusCode.OK)
        {
            return new HttpResponseMessage();
        }
    }
}