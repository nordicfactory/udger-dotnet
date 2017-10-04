/*
 UdgerParser - test speed 
 
  author     The Udger.com Team (info@udger.com)
  license    GNU Lesser General Public License
  link       http://udger.com/products/local_parser
*/

using System.Net;
using System.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Udger.Parser.V3;

namespace UdgerSpeedTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // ip 2000/s
            // ua 200/s
            
            string line;

            Console.WriteLine("start");

            #region UdgerParse
            // Create a new UdgerParser object
            // Data file can be downloaded from http://data.udger.com/
            var parser = new UdgerParser(@"C:\code\notebooks\data\udgerdb_v3.dat");

            #endregion

            #region IP test
            Console.WriteLine("download test IP file start");
            var client = new WebClient();
            var stream = client.OpenRead("https://raw.githubusercontent.com/udger/test-data/master/test_ua-ip/ip_10000.txt");
            var reader = new StreamReader(stream);
            Console.WriteLine("download test IP file end");


            Console.WriteLine("parse IP start");
            var sw = Stopwatch.StartNew();
            int n = 0;
            while ((line = reader.ReadLine()) != null)
            {
                n += 1;
                if (n%100 == 0)
                    Console.Write(".");
                // Parse
                parser.ParseIp(line.Trim());
            }
            Console.WriteLine();

            Console.WriteLine("parse IP end, time (ms): " + sw.ElapsedMilliseconds );
            #endregion

            #region UA test
            Console.WriteLine("download test UA file start");
            client = new WebClient();
            stream = client.OpenRead("https://raw.githubusercontent.com/udger/test-data/master/test_ua-ip/ua_10000.txt");
            reader = new StreamReader(stream);
            var lines = new List<string>();
            
            while ((line = reader.ReadLine()) != null)
            {
                // Parse
                lines.Add(line);
            }
            Console.WriteLine("download test UA file end");


            Console.WriteLine("parse UA start");
            sw.Restart();
            n = 0;
            foreach (var l in lines)
            {
                n += 1;
                if (n % 100 == 0)
                    Console.Write(".");
                parser.ParseUa(l);
            }
            
            Console.WriteLine("parse UA end, time (ms): " + sw.ElapsedMilliseconds);

            Console.WriteLine("parse UA cached start");
            sw.Restart();

            foreach (var l in lines)
            {
                parser.ParseUa(l);
            }
            Console.WriteLine("parser UA cached end, time (ms): " + sw.ElapsedMilliseconds);
            #endregion

            Console.WriteLine("end");
            // Suspend the screen.
            Console.ReadLine();

        }
    }
}