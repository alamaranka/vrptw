using System;
using VRPTW.Algorithm;
using VRPTW.Configuration;

namespace VRPTW.Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new Config().GetSolverParameters();
            new GSolver(config).Run();
            Console.ReadLine();
        }  
    }
}