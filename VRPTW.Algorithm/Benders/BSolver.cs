using Gurobi;
using System;
using System.Collections.Generic;
using System.Text;
using VRPTW.Configuration;
using VRPTW.Helper;
using VRPTW.Heuristics;
using VRPTW.Model;

namespace VRPTW.Algorithm.Benders
{
    public class BSolver : IAlgorithm
    {
        private double UB = double.MaxValue;
        private double LB = double.MinValue;
        private readonly double _epsilon = 0.01;
        private GRBEnv _env;
        private GRBModel _masterProblem;
        private GRBModel _subProblem;
        private readonly Dataset _dataset;
        private List<Vehicle> _vehicles;
        private List<Customer> _vertices;
        private double[,,] _binarySolution;

        public BSolver(Dataset dataset)
        {
            _dataset = dataset;
        }

        public Solution Run()
        {
            SetInputData();
            GenerateInitialIntegerSolution();
            InitializeEnv();
            GenerateMasterProblem();
            GenerateSubProblem();

            while (UB - LB > _epsilon)
            {
                //do stuff
                _subProblem.Optimize();
                var constraints = _subProblem.GetConstrs();
                var duals = constraints[0].Pi;
            }

            return new Solution();
        }

        private void GenerateInitialIntegerSolution()
        {
            var initialSolution = new InitialSolution(Helpers.Clone(_dataset)).Get();
            _binarySolution = Helpers.ExtractVehicleTraverseFromSolution
                                        (initialSolution, _dataset.Vehicles, _dataset.Vertices);
        }

        private void SetInputData()
        {
            _vertices = _dataset.Vertices;
            _vertices.Add(Helpers.Clone(_vertices[0]));
            _vehicles = _dataset.Vehicles;
        }

        private void InitializeEnv()
        {
            _env = new GRBEnv("VRPTW");
        }

        private void GenerateMasterProblem()
        {
            _masterProblem = new MasterProblem(_env, _vehicles, _vertices).GetModel();
        }

        private void GenerateSubProblem()
        {
            _subProblem = new SubProblem(_env, _vehicles, _vertices, _binarySolution).GetModel();
        }

        private List<List<List<double>>> InitializeBinarySolution()
        {
            var binarySolution = new List<List<List<double>>>();
            for (int v = 0; v < _dataset.Vehicles.Count; v++)
            {
                var binarySolutionV = new List<List<double>>();
                for (int s = 0; s < _dataset.Vertices.Count + 1; s++)
                {
                    var binarySolutionC = new List<double>();
                    for (int e = 0; e < _dataset.Vertices.Count + 1; e++)
                    {
                        binarySolutionC.Add(0.0);
                    }
                    binarySolutionV.Add(binarySolutionC);
                }
                binarySolution.Add(binarySolutionV);
            }
            return binarySolution;
        }
    }
}
