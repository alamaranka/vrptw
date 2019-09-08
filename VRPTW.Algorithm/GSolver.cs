using Gurobi;
using System;
using System.Collections.Generic;
using System.Linq;
using VRPTW.Configuration;
using VRPTW.Helper;
using VRPTW.Model;

namespace VRPTW.Algorithm
{
    public class GSolver
    {
        private GRBEnv _env;
        private GRBModel _model;
        private int _status;
        private Dataset _dataset;
        private List<Vehicle> _vehicles;
        private List<Customer> _vertices;
        private List<List<List<GRBVar>>> _vehicleTraverse;
        private List<List<GRBVar>> _serviceStart;

        private int _numberOfVertices;
        private int _numberOfCustomers;
        private int _numberOfVehicles;

        private GRBLinExpr _cost;

        private const int BIGM = 1_000;
        private readonly double _timeLimit;
        private readonly double _mipGap;
        private readonly int _threads;

        private Solution _solution = new Solution();

        public GSolver(Dataset dataset)
        {
            _dataset = dataset;
            _timeLimit = Config.GetSolverParam().TimeLimit;
            _mipGap = Config.GetSolverParam().MIPGap;
            _threads = Config.GetSolverParam().Threads;
        }

        public Solution Run()
        {
            SetInputData();
            InitializeEnvAndModel();
            SetSolverParameters();
            InitializeDecisionVariables();
            CreateDecisionVariables();
            CreateObjective();
            CreateConstraints();
            Solve();
            return Output();
        }

        private void SetInputData()
        {
            _vertices = _dataset.Vertices;
            _vertices.Add(_vertices[0].Clone());
            _vehicles = _dataset.Vehicles;
            _numberOfVertices = _vertices.Count;
            _numberOfCustomers = _vertices.Count - 2;
            _numberOfVehicles = _vehicles.Count;
        }

        private void InitializeEnvAndModel()
        {
            _env = new GRBEnv("VRPTW");
            _model = new GRBModel(_env);
        }

        private void SetSolverParameters()
        {
            _model.Parameters.TimeLimit = _timeLimit;
            _model.Parameters.MIPGap = _mipGap;
            _model.Parameters.Threads = _threads;
        }

        private void InitializeDecisionVariables()
        {
            _vehicleTraverse = new List<List<List<GRBVar>>>();
            _serviceStart = new List<List<GRBVar>>();
        }

        private void CreateDecisionVariables()
        {
            for (int v = 0; v < _numberOfVehicles; v++)
            {
                List<List<GRBVar>> vehicleTraverseV = new List<List<GRBVar>>();
                List<GRBVar> serviceStartV = new List<GRBVar>();
                for (int s = 0; s < _numberOfVertices; s++)
                {
                    List<GRBVar> vehicleTraverseC = new List<GRBVar>();
                    serviceStartV.Add(_model.AddVar(0, BIGM, 0, GRB.CONTINUOUS, ""));
                    for (int e = 0; e < _numberOfVertices; e++)
                    {
                        vehicleTraverseC.Add(_model.AddVar(0, 1, 0, GRB.BINARY, ""));
                    }
                    vehicleTraverseV.Add(vehicleTraverseC);
                }
                _vehicleTraverse.Add(vehicleTraverseV);
                _serviceStart.Add(serviceStartV);
            }
        }

