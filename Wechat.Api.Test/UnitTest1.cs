using System;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;
using Wechat.Util;
using Wechat.Util.Cache;
using Wechat.Util.Log;

namespace Wechat.Api.Test
{
    [TestClass]
    public class UnitTest1
    {


        [TestMethod]
        public void TestCache()
        {
            var cache = RedisCache.CreateInstance();
            var ss = cache.Get($"customer:service:merchant_wechat_mapping_wxid_sarw9d5l0ip622");
            //cache.Add(@"customer:service:merchant:wechat_mapping", "123345655");

            var adad = cache.HashGet("customer_service_merchant_wechat_mapping", "1233456");
            var ccc = cache.HashGet<string>("customer_service_merchant_wechat_mapping", "wxid_sarw9d5l0ip622");

            string key = "TestCache";
            var reuslt = cache.Get<CustomerInfoCache>(key);
            cache.Add(key, new CustomerInfoCache() { WxId = "1111111111TestCache" });
            cache.Add(key, new CustomerInfoCache() { WxId = "1111111111TestCache" });
            var reuslt1 = cache.Get<CustomerInfoCache>(key);
            cache.Remove(key);
            var reuslt2 = cache.Get<CustomerInfoCache>(key);
        }
        [TestMethod]
        public void TestBrokenServerCache()
        {
            var ddddd = Convert.FromBase64String("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");
            var fs = File.Create("D:/dda.slik");
            fs.Write(ddddd, 0, ddddd.Length);
            fs.Close();




        }

        [TestMethod]
        public void TestLog()
        {
            Logger.GetLog<UnitTest1>().Info("错误第哦啊我记得了咯今晚大结局啊我的");

            string ss = "<?xml version=\"1.0\"?>\n<msg>\n\t<videomsg aeskey=\"c72e763d4d743e785fa2d0de1b1eb0ee\" cdnthumbaeskey=\"c72e763d4d743e785fa2d0de1b1eb0ee\" cdnvideourl=\"306b02010004643062020100020432e3e1d802032f54690204b3e0313a02045c9c8a71043d617570766964656f5f323036613764666233376238316232645f313535333736323932375f3136343833353238303331393239333566653735313134300204010800040201000400\" cdnthumburl=\"306b02010004643062020100020432e3e1d802032f54690204b3e0313a02045c9c8a71043d617570766964656f5f323036613764666233376238316232645f313535333736323932375f3136343833353238303331393239333566653735313134300204010800040201000400\" length=\"392677\" playlength=\"4\" cdnthumblength=\"9924\" cdnthumbwidth=\"290\" cdnthumbheight=\"512\" fromusername=\"wxid_xqyjnvihzqyn12\" md5=\"2100082c056b1bcdf16eeb4727c4a0cc\" newmd5=\"b65d1c17670555bae4a0475377d396af\" isad=\"0\" />\n</msg>\n";

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(ss);
            XmlNode msg = xml.SelectSingleNode("/msg").SelectSingleNode("videomsg");
            var ddd = msg.Attributes["length"];

            var dd4d = msg.Attributes["length4"];
        }
    }
}
