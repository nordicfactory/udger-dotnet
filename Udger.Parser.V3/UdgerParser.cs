using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Udger.Parser.V3.DbModels;

namespace Udger.Parser.V3
{
    public class UdgerParser : IDisposable
    {
        private class IdRegString
        {
            public int Id;
            public int WordId1;
            public int WordId2;
            public Regex Pattern;
        }

        private DbConnection _connection;
        private readonly string _dbFilePath;
        private ImmutableList<IdRegString> _clientRegstringList;
        private ImmutableList<IdRegString> _osRegstringList;
        private ImmutableList<IdRegString> _deviceRegstringList;
        private WordDetector _clientWordDetector;
        private WordDetector _osWordDetector;
        private ImmutableList<DatacenterRange> _datacenterRangeList;
        private ConcurrentDictionary<int, Os> _rowIdOsDictionary;
        private ConcurrentDictionary<int, Os> _clientIdOsDictionary;
        private ConcurrentDictionary<int, Device> _rowIdDeviceDictionary;
        private ConcurrentDictionary<int, Device> _classIdDeviceDictionary;
        private ConcurrentDictionary<string, Ua> _uaStringUaDictionary;
        private ConcurrentDictionary<int, Ua> _rowIdUaDictionary;
        private ImmutableList<DeviceRegex> _osDeviceRegexList;
        private ConcurrentDictionary<Tuple<string, string>, DeviceBrand> _regexDeviceBrandDictionary; // <regexid,code>

        private readonly LRUCache<string, UaResult> _cache;


        private readonly string ID_CRAWLER = "crawler";
        private readonly Regex _ipv6NormalizeRegex = new Regex("((?:(?:^|:)0+\\b){2,}):?(?!\\S*\\b\\1:0+\\b)(\\S*)", RegexOptions.Compiled);
        private readonly ConcurrentDictionary<string, Regex> _regexCache = new ConcurrentDictionary<string, Regex>();
        private readonly Regex PAT_UNPERLIZE = new Regex(@"^/?(.*)/([si]*)\s*$", RegexOptions.Compiled);

        private readonly Comparer<DatacenterRange> _datacenterRangeIpFromComparer = Comparer<DatacenterRange>.Create((a, b) =>
            a == null && b == null ? 0 :
                (a == null ? -1 :
                    (b == null ? 1 :
                        a.IpFrom.CompareTo(b.IpFrom))));

        public bool OsParserEnabled { get; set; } = true;
        public bool DeviceParserEnabled { get; set; } = true;
        public bool DeviceBrandParserEnabled { get; set; } = true;
        public bool IpParserEnabled { get; set; } = true;
        public bool DataCenterParserEnabled { get; set; } = true;

        /// <summary>
        /// Create a new UdgerParser. 
        /// 
        /// The parser is threadsafe when inMemory is true, otherwise it is not.
        /// </summary>
        /// <param name="dbFilePath"></param>
        /// <param name="cacheCapacity"></param>
        public UdgerParser(string dbFilePath = "udgerdb_v3.dat", int cacheCapacity = 10000)
        {
            _dbFilePath = dbFilePath;
            if (cacheCapacity > 0)
            {
                _cache = new LRUCache<string, UaResult>(cacheCapacity);
            }
            Prepare();
        }

        public UaResult ParseUa(string uaString)
        {
            if (_cache != null)
            {
                if (_cache.TryGetValue(uaString, out var v))
                    return v;
            }

            var ret = new UaResult(uaString);

            //Prepare();

            var clientInfo = ClientDetector(uaString, ret);

            if (OsParserEnabled)
            {
                OsDetector(uaString, ret, clientInfo);
            }

            if (DeviceParserEnabled)
            {
                DeviceDetector(uaString, ret, clientInfo);
            }

            if (DeviceBrandParserEnabled)
            {
                if (!string.IsNullOrEmpty(ret.OsFamilyCode))
                {
                    FetchDeviceBrand(uaString, ret);
                }
            }

            _cache?.Add(uaString, ret);

            return ret;
        }


