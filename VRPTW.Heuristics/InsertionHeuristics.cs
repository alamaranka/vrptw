using System;
using System.Collections.Generic;
using System.Linq;
using VRPTW.Configuration;
using VRPTW.Helper;
using VRPTW.Model;

namespace VRPTW.Heuristics
{
    public class InsertionHeuristics
    {
        private readonly Customer _depot;
        private readonly List<Customer> _candidateCustomers;
        private readonly double _routeCapacity;
        public Route _route;

        public InsertionHeuristics(Customer depot, List<Customer> unRoutedCustomers, double routeCapacity)
        {
            _depot = depot;
            _candidateCustomers = unRoutedCustomers;
            _routeCapacity = routeCapacity;
        }

        public Route Generate()
        {
            InitializeRoute();

            while (_candidateCustomers.Count > 0)
            {
                var anyFeasibleCustomer = false;

                for (var p = 1; p < _route.Customers.Count; p++)
                {
                    var previous = _route.Customers[p - 1];
                    var next = _route.Customers[p];
                    var feasibleCustomersToInsert = new List<Customer>();
                    var insertionValueOfFeasibleCustomers = new List<double>();
                    foreach (var candidate in _candidateCustomers)
                    {
                        if (IsFeasibleToInsert(candidate, next))
                        {
                            feasibleCustomersToInsert.Add(candidate);
                            insertionValueOfFeasibleCustomers
                                .Add(InsertionValueOfCustomer(previous, candidate, next));
                        }
                    }
                    if (feasibleCustomersToInsert.Count > 0)
                    {
                        var indexOfBestFeasibleCustomer = insertionValueOfFeasibleCustomers
                                                                .IndexOf(insertionValueOfFeasibleCustomers.Max());
                        var bestCustomerToInsert = feasibleCustomersToInsert[indexOfBestFeasibleCustomer];
                        InsertCustomerToTheRoute(previous, bestCustomerToInsert, next);
                        anyFeasibleCustomer = true;
                    }
                }
                if (!anyFeasibleCustomer) 
                {
                    break; 
                }
            }
            return _route;
        }

        private void InitializeRoute()
        {
            _route = new Route()
            {
                Customers = new List<Customer>() { _depot, Helpers.Clone(_depot) },
                Load = 0.0,
                Distance = 0.0
            };
            InsertCustomerToTheRoute(_route.Customers[0], GetSeedCustomer(), _route.Customers[1]);
        }

        private void InsertCustomerToTheRoute(Customer previous, Customer candidate, Customer next)
        {
            _route.Customers.Insert(_route.Customers.IndexOf(next), candidate);
            for (var p = _route.Customers.IndexOf(candidate); p < _route.Customers.Count; p++)
            {
                _route.Customers[p].ServiceStart = Helpers.CalculateServiceStart(_route.Customers[p - 1], _route.Customers[p]);
            }
            _route.Load += candidate.Demand;
            _route.Distance = _route.Distance -
                              Helpers.CalculateDistance(previous, next) +
                              Helpers.CalculateDistance(previous, candidate) +
                              Helpers.CalculateDistance(candidate, next);
            _candidateCustomers.Remove(candidate);
        }

        private bool IsFeasibleToInsert(Customer candidate, Customer next)
        {
            if (_route.Load + candidate.Demand > _routeCapacity)
            {
                return false;
            }

            var candidateRoute = Helpers.Clone(_route);
            candidateRoute.Customers.Insert(_route.Customers.IndexOf(next), candidate);

            for (var p = candidateRoute.Customers.IndexOf(candidate); p < candidateRoute.Customers.Count; p++)
            {
                var newServiceStartTime = Helpers.CalculateServiceStart(candidateRoute.Customers[p - 1], candidateRoute.Customers[p]);
                var isAfterTimeEnd = newServiceStartTime > candidateRoute.Customers[p].TimeEnd;
                candidateRoute.Customers[p].ServiceStart = newServiceStartTime;
                if (isAfterTimeEnd)
                {
                    return false;
                }
            }
            return true;
        }

        private double InsertionValueOfCustomer(Customer previous, Customer candidate, Customer next)
        {
            var alpha1 = Config.GetHeuristicsParam().InitialSolutionParam.Alpha1;
            var alpha2 = Config.GetHeuristicsParam().InitialSolutionParam.Alpha2;
            var mu = Config.GetHeuristicsParam().InitialSolutionParam.Mu;
            var lambda = Config.GetHeuristicsParam().InitialSolutionParam.Lambda;
            var c11 = Helpers.CalculateDistance(previous, candidate) +
                      Helpers.CalculateDistance(candidate, next) -
                      Helpers.CalculateDistance(previous, next) * mu;
            var candidateStartTime = Math.Max(previous.ServiceStart +
                                     previous.ServiceTime +
                                     Helpers.CalculateDistance(previous, candidate),
                                     candidate.TimeStart);
            var c12 = Math.Max(candidateStartTime +
                      candidate.ServiceTime +
                      Helpers.CalculateDistance(candidate, next),
                      candidate.TimeStart) -
                      next.ServiceStart;
            var c1 = alpha1 * c11 + alpha2 * c12;
            return lambda * Helpers.CalculateDistance(_depot, candidate) - c1;
        }

        private Customer GetSeedCustomer()
        {
            var maxDistance = 0.0;
            var seedCustomer = new Customer();
            foreach (var customer in _candidateCustomers)
            {
                var distance = Helpers.CalculateDistance(_depot, customer);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    seedCustomer = customer;
                }
            }
            _candidateCustomers.Remove(seedCustomer);
            return seedCustomer;
        }
    }
}
