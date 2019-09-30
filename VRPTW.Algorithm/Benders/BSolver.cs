using Gurobi;
using System;
using System.Collections.Generic;
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
        private readonly Dataset _dataset;
        private List<Vehicle> _vehicles;
        private List<Customer> _vertices;

        public BSolver(Dataset dataset)
        {
            _dataset = dataset;
        }

        public Solution Run()
        {
            var integerSolution = GenerateInitialIntegerSolution();
            SetInputData();
            InitializeEnv();
            var masterProblem = GenerateMasterProblem();

            while (UB - LB > _epsilon)
            {
                var subProblem = GeneratePrimalSubProblem(integerSolution);
                subProblem._model.Optimize();
                var A = subProblem._A;
                var B = subProblem._B;
                var b = subProblem._b;
                var c = subProblem._c;
                var sense = subProblem._sense;
                var dualProblem = GenerateDualSubProblem(A, b, c, sense);
                dualProblem._model.Optimize();
                var status = dualProblem.GetStatus();
                var dualSolution = dualProblem.GetSolution();

                if (status == GRB.Status.UNBOUNDED)
                {
                    masterProblem.AddFeasibilityCut(B, b, dualSolution);
                } 
                else if (status == GRB.Status.OPTIMAL)
                {
                    masterProblem.AddOptimalityCut(B, b, dualSolution);
                }
                if (subProblem._model.Status == GRB.Status.OPTIMAL)
                {
                    UB = Math.Min(UB, subProblem._model.Get(GRB.DoubleAttr.ObjVal));
                }
                masterProblem._model.Optimize();
                integerSolution = masterProblem.GetSolution();
                LB = masterProblem._model.Get(GRB.DoubleAttr.ObjVal);
            }

            return new Solution();
        }

        private double[,,] GenerateInitialIntegerSolution()
        {
            var initialSolution = new InitialSolution(_dataset).Get();
            return Helpers.ExtractVehicleTraverseFromSolution
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

        private MasterProblem GenerateMasterProblem()
        {
            return new MasterProblem(_env, _vehicles, _vertices);
        }

        private PrimalSubProblem GeneratePrimalSubProblem(double[,,] integerSolution)
        {
            return new PrimalSubProblem(_env, _vehicles, _vertices, integerSolution);
        }

        private DualSubProblem GenerateDualSubProblem(Dictionary<(int, int), double> A, List<double> b, List<double> c, List<char> sense)
        {
            return new DualSubProblem(_env, A, b, c, sense);
        }

        private double[] GetDuals(GRBConstr[] constrs)
        {
            var duals = new double[constrs.Length];
            for (var c = 0; c < constrs.Length; c++)
            {
                duals[c] = constrs[c].Pi;
            }
            return duals;
        }

        private double[] GetRhs(GRBConstr[] constrs)
        {
            var rhs = new double[constrs.Length];
            for (var c = 0; c < constrs.Length; c++)
            {
                rhs[c] = constrs[c].RHS;
            }
            return rhs;
        }
    }
}
