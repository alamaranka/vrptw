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
            Solution solution;

            switch (Config.GetSolverType())
            {
                case "GSolver":
                    solution = new GSolver(dataset).Run();
                    break;
                case "Heuristics":
                    solution = new InitialSolution(dataset).Get();
                    break;
                default:
                    break;
            }
        }
    }
}