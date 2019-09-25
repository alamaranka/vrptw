using Gurobi;
using System.Collections.Generic;
using System.Linq;
using VRPTW.Configuration;
using VRPTW.Helper;
using VRPTW.Model;

namespace VRPTW.Algorithm.Benders
{
    class SubProblem
    {
        private List<List<GRBVar>> _serviceStart;
        private GRBLinExpr _cost;  
        private List<Vehicle> _vehicles;
        private List<Customer> _vertices;
        private readonly double[,,] _integerSolution;
        public GRBModel _model { get; }
        public List<double> _b { get; }
        public List<List<double>> _B { get; }
        
        public SubProblem(GRBEnv env, List<Vehicle> vehicles, List<Customer> vertices, double[,,] integerSolution)
        {
            _vehicles = vehicles;
            _vertices = vertices;
            _model = new GRBModel(env);
            _integerSolution = integerSolution;
            _b = new List<double>();
            _B = new List<List<double>>();
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
            _model.SetObjective(_cost, GRB.MINIMIZE);
        }

        private void CreateConstraints()
        {
            UnAllowedTraverses();
            EachCustomerMustBeVisitedOnce();
            AllVehiclesMustStartFromTheDepot();
            AllVehiclesMustEndAtTheDepot();
            VehiclesMustLeaveTheArrivingCustomer();
            VehiclesLoadUpCapacity();
            DepartureFromACustomerAndItsImmediateSuccessor();
            TimeWindowsMustBeSatisfied();
        }

        private void UnAllowedTraverses()
        {
            var unAllowedTraverses = 0.0;
            var BRow = new List<double>();
            BRow.AddRange(Enumerable.Repeat(0.0, _vehicles.Count * _vertices.Count * _vertices.Count));
            for (int v = 0; v < _vehicles.Count; v++)
            {
                for (int s = 0; s < _vertices.Count; s++)
                {
                    unAllowedTraverses += _integerSolution[v, s, s] +
                                          _integerSolution[v, s, 0] +
                                          _integerSolution[v, _vertices.Count - 1, s];
                    BRow[_vertices.Count * _vertices.Count * v + _vertices.Count * s + s] = 1.0;
                    BRow[_vertices.Count * _vertices.Count * v + _vertices.Count * s] = 1.0;
                    BRow[_vertices.Count * _vertices.Count * v + _vertices.Count * (_vertices.Count - 1) + s] = 1.0;
                }
            }
            _model.AddConstr(unAllowedTraverses, GRB.EQUAL, 0.0, "_UnAllowedTraverses");
            _b.Add(0.0);
            _B.Add(BRow);
        }

        private void EachCustomerMustBeVisitedOnce()
        {
            for (int s = 1; s <= _vertices.Count - 2; s++)
            {
                var customerVisit = 0.0;
                var BRow = new List<double>();
                BRow.AddRange(Enumerable.Repeat(0.0, _vehicles.Count * _vertices.Count * _vertices.Count));
                for (int v = 0; v < _vehicles.Count; v++)
                {
                    for (int e = 0; e < _vertices.Count; e++)
                    {
                        customerVisit += _integerSolution[v,s,e];
                        BRow[_vehicles.Count * _vertices.Count * s + _vertices.Count * v + e] = 1.0;
                    }
                }
                _model.AddConstr(customerVisit, GRB.EQUAL, 1.0, "_EachCustomerMustVisitedOnce");
                _b.Add(1.0);
                _B.Add(BRow);
            }
        }

        private void AllVehiclesMustStartFromTheDepot()
        {
            for (int v = 0; v < _vehicles.Count; v++)
            {
                var vehicleStart = 0.0;
                var BRow = new List<double>();
                BRow.AddRange(Enumerable.Repeat(0.0, _vehicles.Count * _vertices.Count * _vertices.Count));
                for (int e = 0; e < _vertices.Count; e++)
                {
                    vehicleStart += _integerSolution[v,0,e];
                    BRow[_vertices.Count * v + e] = 1.0;
                }
                _model.AddConstr(vehicleStart, GRB.EQUAL, 1.0, "_AllVehiclesMustStartFromTheDepot");
                _b.Add(1.0);
                _B.Add(BRow);
            }
        }

        private void AllVehiclesMustEndAtTheDepot()
        {
            for (int v = 0; v < _vehicles.Count; v++)
            {
                var vehicleEnd = 0.0;
                for (int s = 0; s < _vertices.Count; s++)
                {
                    vehicleEnd += _integerSolution[v,s,_vertices.Count - 1];
                }
                _model.AddConstr(vehicleEnd, GRB.EQUAL, 1.0, "_AllVehiclesMustEndAtTheDepot");
            }
        }

        private void VehiclesMustLeaveTheArrivingCustomer()
        {
            for (int v = 0; v < _vehicles.Count; v++)
            {
                for (int s = 1; s <= _vertices.Count - 2; s++)
                {
                    var arrival = 0.0;
                    var leave = 0.0;
                    for (int e = 0; e < _vertices.Count; e++)
                    {
                        arrival += _integerSolution[v,e,s];
                        leave += _integerSolution[v,s,e];
                    }
                    _model.AddConstr(arrival - leave, GRB.EQUAL, 0.0, "_VehiclesMustLeaveTheArrivingCustomer");
                }
            }
        }

        private void VehiclesLoadUpCapacity()
        {
            for (int v = 0; v < _vehicles.Count; v++)
            {
                var vehicleCapacity = 0.0;
                for (int s = 1; s <= _vertices.Count - 2; s++)
                {
                    for (int e = 0; e < _vertices.Count; e++)
                    {
                        vehicleCapacity += _vertices[s].Demand * _integerSolution[v,s,e];
                    }
                }
                _model.AddConstr(vehicleCapacity, GRB.LESS_EQUAL, _vehicles[v].Capacity, "_VehiclesLoadUpCapacity");
            }
        }

        private void DepartureFromACustomerAndItsImmediateSuccessor()
        {
            for (int v = 0; v < _vehicles.Count; v++)
            {
                for (int s = 0; s < _vertices.Count; s++)
                {
                    for (int e = 0; e < _vertices.Count; e++)
                    {
                        _model.AddConstr(_serviceStart[v][s]
                                        + Helpers.CalculateDistance(_vertices[s], _vertices[e])
                                        + _vertices[s].ServiceTime
                                        - BigM() * (1 - _integerSolution[v,s,e])
                                        - _serviceStart[v][e]
                                        , GRB.LESS_EQUAL
                                        , 0.0
                                        , "_DepartureFromACustomerAndItsImmediateSuccessor");
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
                    _model.AddConstr(_serviceStart[v][s], GRB.LESS_EQUAL,
                                     _vertices[s].TimeEnd, "_TimeWindowsMustBeSatisfied_Upper");
                    _model.AddConstr(_serviceStart[v][s], GRB.GREATER_EQUAL,
                                     _vertices[s].TimeStart, "_TimeWindowsMustBeSatisfied_Lower");
                }
            }
        }

        private double BigM()
        {
            return _vertices[0].TimeEnd - _vertices[0].TimeStart;
        }
    }
}
