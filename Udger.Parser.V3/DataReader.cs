using System;
using System.Data;
using System.Data.Common;

namespace Udger.Parser.V3
{


    internal class DatacenterRange
    {
        public string HomePage { get; set; }
        public string Name { get; set; }
        public string NameCode { get; set; }
        public long IpFrom { get; set; }
        public long IpTo { get; set; }
    }

    internal static class DataReader
    {
        private static string UDGER_UA_DEV_BRAND_LIST_URL = "https://udger.com/resources/ua-list/devices-brand-detail?brand=";

        internal static DatacenterRange FetchDatacenterRange(IDataRecord rs)
        {
            var ret = new DatacenterRange
            {
                HomePage = GetDbString(rs, "datacenter_homepage"),
                Name = GetDbString(rs, "datacenter_name"),
                NameCode = GetDbString(rs, "datacenter_name_code"),
                IpFrom = GetDbInt32(rs, "iplong_from"),
                IpTo = GetDbInt32(rs, "iplong_to"),
            };
            return ret;
        }

        internal static void FetchDataCenter(IDataRecord rs, IpResult ret)
        {
            ret.DataCenterHomePage = GetDbString(rs, "datacenter_homepage");
            ret.DataCenterName = GetDbString(rs, "datacenter_name");
            ret.DataCenterNameCode = GetDbString(rs, "datacenter_name_code");
        }


        internal static void FetchUdgerIp(IDataRecord rs, IpResult ret)
        {
            ret.CrawlerCategory = GetDbString(rs, "crawler_category");
            ret.CrawlerCategoryCode = GetDbString(rs, "crawler_category_code");
            ret.CrawlerFamily = GetDbString(rs, "crawler_family");
            ret.CrawlerFamilyCode = GetDbString(rs, "crawler_family_code");
            ret.CrawlerFamilyHomepage = GetDbString(rs, "crawler_family_homepage");
            ret.CrawlerFamilyIcon = GetDbString(rs, "crawler_family_icon");
            ret.CrawlerFamilyInfoUrl = GetDbString(rs, "crawler_family_info_url");
            ret.CrawlerFamilyVendor = GetDbString(rs, "crawler_family_vendor");
            ret.CrawlerFamilyVendorCode = GetDbString(rs, "crawler_family_vendor_code");
            ret.CrawlerFamilyVendorHomepage = GetDbString(rs, "crawler_family_vendor_homepage");
            ret.CrawlerLastSeen = GetDbString(rs, "crawler_last_seen");
            ret.CrawlerName = GetDbString(rs, "crawler_name");
            ret.CrawlerRespectRobotstxt = GetDbString(rs, "crawler_respect_robotstxt");
            ret.CrawlerVer = GetDbString(rs, "crawler_ver");
            ret.CrawlerVerMajor = GetDbString(rs, "crawler_ver_major");
            ret.IpCity = GetDbString(rs, "ip_city");
            ret.IpClassification = GetDbString(rs, "ip_classification");
            ret.IpClassificationCode = GetDbString(rs, "ip_classification_code");
            ret.IpCountry = GetDbString(rs, "ip_country");
            ret.IpCountryCode = GetDbString(rs, "ip_country_code");
            ret.IpHostname = GetDbString(rs, "ip_hostname");
            ret.IpLastSeen = GetDbString(rs, "ip_last_seen");
        }

        internal static void FetchDevice(DbDataReader rs, UaResult ret)
        {
            ret.DeviceClass = GetDbString(rs, "device_class");
            ret.DeviceClassCode = GetDbString(rs, "device_class_code");
            ret.DeviceClassIcon = GetDbString(rs, "device_class_icon");
            ret.DeviceClassIconBig = GetDbString(rs, "device_class_icon_big");
            ret.DeviceClassInfoUrl = GetDbString(rs, "device_class_info_url");
        }

