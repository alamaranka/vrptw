using Gurobi;
using System;
using System.Collections.Generic;
using System.Text;
using VRPTW.Configuration;
using VRPTW.Model;

namespace VRPTW.Algorithm.Benders
{
    public class DualSubProblem
    {
        private List<GRBVar> _w;
        private GRBLinExpr _cost = new GRBLinExpr();
        private List<double> _b = new List<double>();
        private Dictionary<(int, int), double> _A = new Dictionary<(int, int), double>();
        private List<double> _c = new List<double>();
        private List<char> _sense = new List<char>();
        public GRBModel _model { get; }

        public DualSubProblem(GRBEnv env, Dictionary<(int, int), double> A, List<double> b, List<double> c, List<char> sense)
        {
            _model = new GRBModel(env);
            _b = b;
            _A = A;
            _c = c;
            _sense = sense;
            Generate();
        }

        public List<double> GetSolution()
        {
            var solution = new List<double>();
            for (int v = 0; v < _b.Count; v++)
            {
                solution.Add(_w[v].Get(GRB.DoubleAttr.X));
            }
            return solution;
        }

        public int GetStatus()
        {
            return _model.Status;
        }

        private void Generate()
        {
            SetSolverParameters();
            InitializeDecisionVariables();
            CreateDecisionVariables();
            CreateObjective();
            CreateConstraints();
        }

        private void SetSolverParameters()
        {
            _model.Parameters.TimeLimit = Config.GetSolverParam().TimeLimit;
            _model.Parameters.MIPGap = Config.GetSolverParam().MIPGap;
            _model.Parameters.Threads = Config.GetSolverParam().Threads;
            _model.Parameters.InfUnbdInfo = 1;
        }

        private void InitializeDecisionVariables()
        {
            _w = new List<GRBVar>();
        }

        private void CreateDecisionVariables()
        {
            for (int v = 0; v < _b.Count; v++)
            {
                switch (_sense[v])
                {
                    case GRB.EQUAL:
                        _w.Add(_model.AddVar(double.MinValue, double.MaxValue, 0, GRB.CONTINUOUS, "w_" + v));
                        break;
                    case GRB.GREATER_EQUAL:
                        _w.Add(_model.AddVar(0, double.MaxValue, 0, GRB.CONTINUOUS, "w_" + v));
                        break;
                    case GRB.LESS_EQUAL:
                        _w.Add(_model.AddVar(double.MinValue, 0, 0, GRB.CONTINUOUS, "w_" + v));
                        break;
                    default:
                        break;
                }
            }   
        }

        private void CreateObjective()
        {
            for (int v = 0; v < _b.Count; v++)
            {
                _cost.AddTerm(_b[v], _w[v]);
            }
            _model.SetObjective(_cost, GRB.MAXIMIZE);
        }

        private void CreateConstraints()
        {
            for (int v = 0; v < _c.Count; v++)
            {
                var line = new GRBLinExpr();
                for (int k = 0; k < _b.Count; k++)
                {
                    if (!_A.TryGetValue((k, v), out double value))
                    {
                        value = 0.0;
                    }

                    line.AddTerm(value, _w[k]);
                }
                _model.AddConstr(line, GRB.LESS_EQUAL, _c[v], "CreateConstraints_" + v);
            }
        }
    }
}
