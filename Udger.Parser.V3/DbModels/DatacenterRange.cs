using Udger.Parser.V3.RangeTree;

namespace Udger.Parser.V3.DbModels
{
    internal class DatacenterRange : IRangeProvider<long>
    {
        public string HomePage { get; set; }
        public string Name { get; set; }
        public string NameCode { get; set; }
        public long IpFrom { get; set; }
        public long IpTo { get; set; }
        
        public Range<long> Range => new Range<long>(IpFrom, IpTo);
    }
}