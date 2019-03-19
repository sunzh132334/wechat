using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Wechat.Api.Response;

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

        public static Task<HttpResponseMessage> ToHttpResponseAsync(this byte[] response)
        {
            return Task.FromResult(HttpResponseMessageHelper.GetImageOk(response));
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