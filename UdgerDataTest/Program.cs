using System;
using System.IO;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using Udger.Parser.V3;

namespace UdgerDataTest
{
    class Program
    {
        private static void AssertString(string name, string actual, object expected)
        {
            if (SetNullEmpty(actual) != Convert.ToString(expected))
                Console.WriteLine($"err {name}: Actual: {actual} Expected {expected}");
        }

        static void Main(string[] args)
        {

            WebClient client;
            string tempPath = Path.GetTempPath();

            //#region Download test data
            Console.WriteLine("download test data from github");
            
            client = new WebClient();
            client.DownloadFile("https://github.com/udger/test-data/blob/master/data_v3/udgerdb_v3.dat?raw=true", tempPath + "udgerdb_v3.dat");
            client.DownloadFile("https://raw.githubusercontent.com/udger/test-data/master/data_v3/test_ua.json", tempPath + "test_ua.json");
            client.DownloadFile("https://raw.githubusercontent.com/udger/test-data/master/data_v3/test_ip.json", tempPath + "test_ip.json");

            Console.WriteLine("download test data end");
            //#endregion

            #region New parser
            UdgerParser parser = new UdgerParser(tempPath + "udgerdb_v3.dat");
            #endregion

            #region test IP
            var jsonString = File.ReadAllText(tempPath + "test_ip.json");
            dynamic jsonResult = JsonConvert.DeserializeObject(jsonString);

            foreach (dynamic x in jsonResult)
            {
                Console.WriteLine("test IP: " + x.test.teststring);
                IpResult i = parser.ParseIp(Convert.ToString(x.test.teststring));

                if (SetNullEmpty(i.CrawlerCategory) != Convert.ToString(x.ret.crawler_category))
                    Console.WriteLine("err CrawlerCategory: " + i.CrawlerCategory);

                if (SetNullEmpty(i.CrawlerCategoryCode) != Convert.ToString(x.ret.crawler_category_code))
                    Console.WriteLine("err CrawlerCategoryCode: " + i.CrawlerCategoryCode);

                if (SetNullEmpty(i.CrawlerFamily) != Convert.ToString(x.ret.crawler_family))
                    Console.WriteLine("err CrawlerFamily: " + i.CrawlerFamily);

                if (SetNullEmpty(i.CrawlerFamilyCode) != Convert.ToString(x.ret.crawler_family_code))
                    Console.WriteLine("err CrawlerFamilyCode: " + i.CrawlerFamilyCode);

                if (HttpUtility.UrlDecode(SetNullEmpty(i.CrawlerFamilyHomepage)) != Convert.ToString(x.ret.crawler_family_homepage))
                    Console.WriteLine("err CrawlerFamilyHomepage: " + i.CrawlerFamilyHomepage);

                if (SetNullEmpty(i.CrawlerFamilyIcon) != Convert.ToString(x.ret.crawler_family_icon))
                    Console.WriteLine("err CrawlerFamilyIcon: " + i.CrawlerFamilyIcon);

                if (HttpUtility.UrlDecode(SetNullEmpty(i.CrawlerFamilyInfoUrl)) != Convert.ToString(x.ret.crawler_family_info_url))
                    Console.WriteLine("err CrawlerFamilyInfoUrl: " + i.CrawlerFamilyInfoUrl);

                if (SetNullEmpty(i.CrawlerFamilyVendor) != Convert.ToString(x.ret.crawler_family_vendor))
                    Console.WriteLine("err CrawlerFamilyVendor: " + i.CrawlerFamilyVendor);

                if (SetNullEmpty(i.CrawlerFamilyVendorCode) != Convert.ToString(x.ret.crawler_family_vendor_code))
                    Console.WriteLine("err CrawlerFamilyVendorCode: " + i.CrawlerFamilyVendorCode);

                if (HttpUtility.UrlDecode(SetNullEmpty(i.CrawlerFamilyVendorHomepage)) != Convert.ToString(x.ret.crawler_family_vendor_homepage))
                    Console.WriteLine("err CrawlerFamilyVendorHomepage: " + i.CrawlerFamilyVendorHomepage);
                /*
                if (setNullEmpty(i.CrawlerLastSeen) != Convert.ToString(x.ret.crawler_category))
                    Console.WriteLine("err CrawlerLastSeen: " + i.CrawlerLastSeen);
                */
                if (SetNullEmpty(i.CrawlerName) != Convert.ToString(x.ret.crawler_name))
                    Console.WriteLine("err CrawlerName: " + i.CrawlerName);

                if (SetNullEmpty(i.CrawlerRespectRobotstxt) != Convert.ToString(x.ret.crawler_respect_robotstxt))
                    Console.WriteLine("err CrawlerRespectRobotstxt: " + i.CrawlerRespectRobotstxt);

                if (SetNullEmpty(i.CrawlerVer) != Convert.ToString(x.ret.crawler_ver))
                    Console.WriteLine("err CrawlerVer: " + i.CrawlerVer);

                if (SetNullEmpty(i.CrawlerVerMajor) != Convert.ToString(x.ret.crawler_ver_major))
                    Console.WriteLine("err CrawlerVerMajor: " + i.CrawlerVerMajor);

                if (HttpUtility.UrlDecode(SetNullEmpty(i.DataCenterHomePage)) != Convert.ToString(x.ret.datacenter_homepage))
                    Console.WriteLine("err DatacenterHomepage: " + i.DataCenterHomePage);

                if (SetNullEmpty(i.DataCenterName) != Convert.ToString(x.ret.datacenter_name))
                    Console.WriteLine("err DatacenterName: " + i.DataCenterName);

                if (SetNullEmpty(i.DataCenterNameCode) != Convert.ToString(x.ret.datacenter_name_code))
                    Console.WriteLine("err DatacenterNameCode: " + i.DataCenterNameCode);

                if (SetNullEmpty(i.Ip) != Convert.ToString(x.ret.ip))
                    Console.WriteLine("err Ip: " + i.Ip);

                if (SetNullEmpty(i.IpCity) != Convert.ToString(x.ret.ip_city))
                    Console.WriteLine("err IpCity: " + i.IpCity);

                if (SetNullEmpty(i.IpClassification) != Convert.ToString(x.ret.ip_classification))
                    Console.WriteLine("err IpClassification: " + i.IpClassification);

                if (SetNullEmpty(i.IpClassificationCode) != Convert.ToString(x.ret.ip_classification_code))
                    Console.WriteLine("err IpClassificationCode: " + i.IpClassificationCode);

                if (SetNullEmpty(i.IpCountry) != Convert.ToString(x.ret.ip_country))
                    Console.WriteLine("err IpCountry: " + i.IpCountry);

                if (SetNullEmpty(i.IpCountryCode) != Convert.ToString(x.ret.ip_country_code))
                    Console.WriteLine("err IpCountryCode: " + i.IpCountryCode);

                if (SetNullEmpty(i.IpHostname) != Convert.ToString(x.ret.ip_hostname))
                    Console.WriteLine("err IpHostname: " + i.IpHostname);
                /*
                if (setNullEmpty(i.IpLastSeen) != Convert.ToString(x.ret.crawler_category))
                    Console.WriteLine("err IpLastSeen: " + i.IpLastSeen);
                */
                if (SetNullEmpty(i.IpVer.ToString()) != Convert.ToString(x.ret.ip_ver))
                    Console.WriteLine("err IpVer: " + i.IpVer);

            }
            #endregion

            #region test UA
            jsonString = File.ReadAllText(tempPath + "test_ua.json");
            jsonResult = JsonConvert.DeserializeObject(jsonString);

            foreach (dynamic x in jsonResult)
            {
                Console.WriteLine("test UA: " + x.test.teststring);
                UaResult a = parser.ParseUa(Convert.ToString(x.test.teststring));
                if (SetNullEmpty(a.CrawlerCategory) != Convert.ToString(x.ret.crawler_category))
                    Console.WriteLine("err CrawlerCategory: " + a.CrawlerCategory);

                if (SetNullEmpty(a.CrawlerCategoryCode) != Convert.ToString(x.ret.crawler_category_code))
                    Console.WriteLine("err CrawlerCategoryCode: " + a.CrawlerCategoryCode);
                /*
                if (setNullEmpty(a.CrawlerLastSeen) != "")
                    Console.WriteLine("err CrawlerLastSeen: " + a.CrawlerLastSeen);
                */
                if (SetNullEmpty(a.CrawlerRespectRobotstxt) != Convert.ToString(x.ret.crawler_respect_robotstxt))
                    Console.WriteLine("err CrawlerRespectRobotstxt: " + a.CrawlerRespectRobotstxt);

                AssertString("DeviceBrand", a.DeviceBrand, Convert.ToString(x.ret.device_brand));

                if (SetNullEmpty(a.DeviceBrandCode) != Convert.ToString(x.ret.device_brand_code))
                    Console.WriteLine("err DeviceBrandCode: " + a.DeviceBrandCode);

                if (SetNullEmpty(a.DeviceBrandHomepage) != Convert.ToString(x.ret.device_brand_homepage))
                    Console.WriteLine("err DeviceBrandHomepage: " + a.DeviceBrandHomepage);

                if (SetNullEmpty(a.DeviceBrandIcon) != Convert.ToString(x.ret.device_brand_icon))
                    Console.WriteLine("err DeviceBrandIcon: " + a.DeviceBrandIcon);

                if (SetNullEmpty(a.DeviceBrandIconBig) != Convert.ToString(x.ret.device_brand_icon_big))
                    Console.WriteLine("err DeviceBrandIconBig: " + a.DeviceBrandIconBig);

                AssertString("DeviceBrandInfoUrl", a.DeviceBrandInfoUrl, Convert.ToString(x.ret.device_brand_info_url));
                //if (HttpUtility.UrlDecode(setNullEmpty(a.DeviceBrandInfoUrl)) != Convert.ToString(x.ret.device_brand_info_url))
                //    Console.WriteLine("err DeviceBrandInfoUrl: " + a.DeviceBrandInfoUrl);

                AssertString("DeviceClass", a.DeviceClass, x.ret.device_class);
                
                if (SetNullEmpty(a.DeviceClassCode) != Convert.ToString(x.ret.device_class_code))
                    Console.WriteLine("err DeviceClassCode: " + a.DeviceClassCode);

                if (SetNullEmpty(a.DeviceClassIcon) != Convert.ToString(x.ret.device_class_icon))
                    Console.WriteLine("err DeviceClassIcon: " + a.DeviceClassIcon);

                if (SetNullEmpty(a.DeviceClassIconBig) != Convert.ToString(x.ret.device_class_icon_big))
                    Console.WriteLine("err DeviceClassIconBig: " + a.DeviceClassIconBig);

                if (HttpUtility.UrlDecode(SetNullEmpty(a.DeviceClassInfoUrl)) != Convert.ToString(x.ret.device_class_info_url))
                    Console.WriteLine("err DeviceClassInfoUrl: " + a.DeviceClassInfoUrl);

                if (SetNullEmpty(a.DeviceMarketname) != Convert.ToString(x.ret.device_marketname))
                    Console.WriteLine("err DeviceMarketname: " + a.DeviceMarketname);

                if (SetNullEmpty(a.Os) != Convert.ToString(x.ret.os))
                    Console.WriteLine("err Os: " + a.Os);

                if (SetNullEmpty(a.OsCode) != Convert.ToString(x.ret.os_code))
                    Console.WriteLine("err OsCode: " + a.OsCode);

                if (SetNullEmpty(a.OsFamily) != Convert.ToString(x.ret.os_family))
                    Console.WriteLine("err OsFamily: " + a.OsFamily);

                if (SetNullEmpty(a.OsFamilyCode) != Convert.ToString(x.ret.os_family_code))
                    Console.WriteLine("err OsFamilyCode: " + a.OsFamilyCode);

                if (SetNullEmpty(a.OsFamilyVendor) != Convert.ToString(x.ret.os_family_vendor))
                    Console.WriteLine("err OsFamilyVendor: " + a.OsFamilyVendor);

                if (SetNullEmpty(a.OsFamilyVendorCode) != Convert.ToString(x.ret.os_family_vendor_code))
                    Console.WriteLine("err OsFamilyVendorCode: " + a.OsFamilyVendorCode);

                if (SetNullEmpty(a.OsFamilyVendorHomepage) != Convert.ToString(x.ret.os_family_vendor_homepage))
                    Console.WriteLine("err OsFamilyVendorHomepage: " + a.OsFamilyVendorHomepage);

                if (SetNullEmpty(a.OsHomepage) != Convert.ToString(x.ret.os_homepage))
                    Console.WriteLine("err OsHomepage: " + a.OsHomepage);

                if (SetNullEmpty(a.OsIcon) != Convert.ToString(x.ret.os_icon))
                    Console.WriteLine("err OsIcon: " + a.OsIcon);

                if (SetNullEmpty(a.OsIconBig) != Convert.ToString(x.ret.os_icon_big))
                    Console.WriteLine("err OsIconBig: " + a.OsIconBig);

                if (HttpUtility.UrlDecode(SetNullEmpty(a.OsInfoUrl)) != Convert.ToString(x.ret.os_info_url))
                    Console.WriteLine("err OsInfoUrl: " + a.OsInfoUrl);

                if (SetNullEmpty(a.Ua) != Convert.ToString(x.ret.ua))
                    Console.WriteLine("err Ua: " + a.Ua + " expected: " + Convert.ToString(x.ret.ua));

                AssertString("UaClass", a.UaClass, x.ret.ua_class);
                AssertString("UaClassCode", a.UaClassCode, x.ret.ua_class_code);

                if (SetNullEmpty(a.UaEngine) != Convert.ToString(x.ret.ua_engine))
                    Console.WriteLine("err UaEngine: " + a.UaEngine);

                if (SetNullEmpty(a.UaFamily) != Convert.ToString(x.ret.ua_family))
                    Console.WriteLine("err UaFamily: " + a.UaFamily);

                if (SetNullEmpty(a.UaFamilyCode) != Convert.ToString(x.ret.ua_family_code))
                    Console.WriteLine("err UaFamilyCode: " + a.UaFamilyCode);

                if (SetNullEmpty(a.UaFamilyHomepage) != Convert.ToString(x.ret.ua_family_homepage))
                    Console.WriteLine("err UaFamilyHomepage: " + a.UaFamilyHomepage);

                if (SetNullEmpty(a.UaFamilyIcon) != Convert.ToString(x.ret.ua_family_icon))
                    Console.WriteLine("err UaFamilyIcon: " + a.UaFamilyIcon);

                if (SetNullEmpty(a.UaFamilyIconBig) != Convert.ToString(x.ret.ua_family_icon_big))
                    Console.WriteLine("err UaFamilyIconBig: " + a.UaFamilyIconBig);

                if (HttpUtility.UrlDecode(SetNullEmpty(a.UaFamilyInfoUrl)) != Convert.ToString(x.ret.ua_family_info_url))
                    Console.WriteLine("err UaFamilyInfoUrl: " + a.UaFamilyInfoUrl);

                if (SetNullEmpty(a.UaFamilyVendor) != Convert.ToString(x.ret.ua_family_vendor))
                    Console.WriteLine("err UaFamilyVendor: " + a.UaFamilyVendor);

                if (SetNullEmpty(a.UaFamilyVendorCode) != Convert.ToString(x.ret.ua_family_vendor_code))
                    Console.WriteLine("err UaFamilyVendorCode: " + a.UaFamilyVendorCode);

                if (SetNullEmpty(a.UaFamilyVendorHomepage) != Convert.ToString(x.ret.ua_family_vendor_homepage))
                    Console.WriteLine("err UaFamilyVendorHomepage: " + a.UaFamilyVendorHomepage);

                if (SetNullEmpty(a.UaString) != Convert.ToString(x.test.teststring))
                    Console.WriteLine("err UaString: " + a.UaString + " Expected: " + Convert.ToString(x.test.teststring));
                /*
                if (setNullEmpty(a.UaUptodateCurrentVersion) != Convert.ToString(x.ret.crawler_category))
                    Console.WriteLine("err UaUptodateCurrentVersion: " + a.UaUptodateCurrentVersion);
                */
                if (SetNullEmpty(a.UaVersion) != Convert.ToString(x.ret.ua_version))
                    Console.WriteLine("err UaVersion: " + a.UaVersion);

                if (SetNullEmpty(a.UaVersionMajor) != Convert.ToString(x.ret.ua_version_major))
                    Console.WriteLine("err UaVersionMajor: " + a.UaVersionMajor);

            }
            #endregion
            Console.ReadLine();
        }

        #region utils

        private static String SetNullEmpty(String x)
        {
            return x ?? "";
        }
        #endregion

    }
}

