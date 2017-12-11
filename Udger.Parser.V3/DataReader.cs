using System.Data;
using System.Data.Common;
using Udger.Parser.V3.DbModels;

namespace Udger.Parser.V3
{
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
                IpFrom = GetDbInt64(rs, "iplong_from"),
                IpTo = GetDbInt64(rs, "iplong_to"),
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

        internal static Device ReadDevice(DbDataReader rs)
        {
            return new Device
            {
                DeviceClass = GetDbString(rs, "device_class"),
                DeviceClassCode = GetDbString(rs, "device_class_code"),
                DeviceClassIcon = GetDbString(rs, "device_class_icon"),
                DeviceClassIconBig = GetDbString(rs, "device_class_icon_big"),
                DeviceClassInfoUrl = GetDbString(rs, "device_class_info_url"),
            };
        }

        internal static Os ReadOS(DbDataReader rs)
        {
            return new Os
            {
                OSFamily = GetDbString(rs, "os_family"),
                OSFamilyCode = GetDbString(rs, "os_family_code"),
                OS = GetDbString(rs, "os"),
                OSCode = GetDbString(rs, "os_code"),
                OSHomePage = GetDbString(rs, "os_home_page"),
                OSIcon = GetDbString(rs, "os_icon"),
                OSIconBig = GetDbString(rs, "os_icon_big"),
                OSFamilyVendor = GetDbString(rs, "os_family_vendor"),
                OSFamilyVendorCode = GetDbString(rs, "os_family_vendor_code"),
                OSFamilyVedorHomepage = GetDbString(rs, "os_family_vendor_homepage"),
                OSInfoUrl = GetDbString(rs, "os_info_url")
            };
        }

        internal static Ua ReadUA(IDataRecord rs)
        {
            return new Ua
            {
                ClientId = GetDbInt32(rs, "client_id"),
                ClassId = GetDbInt32(rs, "class_id"),
                UaClass = GetDbString(rs, "ua_class"),
                UaClassCode = GetDbString(rs, "ua_class_code"),
                UserAgent = GetDbString(rs, "ua"),
                UaEngine = GetDbString(rs, "ua_engine"),
                UaVersion = GetDbString(rs, "ua_version"),
                UaVersionMajor = GetDbString(rs, "ua_version_major"),
                CrawlerLastSeen = GetDbString(rs, "crawler_last_seen"),
                CrawlerRespectRobotstxt = GetDbString(rs, "crawler_respect_robotstxt"),
                CrawlerCategory = GetDbString(rs, "crawler_category"),
                CrawlerCategoryCode = GetDbString(rs, "crawler_category_code"),
                UaUptodateCurrentVersion = GetDbString(rs, "ua_uptodate_current_version"),
                UaFamily = GetDbString(rs, "ua_family"),
                UaFamilyCode = GetDbString(rs, "ua_family_code"),
                UaFamilyHomepage = GetDbString(rs, "ua_family_homepage"),
                UaFamilyIcon = GetDbString(rs, "ua_family_icon"),
                UaFamilyIconBig = GetDbString(rs, "ua_family_icon_big"),
                UaFamilyVendor = GetDbString(rs, "ua_family_vendor"),
                UaFamilyVendorCode = GetDbString(rs, "ua_family_vendor_code"),
                UaFamilyVendorHomepage = GetDbString(rs, "ua_family_vendor_homepage"),
                UaFamilyInfoUrl = GetDbString(rs, "ua_family_info_url"),
            };
        }

        internal static DeviceRegex ReadDeviceRegex(IDataRecord dr)
        {
            return new DeviceRegex
            {
                Id = GetDbString(dr, "id"),
                OsCode = GetDbString(dr, "os_code"),
                OsFamilyCode = GetDbString(dr, "os_family_code"),
                Regstring = GetDbString(dr, "regstring"),
                Sequence = GetDbInt32(dr, "sequence"),
            };
        }

        internal static DeviceBrand ReadDeviceBrand(IDataRecord dr)
        {
            return new DeviceBrand
            {
                Marketname = GetDbString(dr, "marketname"),
                Brand = GetDbString(dr, "brand"),
                BrandCode = GetDbString(dr, "brand_code"),
                BrandHomepage = GetDbString(dr, "brand_url"),
                BrandIcon = GetDbString(dr, "icon"),
                BrandIconBig = GetDbString(dr, "icon_big"),
                BrandInfoUrl = UDGER_UA_DEV_BRAND_LIST_URL + GetDbString(dr, "brand_code"),
            };
        }

        public static string GetDbString(IDataRecord rs, string name)
        {
            return rs.IsDBNull(rs.GetOrdinal(name)) ? "" : rs.GetString(rs.GetOrdinal(name));
        }

        public static int GetDbInt32(IDataRecord rs, string name)
        {
            var i = rs.GetOrdinal(name);
            return rs.IsDBNull(i) ? 0 : rs.GetInt32(i);
        }

        public static long GetDbInt64(IDataRecord rs, string name)
        {
            var i = rs.GetOrdinal(name);
            return rs.IsDBNull(i) ? 0 : rs.GetInt64(i);
        }

    }
}
