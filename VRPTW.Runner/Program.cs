using System;
using VRPTW.Algorithm;
using VRPTW.Data;

namespace VRPTW.Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            //TODO: consider appsettings.json for config
            var source = "xml"; var timeLimit = 120.0; var mipGap = 0.05;
            new GSolver("xml", timeLimit, mipGap).Run();
            if (source.Equals("database")) { new DBConnManager().Close(); }
            Console.ReadLine();
        }
    }
}