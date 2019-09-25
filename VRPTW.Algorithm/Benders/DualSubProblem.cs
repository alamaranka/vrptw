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
        private List<Vehicle> _vehicles;
        private List<Customer> _vertices;
        private List<double> _b = new List<double>();
        private List<List<double>> _A = new List<List<double>>();
        private List<double> _c = new List<double>();
        private List<char> _sense = new List<char>();
        public GRBModel _model { get; }
        public List<double> _solution { get; }

        public DualSubProblem(GRBEnv env, List<Vehicle> vehicles, List<Customer> vertices,
                              List<List<double>> A, List<double> b, List<double> c, List<char> sense)
        {
            _vehicles = vehicles;
            _vertices = vertices;
            _model = new GRBModel(env);
            _b = b;
            _A = A;
            _c = c;
            _sense = sense;
            Generate();
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

    }
}
