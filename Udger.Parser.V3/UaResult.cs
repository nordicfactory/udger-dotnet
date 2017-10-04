using System;

namespace Udger.Parser.V3
{
    public class UaResult : IEquatable<UaResult>
    {
        /// <summary>
        /// Gets user agent string.
        /// </summary>
        public string UaString { get; }

        /// <summary>
        /// Gets client id.
        /// </summary>
        public int ClientId { get; internal set; }

        /// <summary>
        /// Gets class id.
        /// </summary>
        public int ClassId { get; internal set; }

        /// <summary>
        /// Gets or sets user agent class.
        /// </summary>
        public string UaClass { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets user agent class code.
        /// </summary>
        public string UaClassCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets user agent.
        /// </summary>
        public string Ua { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets user agent engine.
        /// </summary>
        public string UaEngine { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets user agent version.
        /// </summary>
        public string UaVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets major user agent version.
        /// </summary>
        public string UaVersionMajor { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last time when crawler has been seen.
        /// </summary>
        public string CrawlerLastSeen { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets flag if crawler respects the robots.txt.
        /// </summary>
        public string CrawlerRespectRobotstxt { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets crawler category.
        /// </summary>
        public string CrawlerCategory { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets crawler category code.
        /// </summary>
        public string CrawlerCategoryCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets flag if user agent is up to date.
        /// </summary>
        public string UaUptodateCurrentVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets user agent family.
        /// </summary>
        public string UaFamily { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets user agent family code.
        /// </summary>
        public string UaFamilyCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets user agent home page URL.
        /// </summary>
        public string UaFamilyHomepage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets user agent family icon.
        /// </summary>
        public string UaFamilyIcon { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets user agent family big icon.
        /// </summary>
        public string UaFamilyIconBig { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets user agent family vendor.
        /// </summary>
        public string UaFamilyVendor { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets user agent family vendor code.
        /// </summary>
        public string UaFamilyVendorCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets user agent family vendor home page.
        /// </summary>
        public string UaFamilyVendorHomepage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets user agent family info URL.
        /// </summary>
        public string UaFamilyInfoUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets operating system family.
        /// </summary>
        public string OsFamily { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets operating system family code.
        /// </summary>
        public string OsFamilyCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets operating system.
        /// </summary>
        public string Os { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets operating system code.
        /// </summary>
        public string OsCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets operating system homepage.
        /// </summary>
        public string OsHomepage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets operating system icon.
        /// </summary>
        public string OsIcon { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets operating system big icon.
        /// </summary>
        public string OsIconBig { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets operating system family vendor.
        /// </summary>
        public string OsFamilyVendor { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets operating system family vendor code.
        /// </summary>
        public string OsFamilyVendorCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets operating system family vendor home page.
        /// </summary>
        public string OsFamilyVendorHomepage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets operating system information URL.
        /// </summary>
        public string OsInfoUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets device class.
        /// </summary>
        public string DeviceClass { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets device class code.
        /// </summary>
        public string DeviceClassCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets device class icon.
        /// </summary>
        public string DeviceClassIcon { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets device big icon.
        /// </summary>
        public string DeviceClassIconBig { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets device class information URL.
        /// </summary>
        public string DeviceClassInfoUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets device market name.
        /// </summary>
        public string DeviceMarketname { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets device brand.
        /// </summary>
        public string DeviceBrand { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets device brand code.
        /// </summary>
        public string DeviceBrandCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets device brand homepage.
        /// </summary>
        public string DeviceBrandHomepage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets device brand icon.
        /// </summary>
        public string DeviceBrandIcon { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets device brand big icon.
        /// </summary>
        public string DeviceBrandIconBig { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets device brand information URL.
        /// </summary>
        public string DeviceBrandInfoUrl { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="UaResult"/> class.
        /// </summary>
        /// <param name="UaString">String representing user agent (browser or robot).</param>
        public UaResult(string UaString)
        {
            this.UaString = UaString ?? throw new ArgumentNullException(nameof(UaString));
        }

        /// <summary>
        /// Compares two instances of <see cref="UaResult"/>.
        /// </summary>
        /// <param name="other">Instance to compare with current one.</param>
        /// <returns>Returns true if instances are eqUal, otherwise returns false.</returns>
        public bool Equals(UaResult other)
        {
            return other != null &&
                   ClassId == other.ClassId &&
                   ClientId == other.ClientId &&
                   CrawlerCategory == other.CrawlerCategory &&
                   CrawlerCategoryCode == other.CrawlerCategoryCode &&
                   CrawlerLastSeen == other.CrawlerLastSeen &&
                   CrawlerRespectRobotstxt == other.CrawlerRespectRobotstxt &&
                   DeviceBrand == other.DeviceBrand &&
                   DeviceBrandCode == other.DeviceBrandCode &&
                   DeviceBrandHomepage == other.DeviceBrandHomepage &&
                   DeviceBrandIcon == other.DeviceBrandIcon &&
                   DeviceBrandIconBig == other.DeviceBrandIconBig &&
                   DeviceBrandInfoUrl == other.DeviceBrandInfoUrl &&
                   DeviceClass == other.DeviceClass &&
                   DeviceClassCode == other.DeviceClassCode &&
                   DeviceClassIcon == other.DeviceClassIcon &&
                   DeviceClassIconBig == other.DeviceClassIconBig &&
                   DeviceClassInfoUrl == other.DeviceClassInfoUrl &&
                   DeviceMarketname == other.DeviceMarketname &&
                   Os == other.Os &&
                   OsCode == other.OsCode &&
                   OsFamily == other.OsFamily &&
                   OsFamilyCode == other.OsFamilyCode &&
                   OsFamilyVendorHomepage == other.OsFamilyVendorHomepage &&
                   OsFamilyVendor == other.OsFamilyVendor &&
                   OsFamilyVendorCode == other.OsFamilyVendorCode &&
                   OsHomepage == other.OsHomepage &&
                   OsIcon == other.OsIcon &&
                   OsIconBig == other.OsIconBig &&
                   OsInfoUrl == other.OsInfoUrl &&
                   Ua == other.Ua &&
                   UaClass == other.UaClass &&
                   UaClassCode == other.UaClassCode &&
                   UaEngine == other.UaEngine &&
                   UaFamily == other.UaFamily &&
                   UaFamilyCode == other.UaFamilyCode &&
                   UaFamilyHomepage == other.UaFamilyHomepage &&
                   UaFamilyIcon == other.UaFamilyIcon &&
                   UaFamilyIconBig == other.UaFamilyIconBig &&
                   UaFamilyInfoUrl == other.UaFamilyInfoUrl &&
                   UaFamilyVendor == other.UaFamilyVendor &&
                   UaFamilyVendorCode == other.UaFamilyVendorCode &&
                   UaFamilyVendorHomepage == other.UaFamilyVendorHomepage &&
                   UaString == other.UaString &&
                   UaUptodateCurrentVersion == other.UaUptodateCurrentVersion &&
                   UaVersion == other.UaVersion &&
                   UaVersionMajor == other.UaVersionMajor;
        }

        /// <summary>
        /// Compares two instances of <see cref="UaResult"/>.
        /// </summary>
        /// <param name="other">Instance to compare with current one.</param>
        /// <returns>Returns true if instances are eqUal, otherwise returns false.</returns>
        public override bool Equals(object other)
        {
            return Equals(other as UaResult);
        }

        /// <summary>
        /// Gets the hash code of the instance.
        /// </summary>
        /// <returns>Returns hash code of the instance.</returns>
        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = (result * prime) + ClassId.GetHashCode();
            result = (result * prime) + ClientId.GetHashCode();
            result = (result * prime) + CrawlerCategory.GetHashCode();
            result = (result * prime) + CrawlerCategoryCode.GetHashCode();
            result = (result * prime) + CrawlerLastSeen.GetHashCode();
            result = (result * prime) + CrawlerRespectRobotstxt.GetHashCode();
            result = (result * prime) + DeviceBrand.GetHashCode();
            result = (result * prime) + DeviceBrandCode.GetHashCode();
            result = (result * prime) + DeviceBrandHomepage.GetHashCode();
            result = (result * prime) + DeviceBrandIcon.GetHashCode();
            result = (result * prime) + DeviceBrandIconBig.GetHashCode();
            result = (result * prime) + DeviceBrandInfoUrl.GetHashCode();
            result = (result * prime) + DeviceClass.GetHashCode();
            result = (result * prime) + DeviceClassCode.GetHashCode();
            result = (result * prime) + DeviceClassIcon.GetHashCode();
            result = (result * prime) + DeviceClassIconBig.GetHashCode();
            result = (result * prime) + DeviceClassInfoUrl.GetHashCode();
            result = (result * prime) + DeviceMarketname.GetHashCode();
            result = (result * prime) + Os.GetHashCode();
            result = (result * prime) + OsCode.GetHashCode();
            result = (result * prime) + OsFamily.GetHashCode();
            result = (result * prime) + OsFamilyCode.GetHashCode();
            result = (result * prime) + OsFamilyVendorHomepage.GetHashCode();
            result = (result * prime) + OsFamilyVendor.GetHashCode();
            result = (result * prime) + OsFamilyVendorCode.GetHashCode();
            result = (result * prime) + OsHomepage.GetHashCode();
            result = (result * prime) + OsIcon.GetHashCode();
            result = (result * prime) + OsIconBig.GetHashCode();
            result = (result * prime) + OsInfoUrl.GetHashCode();
            result = (result * prime) + Ua.GetHashCode();
            result = (result * prime) + UaClass.GetHashCode();
            result = (result * prime) + UaClassCode.GetHashCode();
            result = (result * prime) + UaEngine.GetHashCode();
            result = (result * prime) + UaFamily.GetHashCode();
            result = (result * prime) + UaFamilyCode.GetHashCode();
            result = (result * prime) + UaFamilyHomepage.GetHashCode();
            result = (result * prime) + UaFamilyIcon.GetHashCode();
            result = (result * prime) + UaFamilyIconBig.GetHashCode();
            result = (result * prime) + UaFamilyInfoUrl.GetHashCode();
            result = (result * prime) + UaFamilyVendor.GetHashCode();
            result = (result * prime) + UaFamilyVendorCode.GetHashCode();
            result = (result * prime) + UaFamilyVendorHomepage.GetHashCode();
            result = (result * prime) + UaString.GetHashCode();
            result = (result * prime) + UaUptodateCurrentVersion.GetHashCode();
            result = (result * prime) + UaVersion.GetHashCode();
            result = (result * prime) + UaVersionMajor.GetHashCode();

            return result;
        }

        /// <summary>
        /// Gets the string presentation of the instance.
        /// </summary>
        /// <returns>Returns the string presentation of the instance.</returns>
        public override string ToString()
        {
            return $@"
                UdgerUaResult[
                UaString={UaString}
                , clientId={ClientId}
                , classId={ClassId}
                , UaClass={UaClass}
                , UaClassCode={UaClassCode}
                , Ua={Ua}
                , UaEngine={UaEngine}
                , UaVersion={UaVersion}
                , UaVersionMajor={UaVersionMajor}
                , crawlerLastSeen={CrawlerLastSeen}
                , crawlerRespectRobotstxt={CrawlerRespectRobotstxt}
                , crawlerCategory={CrawlerCategory}
                , crawlerCategoryCode={CrawlerCategoryCode}
                , UaUptodateCurrentVersion={UaUptodateCurrentVersion}
                , UaFamily={UaFamily}
                , UaFamilyCode={UaFamilyCode}
                , UaFamilyHomepage={UaFamilyHomepage}
                , UaFamilyIcon={UaFamilyIcon}
                , UaFamilyIconBig={UaFamilyIconBig}
                , UaFamilyVendor={UaFamilyVendor}
                , UaFamilyVendorCode={UaFamilyVendorCode}
                , UaFamilyVendorHomepage={UaFamilyVendorHomepage}
                , UaFamilyInfoUrl={UaFamilyInfoUrl}
                , OsFamily={OsFamily}
                , OsFamilyCode={OsFamilyCode}
                , Os={Os}
                , OsCode={OsCode}
                , OsHomePage={OsHomepage}
                , OsIcon={OsIcon}
                , OsIconBig={OsIconBig}
                , OsFamilyVendor={OsFamilyVendor}
                , OsFamilyVendorCode={OsFamilyVendorCode}
                , OsFamilyVedorHomepage={OsFamilyVendorHomepage}
                , OsInfoUrl={OsInfoUrl}
                , deviceClass={DeviceClass}
                , deviceClassCode={DeviceClassCode}
                , deviceClassIcon={DeviceClassIcon}
                , deviceClassIconBig={DeviceClassIconBig}
                , deviceClassInfoUrl={DeviceClassInfoUrl}
                , deviceMarketname={DeviceMarketname}
                , deviceBrand={DeviceBrand}
                , deviceBrandCode={DeviceBrandCode}
                , deviceBrandHomepage={DeviceBrandHomepage}
                , deviceBrandIcon={DeviceBrandIcon}
                , deviceBrandIconBig={DeviceBrandIconBig}
                , deviceBrandInfoUrl={DeviceBrandInfoUrl}
                ];
            ";
        }
    }
}