        public IpResult ParseIp(string ip)
        {
            var ret = new IpResult(ip);

            if (!IPAddress.TryParse(ip, out var addr))
                return ret;

            long? ipv4Int = null;
            string normalizedIp = null;

            if (addr.AddressFamily == AddressFamily.InterNetwork)
            {
                ipv4Int = 0;
                foreach (var b in addr.GetAddressBytes())
                {
                    ipv4Int = ipv4Int << 8 | (b & 0xFF);
                }
                normalizedIp = addr.ToString();
            }
            else if (addr.AddressFamily == AddressFamily.InterNetworkV6)
            {
                normalizedIp = _ipv6NormalizeRegex.Replace(addr.ToString(), "::$2");
            }


            ret.IpClassification = "Unrecognized";
            ret.IpClassificationCode = "unrecognized";

            if (normalizedIp != null)
            {
                //Prepare();

                if (IpParserEnabled)
                {
                    using (var ipRs = GetFirstRow(UdgerSqlQuery.SqlIp, normalizedIp))
                    {
                        if (ipRs.HasRows && ipRs.Read())
                        {
                            DataReader.FetchUdgerIp(ipRs, ret);
                            if (ID_CRAWLER != ret.IpClassificationCode)
                            {
                                ret.CrawlerFamilyInfoUrl = "";
                            }
                        }
                    }
                }

                if (!DataCenterParserEnabled) return ret;

                if (ipv4Int != null)
                {
                    ret.IpVer = 4;
                    var index = _datacenterRangeList.BinarySearch(new DatacenterRange { IpFrom = ipv4Int.Value }, _datacenterRangeIpFromComparer);
                    if (index >= 0)
                    {
                        var dc = _datacenterRangeList[index];
                        ret.DataCenterName = dc.Name;
                        ret.DataCenterHomePage = dc.HomePage;
                        ret.DataCenterNameCode = dc.NameCode;
                    }
                    else
                    {
                        index = ~index - 1; // -1 since binary search will return the first element bigger than the search value
                        if (index >= 0 && index < _datacenterRangeList.Count)
                        {
                            var dc = _datacenterRangeList[index];
                            if (dc.IpTo > ipv4Int)
                            {
                                ret.DataCenterName = dc.Name;
                                ret.DataCenterHomePage = dc.HomePage;
                                ret.DataCenterNameCode = dc.NameCode;
                            }
                        }
                    }

                }
                else
                {
                    ret.IpVer = 6;
                    var ipArray = Ip6ToArray(addr);
                    using (var dataCenterRs = GetFirstRow(UdgerSqlQuery.SqlDatacenterRange6,
                        ipArray[0], ipArray[0],
                        ipArray[1], ipArray[1],
                        ipArray[2], ipArray[2],
                        ipArray[3], ipArray[3],
                        ipArray[4], ipArray[4],
                        ipArray[5], ipArray[5],
                        ipArray[6], ipArray[6],
                        ipArray[7], ipArray[7]
                    ))
                    {
                        if (dataCenterRs.Read())
                        {
                            DataReader.FetchDataCenter(dataCenterRs, ret);
                        }
                    }
                }
            }

            return ret;
        }

        private static int[] Ip6ToArray(IPAddress addr)
        {
            var ret = new int[8];
            var bytes = addr.GetAddressBytes();
            for (var i = 0; i < 8; i++)
            {
                ret[i] = ((bytes[i * 2] << 8) & 0xff00) | (bytes[i * 2 + 1] & 0xff);
            }
            return ret;
        }


        private Regex GetRegexFromCache(string regex)
        {
            if (_regexCache.TryGetValue(regex, out var patRegex))
                return patRegex;

            patRegex = MakeRegex(regex);
            _regexCache[regex] = patRegex;
            return patRegex;
        }
        
        private void FetchDeviceBrand(string uaString, UaResult ret)
        {
            var deviceRegexs = _osDeviceRegexList.Where(x => x.OsFamilyCode == ret.OsFamilyCode && (x.OsCode == "-all-" || x.OsCode == ret.OsCode));

            foreach (var deviceRegex in deviceRegexs)
            {
                var patRegex = GetRegexFromCache(deviceRegex.Regstring);
                var matcher = patRegex.Match(uaString);
                if (!matcher.Success)
                    continue;

                if (_regexDeviceBrandDictionary.TryGetValue(Tuple.Create(deviceRegex.Id, matcher.Groups[1].Value), out var deviceBrand))
                {
                    ret.DeviceMarketname = deviceBrand.Marketname;
                    ret.DeviceBrand = deviceBrand.Brand;
                    ret.DeviceBrandCode = deviceBrand.BrandCode;
                    ret.DeviceBrandHomepage = deviceBrand.BrandHomepage;
                    ret.DeviceBrandIcon = deviceBrand.BrandIcon;
                    ret.DeviceBrandIconBig = deviceBrand.BrandIconBig;
                    ret.DeviceBrandInfoUrl = deviceBrand.BrandInfoUrl;
                    return;
                }
            }
        }

