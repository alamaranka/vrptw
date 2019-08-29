using Gurobi;
using System;
using System.Collections.Generic;
using System.Text;
using VRPTW.Data;
using VRPTW.Helper;
using VRPTW.Model;

namespace VRPTW.Algorithm
{
    public class GSolver
    {
        private GRBEnv _env;
        private GRBModel _model;
        private int _status;
        private List<Vehicle> _vehicles;
        private List<Customer> _vertices;
        private List<List<List<GRBVar>>> _vehicleTraverse;
        private List<List<GRBVar>> _serviceStart;

        private int _numberOfVertices;
        private int _numberOfCustomers;
        private int _numberOfVehicles;

        private GRBLinExpr _cost;

        private const int BIGM = 1_000;
        private readonly string _dataSource;
        private readonly double _timeLimit;
        private readonly double _mipGap;

        public GSolver(string dataSource, double timeLimit, double mipGap)
        {
            _dataSource = dataSource;
            _timeLimit = timeLimit;
            _mipGap = mipGap;
        }

        public void Run()
        {
            SetInputData();
            InitializeEnvAndModel();
            SetSolverParameters();
            InitializeDecisionVariables();
            CreateDecisionVariables();
            CreateObjective();
            CreateConstraints();
            Solve();
            Output();
        }

        private void SetInputData()
        {
            switch (_dataSource)
            {
                case "database":
                    var dBReader = new DBReader();
                    _vertices = dBReader.GetVertices();
                    _vehicles = dBReader.GetVehicles();
                    break;
                case "xml":
                    XMLReader xMLReader = new XMLReader();
                    _vertices = xMLReader.GetVertices();
                    _vehicles = xMLReader.GetVehicles();
                    break;
                default:
                    break;
            }

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

        private void PrintRoutes()
        {
            for (int v = 0; v < _numberOfVehicles; v++)
            {
                var route = new List<string>() { "0" };
                var currentVertex = 0;
                for (int s = 0; s < _numberOfVertices; s++)
                {
                    for (int e = 0; e < _numberOfVertices; e++)
                    {
                        if (Math.Round(_vehicleTraverse[v][currentVertex][e].Get(GRB.DoubleAttr.X)) == 1.0)
                        {
                            route.Add(_vertices[e].Name);
                            currentVertex = e;
                        }
                    }
                }
                Console.Write("Vehicle {0}: ", v + 1);
                foreach (var c in route)
                {
                    Console.Write(c + " ");
                }
                Console.WriteLine();
            }
        }

        private void Output()
        {
            if (_status == GRB.Status.INFEASIBLE || _status == GRB.Status.UNBOUNDED)
            {
                Console.WriteLine("Infeasible or unbounded solution!");
            }
            else
            {
                PrintRoutes();
            }
        }

    }
}