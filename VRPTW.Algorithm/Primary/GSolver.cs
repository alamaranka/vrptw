using Gurobi;
using System;
using System.Collections.Generic;
using System.Linq;
using VRPTW.Configuration;
using VRPTW.Helper;
using VRPTW.Model;

namespace VRPTW.Algorithm
{
    public class GSolver : IAlgorithm
    {
        private GRBEnv _env;
        private GRBModel _model;
        private int _status;
        private List<List<List<GRBVar>>> _vehicleTraverse;
        private List<List<GRBVar>> _serviceStart;
        private GRBLinExpr _cost;
        private readonly Dataset _dataset;
        private List<Vehicle> _vehicles;
        private List<Customer> _vertices;

        public GSolver(Dataset dataset)
        {
            _dataset = dataset;
        }

        public Solution Run()
        {
            SetInputData();
            InitializeEnv();
            InitializeModel();
            SetSolverParameters();
            InitializeDecisionVariables();
            CreateBinaryDecisionVariables();
            CreateGeneralDecisionVariables();
            CreateObjective();
            CreateConstraints();
            Solve();
            return Output();
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
            _vehicleTraverse = new List<List<List<GRBVar>>>();
            _serviceStart = new List<List<GRBVar>>();
        }

        private void CreateBinaryDecisionVariables()
        {
            for (int v = 0; v < _vehicles.Count; v++)
            {
                List<List<GRBVar>> vehicleTraverseV = new List<List<GRBVar>>();
                for (int s = 0; s < _vertices.Count; s++)
                {
                    List<GRBVar> vehicleTraverseC = new List<GRBVar>();
                    for (int e = 0; e < _vertices.Count; e++)
                    {
                        vehicleTraverseC.Add(_model.AddVar(0, 1, 0, GRB.BINARY, "vehicle_traverse_" + v + s + e));
                    }
                    vehicleTraverseV.Add(vehicleTraverseC);
                }
                _vehicleTraverse.Add(vehicleTraverseV);
            }
        }

        private void CreateGeneralDecisionVariables()
        {
            for (int v = 0; v < _vehicles.Count; v++)
            {
                List<GRBVar> serviceStartV = new List<GRBVar>();
                for (int s = 0; s < _vertices.Count; s++)
                {
                    serviceStartV.Add(_model.AddVar(0, BigM(), 0, GRB.CONTINUOUS, "service_start_" + v + s));
                }
                _serviceStart.Add(serviceStartV);
            }
        }

