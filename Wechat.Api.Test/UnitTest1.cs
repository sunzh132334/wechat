using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wechat.Util.Cache;

namespace Wechat.Api.Test
{
    [TestClass]
    public class UnitTest1
    {


        [TestMethod]
        public void TestCache()
        {
            var cache = CacheHelper.CreateInstance();
            string key = "TestCache";
            var reuslt = cache.Get<CustomerInfoCache>(key)  ;
            cache.Add(key, new CustomerInfoCache() { WxId = "1111111111TestCache" });
            cache.Add(key, new CustomerInfoCache() { WxId = "1111111111TestCache" });
            var reuslt1 = cache.Get<CustomerInfoCache>(key);
            cache.Remove(key);
            var reuslt2 = cache.Get<CustomerInfoCache>(key);    
        }

        public void TestBrokenServerCache()
        {
            var cache = CacheHelper.CreateInstance();
            string key = "TestBrokenServerCache";
            cache.Add(key, new CustomerInfoCache() { WxId = "222222222222TestBrokenServerCache" });
 
            var reuslt1 = cache.Get(key);
         
        }
    }
}
