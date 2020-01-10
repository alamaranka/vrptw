using Newtonsoft.Json;
using System.IO;
using System.Linq;
using VRPTW.Algorithm;
using VRPTW.Algorithm.Benders;
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
            Solution solution = new Solution();

            switch (Config.GetSolverType())
            {
                case "GSolver":
                    solution = new GSolver(dataset).Run();
                    break;
                case "BSolver":
                    solution = new BSolver(dataset).Run();
                    break;
                case "HSolver":
                    solution = new HSolver(dataset).Run();
                    break;
                default:
                    break;
            }

            var outputPathString = Config.GetFileOperation().OutputPath +
                                   Config.GetFileOperation().OutputName;

            using StreamWriter file = File.CreateText(outputPathString);
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(file, solution);
        }
    }
}