        private void CreateObjective()
        {
            _cost = new GRBLinExpr();
            for (int v = 0; v < _vehicles.Count; v++)
            {
                for (int s = 0; s < _vertices.Count; s++)
                {
                    for (int e = 0; e < _vertices.Count; e++)
                    {
                        var distance = Helpers.CalculateDistance(_vertices[s], _vertices[e]);
                        _cost.AddTerm(distance, _vehicleTraverse[v][s][e]);
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
            var unAllowedTraverses = new GRBLinExpr();
            for (int v = 0; v < _vehicles.Count; v++)
            {
                for (int s = 0; s < _vertices.Count; s++)
                {
                    unAllowedTraverses += _vehicleTraverse[v][s][s] +
                                          _vehicleTraverse[v][s][0] +
                                          _vehicleTraverse[v][_vertices.Count - 1][s];
                }
            }
            _model.AddConstr(unAllowedTraverses, GRB.EQUAL, 0.0, "_UnAllowedTraverses");
        }

        private void EachCustomerMustBeVisitedOnce()
        {
            for (int s = 1; s <= _vertices.Count - 2; s++)
            {
                var customerVisit = new GRBLinExpr();
                for (int v = 0; v < _vehicles.Count; v++)
                {
                    for (int e = 0; e < _vertices.Count; e++)
                    {
                        customerVisit.AddTerm(1.0, _vehicleTraverse[v][s][e]);
                    }
                }
                _model.AddConstr(customerVisit, GRB.EQUAL, 1.0, "_EachCustomerMustVisitedOnce");
            }
        }

        private void AllVehiclesMustStartFromTheDepot()
        {
            for (int v = 0; v < _vehicles.Count; v++)
            {
                var vehicleStart = new GRBLinExpr();
                for (int e = 0; e < _vertices.Count; e++)
                {
                    vehicleStart.AddTerm(1.0, _vehicleTraverse[v][0][e]);
                }
                _model.AddConstr(vehicleStart, GRB.EQUAL, 1.0, "_AllVehiclesMustStartFromTheDepot");
            }
        }

        private void AllVehiclesMustEndAtTheDepot()
        {
            for (int v = 0; v < _vehicles.Count; v++)
            {
                var vehicleEnd = new GRBLinExpr();
                for (int s = 0; s < _vertices.Count; s++)
                {
                    vehicleEnd.AddTerm(1.0, _vehicleTraverse[v][s][_vertices.Count - 1]);
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
                    var arrival = new GRBLinExpr();
                    var leave = new GRBLinExpr();
                    for (int e = 0; e < _vertices.Count; e++)
                    {
                        arrival.AddTerm(1.0, _vehicleTraverse[v][e][s]);
                        leave.AddTerm(1.0, _vehicleTraverse[v][s][e]);
                    }
                    _model.AddConstr(arrival - leave, GRB.EQUAL, 0.0, "_VehiclesMustLeaveTheArrivingCustomer");
                }
            }
        }

        private void VehiclesLoadUpCapacity()
        {
            for (int v = 0; v < _vehicles.Count; v++)
            {
                var vehicleCapacity = new GRBLinExpr();
                for (int s = 1; s <= _vertices.Count - 2; s++)
                {
                    for (int e = 0; e < _vertices.Count; e++)
                    {
                        vehicleCapacity.AddTerm(_vertices[s].Demand, _vehicleTraverse[v][s][e]);
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
                                        - BigM() * (1 - _vehicleTraverse[v][s][e])
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

        private void Solve()
        {
            _model.Optimize();
            _status = _model.Status;
        }

        private Solution GenerateRoutes()
        {
            var routes = new List<Route>();
            for (int vehicle = 0; vehicle < _vehicles.Count; vehicle++)
            {
                var customers = new List<Customer>();
                var currentVertex = 0;
                var totalDistanceOfTheRoute = 0.0;
                for (int destVertex = 0; destVertex < _vertices.Count; destVertex++)
                {
                    if (Math.Round(_vehicleTraverse[vehicle][currentVertex][destVertex].Get(GRB.DoubleAttr.X)) == 1.0)
                    {
                        if (currentVertex == 0)
                        {
                            _vertices[currentVertex].ServiceStart =
                                        _serviceStart[vehicle][currentVertex].Get(GRB.DoubleAttr.X);
                            customers.Add(_vertices[currentVertex]);
                        }
                        _vertices[destVertex].ServiceStart =
                                        _serviceStart[vehicle][destVertex].Get(GRB.DoubleAttr.X);
                        customers.Add(_vertices[destVertex]);
                        totalDistanceOfTheRoute += Helpers.CalculateDistance(_vertices[currentVertex], _vertices[destVertex]);
                        currentVertex = destVertex;
                        destVertex = 0;
                    }
                }
                var route = new Route()
                {
                    Customers = customers,
                    Load = customers.Sum(c => c.Demand),
                    Distance = totalDistanceOfTheRoute
                };
                routes.Add(route);
            }
            return new Solution()
            {
                Routes = routes,
                Cost = _model.ObjVal
            };
        }

        private Solution Output()
        {
            if (_status == GRB.Status.INFEASIBLE || _status == GRB.Status.UNBOUNDED)
            {
                Console.WriteLine("Infeasible or unbounded solution!");
                return null;
            }
            else
            {
                return GenerateRoutes();
            }
        }

    }
}