        private Tuple<int, Match> FindIdFromListFullScan(string uaString, ImmutableList<IdRegString> list)
        {
            foreach (var irs in list)
            {
                var matcher = irs.Pattern.Match(uaString);
                if (matcher.Success)
                {
                    return Tuple.Create(irs.Id, matcher);
                }
            }
            return Tuple.Create<int, Match>(-1, null);
        }

        private void DeviceDetector(string uaString, UaResult ret, ClientInfo clientInfo)
        {
            var rowidMatchTuple = FindIdFromListFullScan(uaString, _deviceRegstringList);
            if (rowidMatchTuple.Item1 != -1)
            {
                if (_rowIdDeviceDictionary.TryGetValue(rowidMatchTuple.Item1, out var device))
                {
                    ret.DeviceClass = device.DeviceClass;
                    ret.DeviceClassCode = device.DeviceClassCode;
                    ret.DeviceClassIcon = device.DeviceClassIcon;
                    ret.DeviceClassIconBig = device.DeviceClassIconBig;
                    ret.DeviceClassInfoUrl = device.DeviceClassInfoUrl;
                }
            }
            else
            {
                if (clientInfo.ClassId != null && clientInfo.ClassId != -1)
                {
                    if (_classIdDeviceDictionary.TryGetValue(clientInfo.ClassId.Value, out var device))
                    {
                        ret.DeviceClass = device.DeviceClass;
                        ret.DeviceClassCode = device.DeviceClassCode;
                        ret.DeviceClassIcon = device.DeviceClassIcon;
                        ret.DeviceClassIconBig = device.DeviceClassIconBig;
                        ret.DeviceClassInfoUrl = device.DeviceClassInfoUrl;
                    }
                }
            }
        }

        private void OsDetector(string uaString, UaResult ret, ClientInfo clientInfo)
        {
            var rowidMatchTuple = FindIdFromList(uaString, _osWordDetector.FindWords(uaString), _osRegstringList);
            if (rowidMatchTuple.Item1 != -1)
            {
                if (_rowIdOsDictionary.TryGetValue(rowidMatchTuple.Item1, out var os))
                {
                    ret.OsFamily = os.OSFamily;
                    ret.OsFamilyCode = os.OSFamilyCode;
                    ret.Os = os.OS;
                    ret.OsCode = os.OSCode;
                    ret.OsHomepage = os.OSHomePage;
                    ret.OsIcon = os.OSIcon;
                    ret.OsIconBig = os.OSIconBig;
                    ret.OsFamilyVendor = os.OSFamilyVendor;
                    ret.OsFamilyVendorCode = os.OSFamilyVendorCode;
                    ret.OsFamilyVendorHomepage = os.OSFamilyVedorHomepage;
                    ret.OsInfoUrl = os.OSInfoUrl;
                }
            }
            else
            {
                if (clientInfo.ClientId == 0)
                    return;

                if (_clientIdOsDictionary.TryGetValue(clientInfo.ClientId, out var os))
                {
                    ret.OsFamily = os.OSFamily;
                    ret.OsFamilyCode = os.OSFamilyCode;
                    ret.Os = os.OS;
                    ret.OsCode = os.OSCode;
                    ret.OsHomepage = os.OSHomePage;
                    ret.OsIcon = os.OSIcon;
                    ret.OsIconBig = os.OSIconBig;
                    ret.OsFamilyVendor = os.OSFamilyVendor;
                    ret.OsFamilyVendorCode = os.OSFamilyVendorCode;
                    ret.OsFamilyVendorHomepage = os.OSFamilyVedorHomepage;
                    ret.OsInfoUrl = os.OSInfoUrl;
                }
            }
        }

        private DbDataReader GetFirstRow(string query, params object[] objs)
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = query;

                for (var i = 0; i < objs.Length; i++)
                {
                    var t = objs[i];
                    cmd.Parameters.Add(t == null ? new SqliteParameter("@" + i, DBNull.Value) : new SqliteParameter("@" + i, t));
                }
                for (var i = 0; i < objs.Length; i++)
                {
                    cmd.Parameters[i].Value = objs[i];
                }