        internal static void FetchOS(DbDataReader rs, UaResult ret)
        {
            ret.OsFamily = Nvl(rs, 0) ?? "";
            ret.OsFamilyCode = Nvl(rs, 1) ?? "";
            ret.Os = Nvl(rs, 2) ?? "";
            ret.OsCode = Nvl(rs, 3) ?? "";
            ret.OsHomepage = Nvl(rs, 4) ?? "";
            ret.OsIcon = Nvl(rs, 5) ?? "";
            ret.OsIconBig = Nvl(rs, 6) ?? "";
            ret.OsFamilyVendor = Nvl(rs, 7) ?? "";
            ret.OsFamilyVendorCode = Nvl(rs, 8) ?? "";
            ret.OsFamilyVendorHomepage = Nvl(rs, 9) ?? "";
            ret.OsInfoUrl = Nvl(rs, 10) ?? "";
        }

        internal static void FetchUA(IDataRecord rs, UaResult ret)
        {
            ret.ClientId = GetDbInt32(rs, "client_id");
            ret.ClassId = GetDbInt32(rs, "class_id");
            ret.UaClass = GetDbString(rs, "ua_class");
            ret.UaClassCode = GetDbString(rs, "ua_class_code");
            ret.Ua = GetDbString(rs, "ua");
            ret.UaEngine = GetDbString(rs, "ua_engine");
            ret.UaVersion = GetDbString(rs, "ua_version");
            ret.UaVersionMajor = GetDbString(rs, "ua_version_major");
            ret.CrawlerLastSeen = GetDbString(rs, "crawler_last_seen");
            ret.CrawlerRespectRobotstxt = GetDbString(rs, "crawler_respect_robotstxt");
            ret.CrawlerCategory = GetDbString(rs, "crawler_category");
            ret.CrawlerCategoryCode = GetDbString(rs, "crawler_category_code");
            ret.UaUptodateCurrentVersion = GetDbString(rs, "ua_uptodate_current_version"); ;
            ret.UaFamily = GetDbString(rs, "ua_family");
            ret.UaFamilyCode = GetDbString(rs, "ua_family_code");
            ret.UaFamilyHomepage = GetDbString(rs, "ua_family_homepage");
            ret.UaFamilyIcon = GetDbString(rs, "ua_family_icon");
            ret.UaFamilyIconBig = GetDbString(rs, "ua_family_icon_big");
            ret.UaFamilyVendor = GetDbString(rs, "ua_family_vendor");
            ret.UaFamilyVendorCode = GetDbString(rs, "ua_family_vendor_code");
            ret.UaFamilyVendorHomepage = GetDbString(rs, "ua_family_vendor_homepage");
            ret.UaFamilyInfoUrl = GetDbString(rs, "ua_family_info_url");
        }


        internal static void FetchDeviceBrand(IDataRecord dr, UaResult ret)
        {
            ret.DeviceMarketname = GetDbString(dr, "marketname");
            ret.DeviceBrand = GetDbString(dr, "brand");
            ret.DeviceBrandCode = GetDbString(dr, "brand_code");
            ret.DeviceBrandHomepage = GetDbString(dr, "brand_url");
            ret.DeviceBrandIcon = GetDbString(dr, "icon");
            ret.DeviceBrandIconBig = GetDbString(dr, "icon_big");
            ret.DeviceBrandInfoUrl = UDGER_UA_DEV_BRAND_LIST_URL + GetDbString(dr, "brand_code");
        }

        private static string GetDbString(IDataRecord rs, string name)
        {
            return rs.IsDBNull(rs.GetOrdinal(name)) ? "" : rs.GetString(rs.GetOrdinal(name));
        }


        private static string Nvl(IDataRecord rs, int i)
        {
            return rs.IsDBNull(i) ? "" : rs.GetString(i);
        }

        private static int GetDbInt32(IDataRecord rs, string name)
        {
            var i = rs.GetOrdinal(name);
            return rs.IsDBNull(i) ? 0 : rs.GetInt32(i);
        }

    }
}
