using System;
using VRPTW.Algorithm;
using VRPTW.Configuration;

namespace VRPTW.Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new Config();
            new GSolver(config.GetSolverParameters()).Run();
            Console.ReadLine();
        }  
    }
}