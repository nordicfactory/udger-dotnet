using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

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
        private readonly LRUCache<string, UaResult> _cache;

        private readonly string ID_CRAWLER = "crawler";
        private readonly Regex _ipv6NormalizeRegex = new Regex("((?:(?:^|:)0+\\b){2,}):?(?!\\S*\\b\\1:0+\\b)(\\S*)", RegexOptions.Compiled);
        private readonly ConcurrentDictionary<string, Regex> _regexCache = new ConcurrentDictionary<string, Regex>();
        //private readonly ConcurrentDictionary<string, DbCommand> _preparedStmtMap = new ConcurrentDictionary<string, DbCommand>();
        private readonly Regex PAT_UNPERLIZE = new Regex(@"^/?(.*)/([si]*)\s*$", RegexOptions.Compiled);

        readonly Comparer<DatacenterRange> _datacenterRangeIpFromComparer = Comparer<DatacenterRange>.Create((a, b) =>
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
        /// <param name="inMemory"></param>
        public UdgerParser(string dbFilePath = "udgerdb_v3.dat", int cacheCapacity = 10000, bool inMemory = false)
        {
            _dbFilePath = dbFilePath;
            if (cacheCapacity > 0)
            {
                _cache = new LRUCache<string, UaResult>(cacheCapacity);
            }
        }

        public UaResult ParseUa(string uaString)
        {
            if (_cache != null)
            {
                if (_cache.TryGetValue(uaString, out var v))
                    return v;
            }

            var ret = new UaResult(uaString);

            Prepare();

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
                Prepare();

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

        private void FetchDeviceBrand(String uaString, UaResult ret)
        {
            using (var preparedStatement = _connection.CreateCommand())
            {
                preparedStatement.CommandText = UdgerSqlQuery.SqlDeviceRegex;
                preparedStatement.Parameters.Add(ret.OsFamilyCode == null
                    ? new SqliteParameter("@0", DBNull.Value)
                    : new SqliteParameter("@0", ret.OsFamilyCode));
                preparedStatement.Parameters.Add(ret.OsCode == null
                    ? new SqliteParameter("@1", DBNull.Value)
                    : new SqliteParameter("@1", ret.OsCode));

                preparedStatement.Parameters[0].Value = (object)ret.OsFamilyCode ?? DBNull.Value;
                preparedStatement.Parameters[1].Value = (object)ret.OsCode ?? DBNull.Value;
                using (var devRegexRs = preparedStatement.ExecuteReader())
                {
                    while (devRegexRs.Read())
                    {
                        var devId = devRegexRs.GetString(0);
                        var regex = devRegexRs.GetString(1);
                        if (devId == null || regex == null)
                            continue;

                        var patRegex = GetRegexFromCache(regex);
                        var matcher = patRegex.Match(uaString);
                        if (!matcher.Success)
                            continue;

                        using (var devNameListRs = GetFirstRow(UdgerSqlQuery.SqlDeviceNameList, devId, matcher.Groups[1].Value))
                        {
                            if (devNameListRs == null || !devNameListRs.Read())
                                continue;

                            DataReader.FetchDeviceBrand(devNameListRs, ret);
                        }
                        break;
                    }
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
                using (var rs = GetFirstRow(UdgerSqlQuery.SqlDevice, rowidMatchTuple.Item1))
                {
                    if (rs != null && rs.Read())
                    {
                        DataReader.FetchDevice(rs, ret);
                    }
                }
            }
            else
            {
                if (clientInfo.ClassId != null && clientInfo.ClassId != -1)
                {
                    using (var rs = GetFirstRow(UdgerSqlQuery.SqlClientClass, clientInfo.ClassId.ToString()))
                    {
                        if (rs != null && rs.Read())
                        {
                            DataReader.FetchDevice(rs, ret);
                        }
                    }
                }
            }
        }

        private void OsDetector(string uaString, UaResult ret, ClientInfo clientInfo)
        {
            var rowidMatchTuple = FindIdFromList(uaString, _osWordDetector.FindWords(uaString), _osRegstringList);
            if (rowidMatchTuple.Item1 != -1)
            {
                using (var rs = GetFirstRow(UdgerSqlQuery.SqlOs, rowidMatchTuple.Item1))
                {
                    
                    if (rs.Read())
                    {
                        DataReader.FetchOS(rs, ret);
                    }
                }
            }
            else
            {
                if (clientInfo.ClientId == 0)
                    return;

                using (var rs = GetFirstRow(UdgerSqlQuery.SqlClientOs, clientInfo.ClientId.ToString()))
                {
                    if (rs.Read())
                    {
                        DataReader.FetchOS(rs, ret);
                    }
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

            using (var rs1 = GetFirstRow(UdgerSqlQuery.SqlCrawler, uaString))
            {
                if (rs1.Read())
                {
                    DataReader.FetchUA(rs1, ret);
                    clientInfo.ClassId = 99;
                    clientInfo.ClientId = -1;
                }
                else
                {
                    var rowidMatchTuple = FindIdFromList(uaString, _clientWordDetector.FindWords(uaString), _clientRegstringList);
                    if (rowidMatchTuple.Item1 != -1)
                    {
                        using (var rs2 = GetFirstRow(UdgerSqlQuery.SqlClient, rowidMatchTuple.Item1))
                        {
                            if (rs2.HasRows && rs2.Read())
                            {
                                DataReader.FetchUA(rs2, ret);
                                clientInfo.ClassId = ret.ClassId;
                                clientInfo.ClientId = ret.ClientId;
                                PatchVersions(ret, rowidMatchTuple.Item2);
                            }
                        }
                    }
                    else
                    {
                        ret.UaClass = "Unrecognized";
                        ret.UaClassCode = "unrecognized";
                    }

                }
            }
            return clientInfo;
        }

        private void Prepare()
        {
            Connect();
            if (_clientRegstringList == null)
            {
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

        private void Connect()
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

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
            _connection = null;
        }
    }
}