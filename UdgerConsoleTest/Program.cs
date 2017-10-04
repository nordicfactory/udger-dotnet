using System;
using Udger.Parser.V3;

namespace UdgerConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a new UdgerParser object
            var parser = new UdgerParser(@"C:\code\notebooks\data\udgerdb_v3.dat");
            
            Console.WriteLine(parser.ParseIp(@"23.20.0.0"));
            Console.WriteLine(parser.ParseIp(@"163.172.0.1"));
            Console.WriteLine(parser.ParseIp(@"127.0.0.1"));


            var uaResult = parser.ParseUa(@"Mozilla/5.0 (Windows NT 10.0; WOW64; rv:55.0) Gecko/20100101 Firefox/55.0");
            Console.WriteLine(uaResult);
            Console.ReadLine();

        }
    }
}
