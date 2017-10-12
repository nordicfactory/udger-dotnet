namespace Udger.Parser.V3
{
    public class IpResult
    {
        public IpResult(string ip )
        {
            Ip = ip;
        }

        public string Ip { get; set; }
        public int IpVer { get; set; }
        public string IpClassification { get; set; }
        public string IpClassificationCode { get; set; }
        public string IpLastSeen { get; set; }
        public string IpHostname { get; set; }
        public string IpCountry { get; set; }
        public string IpCountryCode { get; set; }
        public string IpCity { get; set; }
        public string CrawlerName { get; set; }
        public string CrawlerVer { get; set; }
        public string CrawlerVerMajor { get; set; }
        public string CrawlerFamily { get; set; }
        public string CrawlerFamilyCode { get; set; }
        public string CrawlerFamilyHomepage { get; set; }
        public string CrawlerFamilyVendor { get; set; }
        public string CrawlerFamilyVendorCode { get; set; }
        public string CrawlerFamilyVendorHomepage { get; set; }
        public string CrawlerFamilyIcon { get; set; }
        public string CrawlerFamilyInfoUrl { get; set; }
        public string CrawlerLastSeen { get; set; }
        public string CrawlerCategory { get; set; }
        public string CrawlerCategoryCode { get; set; }
        public string CrawlerRespectRobotstxt { get; set; }
        public string DataCenterName { get; set; }
        public string DataCenterNameCode { get; set; }
        public string DataCenterHomePage { get; set; }


        public override string ToString()
        {
            return
                $@"UdgerIpResult [
    ip={Ip}, 
    ipVer={IpVer}, 
    ipClassification={IpClassification}, 
    ipClassificationCode={IpClassificationCode}, 
    ipLastSeen={IpLastSeen}, 
    ipHostname={IpHostname}, 
    ipCountry={IpCountry}, 
    ipCountryCode={IpCountryCode}, 
    ipCity={IpCity}, 
    crawlerName={CrawlerName}, 
    crawlerVer={CrawlerVer}, 
    crawlerVerMajor={CrawlerVerMajor},
    crawlerFamily={CrawlerFamily}, 
    crawlerFamilyCode={CrawlerFamilyCode}, 
    crawlerFamilyHomepage={CrawlerFamilyHomepage}, 
    crawlerFamilyVendor={CrawlerFamilyVendor}, 
    crawlerFamilyVendorCode={CrawlerFamilyVendorCode}, 
    crawlerFamilyVendorHomepage={CrawlerFamilyVendorHomepage}, 
    crawlerFamilyIcon={CrawlerFamilyIcon}, 
    crawlerFamilyInfoUrl={CrawlerFamilyInfoUrl},
    crawlerLastSeen={CrawlerLastSeen}, 
    crawlerCategory={CrawlerCategory}, 
    crawlerCategoryCode={CrawlerCategoryCode}, 
    crawlerRespectRobotstxt={CrawlerRespectRobotstxt}, 
    dataCenterName={DataCenterName}, 
    dataCenterNameCode={DataCenterNameCode}, 
    dataCenterHomePage={DataCenterHomePage}]";
        }
    }
}