                return cmd.ExecuteReader(CommandBehavior.SingleRow);
            }
        }
        
        private Tuple<int, Match> FindIdFromList(string uaString, ICollection<int> foundClientWords, IEnumerable<IdRegString> list)
        {
            foreach (var irs in list)
            {
                if ((irs.WordId1 == 0 || foundClientWords.Contains(irs.WordId1)) &&
                    (irs.WordId2 == 0 || foundClientWords.Contains(irs.WordId2)))
                {
                    var matcher = irs.Pattern.Match(uaString);
                    if (!matcher.Success) continue;

                    return Tuple.Create(irs.Id, matcher);
                }
            }
            return Tuple.Create<int, Match>(-1, null);
        }


        private void PatchVersions(UaResult ret, Match lastPatternMatcher)
        {
            if (lastPatternMatcher != null)
            {
                var version = "";
                if (lastPatternMatcher.Groups.Count >= 1)
                {
                    version = lastPatternMatcher.Groups[1].Value ?? "";
                }
                ret.UaVersion = version;
                ret.UaVersionMajor = version.Split('\\', '.')[0];
                ret.Ua = (ret.Ua ?? "") + " " + version;
            }
            else
            {
                ret.UaVersion = "";
                ret.UaVersionMajor = "";
            }
        }


        private ClientInfo ClientDetector(string uaString, UaResult ret)
        {
            var clientInfo = new ClientInfo();
            
            if (_uaStringUaDictionary.TryGetValue(uaString, out var ua))
            {
                ret.ClientId = ua.ClientId;
                ret.ClassId = ua.ClassId;
                ret.UaClass = ua.UaClass;
                ret.UaClassCode = ua.UaClassCode;
                ret.Ua = ua.UserAgent;
                ret.UaEngine = ua.UaEngine;
                ret.UaVersion = ua.UaVersion;
                ret.UaVersionMajor = ua.UaVersionMajor;
                ret.CrawlerLastSeen = ua.CrawlerLastSeen;
                ret.CrawlerRespectRobotstxt = ua.CrawlerRespectRobotstxt;
                ret.CrawlerCategory = ua.CrawlerCategory;
                ret.CrawlerCategoryCode = ua.CrawlerCategoryCode;
                ret.UaUptodateCurrentVersion = ua.UaUptodateCurrentVersion;
                ret.UaFamily = ua.UaFamily;
                ret.UaFamilyCode = ua.UaFamilyCode;
                ret.UaFamilyHomepage = ua.UaFamilyHomepage;
                ret.UaFamilyIcon = ua.UaFamilyIcon;
                ret.UaFamilyIconBig = ua.UaFamilyIconBig;
                ret.UaFamilyVendor = ua.UaFamilyVendor;
                ret.UaFamilyVendorCode = ua.UaFamilyVendorCode;
                ret.UaFamilyVendorHomepage = ua.UaFamilyVendorHomepage;
                ret.UaFamilyInfoUrl = ua.UaFamilyInfoUrl;

                clientInfo.ClassId = 99;
                clientInfo.ClientId = -1;
            }
            else
            {
                var rowidMatchTuple = FindIdFromList(uaString, _clientWordDetector.FindWords(uaString), _clientRegstringList);
                if (rowidMatchTuple.Item1 != -1)
                {
                    if (_rowIdUaDictionary.TryGetValue(rowidMatchTuple.Item1, out ua))
                    {
                        ret.ClientId = ua.ClientId;
                        ret.ClassId = ua.ClassId;
                        ret.UaClass = ua.UaClass;
                        ret.UaClassCode = ua.UaClassCode;
                        ret.Ua = ua.UserAgent;
                        ret.UaEngine = ua.UaEngine;
                        ret.UaVersion = ua.UaVersion;
                        ret.UaVersionMajor = ua.UaVersionMajor;
                        ret.CrawlerLastSeen = ua.CrawlerLastSeen;
                        ret.CrawlerRespectRobotstxt = ua.CrawlerRespectRobotstxt;
                        ret.CrawlerCategory = ua.CrawlerCategory;
                        ret.CrawlerCategoryCode = ua.CrawlerCategoryCode;
                        ret.UaUptodateCurrentVersion = ua.UaUptodateCurrentVersion;
                        ret.UaFamily = ua.UaFamily;
                        ret.UaFamilyCode = ua.UaFamilyCode;
                        ret.UaFamilyHomepage = ua.UaFamilyHomepage;
                        ret.UaFamilyIcon = ua.UaFamilyIcon;
                        ret.UaFamilyIconBig = ua.UaFamilyIconBig;
                        ret.UaFamilyVendor = ua.UaFamilyVendor;
                        ret.UaFamilyVendorCode = ua.UaFamilyVendorCode;
                        ret.UaFamilyVendorHomepage = ua.UaFamilyVendorHomepage;
                        ret.UaFamilyInfoUrl = ua.UaFamilyInfoUrl;

                        clientInfo.ClassId = ret.ClassId;
                        clientInfo.ClientId = ret.ClientId;
                        PatchVersions(ret, rowidMatchTuple.Item2);
                    }
                }
                else
                {
                    ret.UaClass = "Unrecognized";
                    ret.UaClassCode = "unrecognized";
                }
            }

            return clientInfo;
        }

        private void Prepare()
        {
            Connect();
            if (_clientRegstringList == null)
            {
                FillRowIdOsDictionary();
                FillClientIdOsDictionary();
                FillRowIdDeviceDictionary();
                FillClassIdDeviceDictionary();
                FillUaStringUaDictionary();
                FillRowIdUaDictionary();
                FillDeviceRegex();
                FillDeviceBrand();
                
                InitStaticStructures(_connection);
            }
        }

        private static void AddUsedWords(HashSet<int> usedWords, DbConnection connection, String regexTableName, String wordIdColumn)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT " + wordIdColumn + " FROM " + regexTableName;
                using (var rs = cmd.ExecuteReader())
                {
                    while (rs.Read())
                    {
                        usedWords.Add(rs.GetInt32(0));
                    }
                }
            }
        }



        private ImmutableList<IdRegString> PrepareRegexpStruct(DbConnection connection, string regexpTableName)
        {
            var ret = new List<IdRegString>();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT rowid, regstring, word_id, word2_id FROM " + regexpTableName + " ORDER BY sequence";
                using (var rs = cmd.ExecuteReader())
                {
                    while (rs.Read())
                    {
                        var irs = new IdRegString
                        {
                            Id = rs.GetInt32(0),

                            WordId1 = rs.GetInt32(2),
                            WordId2 = rs.GetInt32(3),
                            Pattern = MakeRegex(rs.GetString(1))
                        };
                        ret.Add(irs);
                    }
                }
            }
            return ret.ToImmutableList();
        }

        private Regex MakeRegex(string regex)
        {
            var filteredRegex = regex;
            var options = "";
            var ms = PAT_UNPERLIZE.Match(regex);
            if (ms.Groups.Count > 2)
            {
                filteredRegex = ms.Groups[1].Value;
                options = ms.Groups[2].Value;
            }
            // In c# we should not escape _ in Regexps.
            filteredRegex = filteredRegex.Replace(@"\_", "_");
            var o = RegexOptions.Compiled;
            foreach (var option in options)
            {
                switch (option)
                {
                    case 's':
                        o = o | RegexOptions.Singleline;
                        break;
                    case 'i':
                        o = o | RegexOptions.IgnoreCase;
                        break;
                    default:
                        Console.WriteLine("Usupported regex option: " + o);
                        break;
                }
            }

            return new Regex(filteredRegex, o);
        }

        private static WordDetector CreateWordDetector(DbConnection connection, string regexTableName, string wordTableName)
        {
            var usedWords = new HashSet<int>();

            AddUsedWords(usedWords, connection, regexTableName, "word_id");
            AddUsedWords(usedWords, connection, regexTableName, "word2_id");

            var wordInfos = new List<WordDetector.WordInfo>();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM " + wordTableName;
                using (var rs = cmd.ExecuteReader())
                {
                    while (rs.Read())
                    {
                        var id = rs.GetInt32(0);
                        if (!usedWords.Contains(id))
                            continue;
                        var word = rs.GetString(1).ToLowerInvariant();
                        wordInfos.Add(new WordDetector.WordInfo(id, word));
                    }
                }
            }
            return new WordDetector(wordInfos);
        }
        private void FillUaStringUaDictionary()
        {
            _uaStringUaDictionary = new ConcurrentDictionary<string, Ua>();
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = UdgerSqlQuery.SqlCrawlerAll;
                using (var rs = cmd.ExecuteReader())
                {
                    while (rs.Read())
                    {
                        _uaStringUaDictionary[DataReader.GetDbString(rs, "ua_string")] = DataReader.ReadUA(rs);
                    }
                }
            }
        }

        private void FillRowIdUaDictionary()
        {
            _rowIdUaDictionary = new ConcurrentDictionary<int, Ua>();
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = UdgerSqlQuery.SqlClientAll;
                using (var rs = cmd.ExecuteReader())
                {
                    while (rs.Read())
                    {
                        _rowIdUaDictionary[DataReader.GetDbInt32(rs, "rowid")] = DataReader.ReadUA(rs);
                    }
                }
            }
        }

        private void FillRowIdOsDictionary()
        {
            _rowIdOsDictionary = new ConcurrentDictionary<int, Os>();
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = UdgerSqlQuery.SqlOsAll;
                using (var rs = cmd.ExecuteReader())
                {
                    while (rs.Read())
                    {
                        _rowIdOsDictionary[DataReader.GetDbInt32(rs, "rowid")] = DataReader.ReadOS(rs);
                    }
                }
            }
        }

        private void FillRowIdDeviceDictionary()
        {
            _rowIdDeviceDictionary = new ConcurrentDictionary<int, Device>();
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = UdgerSqlQuery.SqlDeviceAll;
                using (var rs = cmd.ExecuteReader())
                {
                    while (rs.Read())
                    {
                        _rowIdDeviceDictionary[DataReader.GetDbInt32(rs, "rowid")] = DataReader.ReadDevice(rs);
                    }
                }
            }
        }

        private void FillClassIdDeviceDictionary()
        {
            _classIdDeviceDictionary = new ConcurrentDictionary<int, Device>();
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = UdgerSqlQuery.SqlClientClassAll;
                using (var rs = cmd.ExecuteReader())
                {
                    while (rs.Read())
                    {
                        _classIdDeviceDictionary[DataReader.GetDbInt32(rs, "id")] = DataReader.ReadDevice(rs);
                    }
                }
            }
        }

        private void FillClientIdOsDictionary()
        {
            _clientIdOsDictionary = new ConcurrentDictionary<int, Os>();
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = UdgerSqlQuery.SqlClientOsAll;
                using (var rs = cmd.ExecuteReader())
                {
                    while (rs.Read())
                    {
                        _clientIdOsDictionary[DataReader.GetDbInt32(rs, "client_id")] = DataReader.ReadOS(rs);
                    }
                }
            }
        }

        private void FillDeviceRegex()
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = UdgerSqlQuery.SqlDeviceRegexAll;
                using (var rs = cmd.ExecuteReader())
                {
                    var l = new List<DeviceRegex>();

                    while (rs.Read())
                    {
                        l.Add(DataReader.ReadDeviceRegex(rs));
                    }
                    _osDeviceRegexList = l.OrderBy(regex => regex.Sequence).ToImmutableList();
                }
            }
        }

        private void FillDeviceBrand()
        {
            _regexDeviceBrandDictionary = new ConcurrentDictionary<Tuple<string, string>, DeviceBrand>();
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = UdgerSqlQuery.SqlDeviceNameListAll;
                using (var rs = cmd.ExecuteReader())
                {
                    while (rs.Read())
                    {
                        var key = Tuple.Create(DataReader.GetDbString(rs, "regex_id"), DataReader.GetDbString(rs, "code"));
                        _regexDeviceBrandDictionary[key] = DataReader.ReadDeviceBrand(rs);
                    }
                }
            }
        }

        private void InitStaticStructures(DbConnection connection)
        {
            if (_clientRegstringList == null)
            {
                _clientRegstringList = PrepareRegexpStruct(connection, "udger_client_regex");
                _osRegstringList = PrepareRegexpStruct(connection, "udger_os_regex");
                _deviceRegstringList = PrepareRegexpStruct(connection, "udger_deviceclass_regex");

                _clientWordDetector = CreateWordDetector(connection, "udger_client_regex", "udger_client_regex_words");
                _osWordDetector = CreateWordDetector(connection, "udger_os_regex", "udger_os_regex_words");

                var l = new List<DatacenterRange>();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = UdgerSqlQuery.SqlDatacenterList;
                    using (var rs = cmd.ExecuteReader())
                    {
                        while (rs.Read())
                        {
                            var r = DataReader.FetchDatacenterRange(rs);
                            l.Add(r);
                        }
                    }
                }
                _datacenterRangeList = l.ToImmutableList();
            }
        }

        private readonly object _obj = new object();

        private void Connect()
        {
            if (_connection != null) return;

            lock (_obj)
            {
                if (_connection != null) return;

                _connection = new SqliteConnection(new SqliteConnectionStringBuilder
                {
                    DataSource = _dbFilePath,
                    Mode = SqliteOpenMode.ReadOnly,
                    Cache = SqliteCacheMode.Shared,
                }.ToString());


                _connection.Open();
            }
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
            _connection = null;
        }
    }
}