        private void CreateObjective()
        {
            _cost = new GRBLinExpr();
            for (int v = 0; v < _numberOfVehicles; v++)
            {
                for (int s = 0; s < _numberOfVertices; s++)
                {
                    for (int e = 0; e < _numberOfVertices; e++)
                    {
                        var distance = DistanceCalculator.Calculate(_vertices[s], _vertices[e]);
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
            for (int v = 0; v < _numberOfVehicles; v++)
            {
                for (int s = 0; s < _numberOfVertices; s++)
                {
                    _model.AddConstr(_vehicleTraverse[v][s][s], GRB.EQUAL, 0.0, "_UnAllowedTraverses_S");
                    _model.AddConstr(_vehicleTraverse[v][s][0], GRB.EQUAL, 0.0, "_UnAllowedTraverses_0");
                    _model.AddConstr(_vehicleTraverse[v][_numberOfVertices - 1][s], GRB.EQUAL, 0.0, "_UnAllowedTraverses_N+1");
                }
            }
        }

        private void EachCustomerMustBeVisitedOnce()
        {
            for (int s = 1; s <= _numberOfCustomers; s++)
            {
                var customerVisit = new GRBLinExpr();
                for (int v = 0; v < _numberOfVehicles; v++)
                {
                    for (int e = 0; e < _numberOfVertices; e++)
                    {
                        customerVisit.AddTerm(1.0, _vehicleTraverse[v][s][e]);
                    }
                }
                _model.AddConstr(customerVisit, GRB.EQUAL, 1.0, "_EachCustomerMustVisitedOnce");
            }
        }

        private void AllVehiclesMustStartFromTheDepot()
        {
            for (int v = 0; v < _numberOfVehicles; v++)
            {
                var vehicleStart = new GRBLinExpr();
                for (int e = 0; e < _numberOfVertices; e++)
                {
                    vehicleStart.AddTerm(1.0, _vehicleTraverse[v][0][e]);
                }
                _model.AddConstr(vehicleStart, GRB.EQUAL, 1.0, "_AllVehiclesMustStartFromTheDepot");
            }
        }

        private void AllVehiclesMustEndAtTheDepot()
        {
            for (int v = 0; v < _numberOfVehicles; v++)
            {
                var vehicleStart = new GRBLinExpr();
                for (int s = 0; s < _numberOfVertices; s++)
                {
                    vehicleStart.AddTerm(1.0, _vehicleTraverse[v][s][_numberOfVertices - 1]);
                }
                _model.AddConstr(vehicleStart, GRB.EQUAL, 1.0, "_AllVehiclesMustEndAtTheDepot");
            }
        }

        private void VehiclesMustLeaveTheArrivingCustomer()
        {
            for (int v = 0; v < _numberOfVehicles; v++)
            {
                for (int s = 1; s <= _numberOfCustomers; s++)
                {
                    var arrival = new GRBLinExpr();
                    var leave = new GRBLinExpr();
                    for (int e = 0; e < _numberOfVertices; e++)
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
            for (int v = 0; v < _numberOfVehicles; v++)
            {
                var vehicleCapacity = new GRBLinExpr();
                for (int s = 1; s <= _numberOfCustomers; s++)
                {
                    for (int e = 0; e < _numberOfVertices; e++)
                    {
                        vehicleCapacity.AddTerm(_vertices[s].Demand, _vehicleTraverse[v][s][e]);
                    }
                }
                _model.AddConstr(vehicleCapacity, GRB.LESS_EQUAL, _vehicles[v].Capacity, "_VehiclesLoadUpCapacity");
            }
        }

        private void DepartureFromACustomerAndItsImmediateSuccessor()
        {
            for (int v = 0; v < _numberOfVehicles; v++)
            {
                for (int s = 0; s < _numberOfVertices; s++)
                {
                    for (int e = 0; e < _numberOfVertices; e++)
                    {
                        _model.AddConstr(_serviceStart[v][s]
                                        + DistanceCalculator.Calculate(_vertices[s], _vertices[e])
                                        + _vertices[s].ServiceTime
                                        - BIGM * (1 - _vehicleTraverse[v][s][e])
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
            for (int v = 0; v < _numberOfVehicles; v++)
            {
                for (int s = 0; s < _numberOfVertices; s++)
                {
                    _model.AddConstr(_serviceStart[v][s], GRB.LESS_EQUAL
                                    , _vertices[s].TimeEnd, "_TimeWindowsMustBeSatisfied_Upper");
                    _model.AddConstr(_serviceStart[v][s], GRB.GREATER_EQUAL
                                    , _vertices[s].TimeStart, "_TimeWindowsMustBeSatisfied_Lower");
                }
            }
        }

        private void Solve()
        {
            _model.Optimize();
            _status = _model.Status;
        }

        private Solution GenerateRoutes()
        {
            var routes = new List<Route>();
            for (int vehicle = 0; vehicle < _numberOfVehicles; vehicle++)
            {
                var customers = new List<Customer>();
                var currentVertex = 0;
                var totalDistanceOfTheRoute = 0.0;
                for (int destVertex = 0; destVertex < _numberOfVertices; destVertex++)
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
                        totalDistanceOfTheRoute += DistanceCalculator
                                                        .Calculate(_vertices[currentVertex], _vertices[destVertex]);
                        currentVertex = destVertex;
                        destVertex = 0;
                    }
                }
                if (customers.Count > 2)
                {
                    var route = new Route()
                    {
                        Customers = customers,
                        Load = customers.Sum(c => c.Demand),
                        Distance = totalDistanceOfTheRoute
                    };
                    route.Customers.ToList().ForEach(c => c.RoutePlanned = route);
                    routes.Add(route);
                }
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