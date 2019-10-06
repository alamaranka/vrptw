using Gurobi;
using System.Collections.Generic;
using System.Linq;
using VRPTW.Configuration;
using VRPTW.Helper;
using VRPTW.Model;

namespace VRPTW.Algorithm.Benders
{
    class MasterProblem
    {
        public GRBModel _model { get; }
        private List<List<List<GRBVar>>> _vehicleTraverse;
        private GRBVar _z;       
        private List<Vehicle> _vehicles;
        private List<Customer> _vertices;
        private int _numberOfFeasibilityCuts = 0;
        private int _numberOfOptimalityCuts = 0;
        
        public MasterProblem(GRBEnv env, List<Vehicle> vehicles, List<Customer> vertices)
        {
            _vehicles = vehicles;
            _vertices = vertices;
            _model = new GRBModel(env);
            Generate();
        }

        public void AddFeasibilityCut(Dictionary<(int, int), double> B, List<double> b, double[] u)
        {
            var expr = new GRBLinExpr();
            for (int x = 0; x < u.Length; x++)
            {
                var By = new GRBLinExpr();          
                var elementsWithItem1IsX = B.Where(b => b.Key.Item1 == x).ToList();
                for (var y = 0; y < elementsWithItem1IsX.Count; y++)
                {
                    var k1 = elementsWithItem1IsX[y].Key.Item1;
                    var k2 = elementsWithItem1IsX[y].Key.Item2;
                    if (!B.TryGetValue((k1, k2), out double value))
                        value = 0.0;
                    var v = k2 / (_vertices.Count * _vertices.Count);
                    var s = (k2 % (_vertices.Count * _vertices.Count)) / _vertices.Count;
                    var e = (k2 % (_vertices.Count * _vertices.Count)) % _vertices.Count;
                    By += value * _vehicleTraverse[v][s][e];
                }
                expr += (b[x] - By) * u[x];
            }                   
            _model.AddConstr(expr, GRB.LESS_EQUAL, 0.0, "AddFeasibilityCut_" + _numberOfFeasibilityCuts++);
            var con = _model.GetConstrs();

        }

        public void AddOptimalityCut(Dictionary<(int, int), double> B, List<double> b, double[] u)
        {
            var expr = new GRBLinExpr();
            for (int x = 0; x < u.Length; x++)
            {
                var fy = new GRBLinExpr();
                var By = new GRBLinExpr();
                var elementsWithItem1IsX = B.Where(b => b.Key.Item1 == x).ToList();
                for (var y = 0; y < elementsWithItem1IsX.Count; y++)
                {
                    var k1 = elementsWithItem1IsX[y].Key.Item1;
                    var k2 = elementsWithItem1IsX[y].Key.Item2;
                    if (!B.TryGetValue((k1, k2), out double value))
                        value = 0.0;
                    var v = k2 / (_vertices.Count * _vertices.Count);
                    var s = (k2 % (_vertices.Count * _vertices.Count)) / _vertices.Count;
                    var e = (k2 % (_vertices.Count * _vertices.Count)) % _vertices.Count;
                    fy += Helpers.CalculateDistance(_vertices[s], _vertices[e]) * _vehicleTraverse[v][s][e];
                    By += value * _vehicleTraverse[v][s][e];
                }
                expr += fy + (b[x] - By) * u[x];
            }
            _model.AddConstr(_z, GRB.GREATER_EQUAL, expr, "AddOptimalityCut_" + _numberOfOptimalityCuts++);
        }

        public double[,,] GetSolution()
        {
            var solution = new double[_vehicles.Count, _vertices.Count, _vertices.Count];
            for (int v = 0; v < _vehicles.Count; v++)
            {
                for (int s = 0; s < _vertices.Count; s++)
                {
                    for (int e = 0; e < _vertices.Count; e++)
                    {
                        solution[v, s, e] = _vehicleTraverse[v][s][e].Get(GRB.DoubleAttr.X);
                    }
                }
            }
            return solution;
        }

        private void Generate()
        {
            SetSolverParameters();
            InitializeDecisionVariables();
            CreateBinaryDecisionVariables();
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
            _vehicleTraverse = new List<List<List<GRBVar>>>();
            _z = new GRBVar();
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
            _z = _model.AddVar(double.MinValue, double.MaxValue, 0, GRB.CONTINUOUS, "");
        }

        private void CreateObjective()
        {
            var cost = new GRBLinExpr();
            cost.AddTerm(1.0, _z);
            _model.SetObjective(cost, GRB.MINIMIZE);
        }

        private void CreateConstraints()
        {
            UnAllowedTraverses();
            EachCustomerMustBeVisitedOnce();
            AllVehiclesMustStartFromTheDepot();
            AllVehiclesMustEndAtTheDepot();
            VehiclesMustLeaveTheArrivingCustomer();
            VehiclesLoadUpCapacity();
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
                    var flow = new GRBLinExpr();
                    for (int e = 0; e < _vertices.Count; e++)
                    {
                        flow.Add(_vehicleTraverse[v][e][s] - _vehicleTraverse[v][s][e]);
                    }
                    _model.AddConstr(flow, GRB.EQUAL, 0.0, "_VehiclesMustLeaveTheArrivingCustomer");
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
    }
}
