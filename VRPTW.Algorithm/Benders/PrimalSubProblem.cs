using Gurobi;
using System;
using System.Collections.Generic;
using System.Linq;
using VRPTW.Configuration;
using VRPTW.Helper;
using VRPTW.Model;

namespace VRPTW.Algorithm.Benders
{
    class PrimalSubProblem
    {
        private List<List<GRBVar>> _serviceStart;
        private GRBLinExpr _cost;  
        private List<Vehicle> _vehicles;
        private List<Customer> _vertices;
        private readonly double[,,] _integerSolution;
        public GRBModel _model { get; }
        public Dictionary<(int, int), double> _A { get; }
        public Dictionary<(int, int), double> _B { get; }
        public List<double> _b { get; }
        public List<double> _ByBar { get; }
        public List<double> _c { get; }
        public List<char> _sense { get; }

        public PrimalSubProblem(GRBEnv env, List<Vehicle> vehicles, List<Customer> vertices, double[,,] integerSolution)
        {
            _vehicles = vehicles;
            _vertices = vertices;
            _model = new GRBModel(env);
            _integerSolution = integerSolution;
            _A = new Dictionary<(int, int), double>();
            _B = new Dictionary<(int, int), double>();
            _b = new List<double>();
            _ByBar = new List<double>();
            _c = new List<double>();
            _sense = new List<char>();
            Generate();
        }

        private void Generate()
        {
            SetSolverParameters();
            InitializeDecisionVariables();
            CreateGeneralDecisionVariables();
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
            _serviceStart = new List<List<GRBVar>>();
        }

        private void CreateGeneralDecisionVariables()
        {
            for (int v = 0; v < _vehicles.Count; v++)
            {
                List<GRBVar> serviceStartV = new List<GRBVar>();
                for (int s = 0; s < _vertices.Count; s++)
                {
                    serviceStartV.Add(_model.AddVar(0, BigM(), 0, GRB.CONTINUOUS, "service_start" + v + s));
                }
                _serviceStart.Add(serviceStartV);
            }
        }

        private void CreateObjective()
        {
            _cost = 0.0;
            for (int v = 0; v < _vehicles.Count; v++)
            {
                for (int s = 0; s < _vertices.Count; s++)
                {
                    for (int e = 0; e < _vertices.Count; e++)
                    {
                        var distance = Helpers.CalculateDistance(_vertices[s], _vertices[e]);
                        _cost += distance * _integerSolution[v,s,e];
                    }
                }
            }
            _c.AddRange(Enumerable.Repeat(0.0, _vehicles.Count * _vertices.Count));
            _model.SetObjective(_cost, GRB.MINIMIZE);
        }

        private void CreateConstraints()
        {
            DepartureFromACustomerAndItsImmediateSuccessor();
            TimeWindowsMustBeSatisfied();
        }

        private void DepartureFromACustomerAndItsImmediateSuccessor()
        {
            for (int v = 0; v < _vehicles.Count; v++)
            {
                for (int s = 0; s < _vertices.Count; s++)
                {
                    for (int e = 0; e < _vertices.Count; e++)
                    {
                        _model.AddConstr(-_serviceStart[v][s]
                                        - Helpers.CalculateDistance(_vertices[s], _vertices[e])
                                        - _vertices[s].ServiceTime
                                        + BigM() * (1 - _integerSolution[v,s,e])
                                        + _serviceStart[v][e]
                                        , GRB.GREATER_EQUAL
                                        , 0.0
                                        , "_DepartureFromACustomerAndItsImmediateSuccessor");
                        _sense.Add(GRB.GREATER_EQUAL);
                        _A[(_b.Count, _vertices.Count * v + s)] = -1.0;
                        _A[(_b.Count, _vertices.Count * v + e)] = 1.0;
                        _B[(_b.Count, _vertices.Count * _vertices.Count * v + _vertices.Count * s + e)] = -BigM();
                        _b.Add(Helpers.CalculateDistance(_vertices[s], _vertices[e]) + _vertices[s].ServiceTime - BigM());
                        _ByBar.Add(-BigM() * _integerSolution[v, s, e]);
                    }
                }
            }
        }

        private void TimeWindowsMustBeSatisfied()
        {
            for (int v = 0; v < _vehicles.Count; v++)
            {
                for (int s = 0; s < _vertices.Count; s++)
                {
                    _model.AddConstr(-_serviceStart[v][s], GRB.GREATER_EQUAL,
                                     -_vertices[s].TimeEnd, "_TimeWindowsMustBeSatisfied_Upper");
                    _sense.Add(GRB.GREATER_EQUAL);
                    _A[(_b.Count, _vertices.Count * v + s)] = -1.0;
                    _b.Add(-_vertices[s].TimeEnd);
                    _ByBar.Add(0.0);

                    _model.AddConstr(_serviceStart[v][s], GRB.GREATER_EQUAL,
                                     _vertices[s].TimeStart, "_TimeWindowsMustBeSatisfied_Lower");
                    _sense.Add(GRB.GREATER_EQUAL);
                    _A[(_b.Count, _vertices.Count * v + s)] = 1.0;
                    _b.Add(_vertices[s].TimeStart);
                    _ByBar.Add(0.0);
                }
            }
        }

        private double BigM()
        {
            return _vertices[0].TimeEnd - _vertices[0].TimeStart;
        }
    }
}
