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
            GenerateInitialIntegerSolution();
            SetInputData();
            InitializeEnv();
            GenerateMasterProblem();
            GenerateSubProblem();

            while (UB - LB > _epsilon)
            {
                //do stuff
                _subProblem.Optimize();
                var duals = GetNonzeroDuals(_subProblem.GetConstrs());
            }

            return new Solution();
        }

        private void GenerateInitialIntegerSolution()
        {
            var initialSolution = new InitialSolution(_dataset).Get();
            _binarySolution = Helpers.ExtractVehicleTraverseFromSolution
                                     (initialSolution, _dataset.Vehicles.Count, _dataset.Vertices.Count);
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

        private double[] GetAllDuals(GRBConstr[] constrs)
        {
            var duals = new double[constrs.Length];
            for (var c = 0; c < constrs.Length; c++)
            {
                if (constrs[c].Pi != 0)
                    duals[c] = constrs[c].Pi;
            }
            return duals;
        }

        private List<double> GetNonzeroDuals(GRBConstr[] constrs)
        {
            var duals = new List<double>();
            for (var c = 0; c < constrs.Length; c++)
            {
                if (constrs[c].Pi != 0.0)
                    duals.Add(constrs[c].Pi);
            }
            return duals;
        }
    }
}
