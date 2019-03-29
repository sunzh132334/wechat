using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Wechat.Api.Response;
using Wechat.Util.Extensions;

namespace Wechat.Api.Extensions
{
    public static class Extension
    {
 
        public static HttpResponseMessage ToHttpResponse(this ResponseBase response)
        {
            return HttpResponseMessageHelper.GetJsonOk(response);
        }

        public static Task<HttpResponseMessage> ToHttpResponseAsync(this ResponseBase response)
        {
            return Task.FromResult(HttpResponseMessageHelper.GetJsonOk(response));
        }

        public static Task<HttpResponseMessage> ToHttpImageResponseAsync(this byte[] response)
        {
            return Task.FromResult(HttpResponseMessageHelper.GetImageOk(response));
        }


        public static Task<HttpResponseMessage> ToHttpVideoResponseAsync(this byte[] response)
        {
            return Task.FromResult(HttpResponseMessageHelper.GetVideoOk(response));
        }

        public static Task<HttpResponseMessage> ToHttpImageResponseAsync(this Image image)
        {
            return Task.FromResult(HttpResponseMessageHelper.GetImageOk(image.ImageToByteArray()));
        }

        public static MMPro.MM.VoiceFormat GetVoiceType(this string fileName)
        {
            MMPro.MM.VoiceFormat voiceFormat = MMPro.MM.VoiceFormat.MM_VOICE_FORMAT_UNKNOWN;
            string extension = Path.GetExtension(fileName).ToLower();
            switch (extension)
            {
                case ".wav":voiceFormat = MMPro.MM.VoiceFormat.MM_VOICE_FORMAT_WAVE;break;
                case ".mp3": voiceFormat = MMPro.MM.VoiceFormat.MM_VOICE_FORMAT_MP3; break;
                case ".silk": voiceFormat = MMPro.MM.VoiceFormat.MM_VOICE_FORMAT_SILK; break;
                case ".speex ": voiceFormat = MMPro.MM.VoiceFormat.MM_VOICE_FORMAT_WAVE; break;
                case ".amr": voiceFormat = MMPro.MM.VoiceFormat.MM_VOICE_FORMAT_AMR; break;
            }         
            return voiceFormat;
        }
    }
}