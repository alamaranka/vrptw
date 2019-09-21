using Gurobi;
using System;
using System.Collections.Generic;
using System.Text;
using VRPTW.Configuration;
using VRPTW.Helper;
using VRPTW.Model;

namespace VRPTW.Algorithm.Benders
{
    class SubProblem
    {
        private GRBEnv _env;
        private GRBModel _model;
        private int _status;
        private List<List<GRBVar>> _serviceStart;
        private GRBLinExpr _cost;
        
        private List<Vehicle> _vehicles;
        private List<Customer> _vertices;
        private readonly double[,,] _binarySolution;
        
        public SubProblem(GRBEnv env, List<Vehicle> vehicles, List<Customer> vertices, double[,,] binarySolution)
        {
            _vehicles = vehicles;
            _vertices = vertices;
            _model = new GRBModel(env);
            _env = env;
            _binarySolution = binarySolution;
            Generate();
        }

        public GRBModel GetModel()
        {
            return _model;
        }

        public void DisposeModel()
        {
            _model.Dispose();
        }

        private void Generate()
        {
            InitializeModel();
            SetSolverParameters();
            InitializeDecisionVariables();
            CreateGeneralDecisionVariables();
            CreateObjective();
            CreateConstraints();
        }

        private void InitializeModel()
        {
            _model = new GRBModel(_env);
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
                    serviceStartV.Add(_model.AddVar(0, BigM(), 0, GRB.CONTINUOUS, ""));
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
                        _cost += distance * _binarySolution[v,s,e];
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
            for (int v = 0; v < _vehicles.Count; v++)
            {
                for (int s = 0; s < _vertices.Count; s++)
                {
                    _model.AddConstr(_binarySolution[v,s,s], GRB.EQUAL, 0.0, "_UnAllowedTraverses_S");
                    _model.AddConstr(_binarySolution[v,s,0], GRB.EQUAL, 0.0, "_UnAllowedTraverses_0");
                    _model.AddConstr(_binarySolution[v,_vertices.Count - 1,s], GRB.EQUAL, 0.0, "_UnAllowedTraverses_N+1");
                }
            }
        }

        private void EachCustomerMustBeVisitedOnce()
        {
            for (int s = 1; s <= _vertices.Count - 2; s++)
            {
                var customerVisit = 0.0;
                for (int v = 0; v < _vehicles.Count; v++)
                {
                    for (int e = 0; e < _vertices.Count; e++)
                    {
                        customerVisit += _binarySolution[v,s,e];
                    }
                }
                _model.AddConstr(customerVisit, GRB.EQUAL, 1.0, "_EachCustomerMustVisitedOnce");
            }
        }

        private void AllVehiclesMustStartFromTheDepot()
        {
            for (int v = 0; v < _vehicles.Count; v++)
            {
                var vehicleStart = 0.0;
                for (int e = 0; e < _vertices.Count; e++)
                {
                    vehicleStart += _binarySolution[v,0,e];
                }
                _model.AddConstr(vehicleStart, GRB.EQUAL, 1.0, "_AllVehiclesMustStartFromTheDepot");
            }
        }

        private void AllVehiclesMustEndAtTheDepot()
        {
            for (int v = 0; v < _vehicles.Count; v++)
            {
                var vehicleStart = 0.0;
                for (int s = 0; s < _vertices.Count; s++)
                {
                    vehicleStart += _binarySolution[v,s,_vertices.Count - 1];
                }
                _model.AddConstr(vehicleStart, GRB.EQUAL, 1.0, "_AllVehiclesMustEndAtTheDepot");
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
                        arrival += _binarySolution[v,e,s];
                        leave += _binarySolution[v,s,e];
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
                        vehicleCapacity += _vertices[s].Demand * _binarySolution[v,s,e];
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
                                        - BigM() * (1 - _binarySolution[v,s,e])
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
