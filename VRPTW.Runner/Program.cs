using System;
using VRPTW.Algorithm;
using VRPTW.Configuration;
using VRPTW.Data;
using VRPTW.Heuristics;
using VRPTW.Model;

namespace VRPTW.Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            var dataSource = Config.GetDataSource();
            var dataset = new DataPreparer(dataSource).GetCustomerAndVehicleData();

            //new GSolver(dataset).Run();
            Solution initialSolution = new InitialSolution(dataset).Get();
        }  
    }
}