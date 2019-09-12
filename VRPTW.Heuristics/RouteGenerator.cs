using System;
using System.Collections.Generic;
using System.Linq;
using VRPTW.Configuration;
using VRPTW.Helper;
using VRPTW.Model;

namespace VRPTW.Heuristics
{
    public class RouteGenerator
    {
        private readonly Customer _depot;
        private readonly List<Customer> _candidateCustomers;
        private readonly double _routeMaxCapacity;
        public Route _route;

        public RouteGenerator(Customer depot, List<Customer> unRoutedCustomers, double routeMaxCapacity)
        {
            _depot = depot;
            _candidateCustomers = unRoutedCustomers;
            _routeMaxCapacity = routeMaxCapacity;
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
                        if (IsFeasibleToInsert(previous, candidate, next))
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
                Customers = new List<Customer>() { _depot, _depot.Clone() },
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
                _route.Customers[p].ServiceStart = CalculateServiceStart(_route.Customers[p - 1], _route.Customers[p]);
            }
            _route.Load += candidate.Demand;
            _route.Distance = _route.Distance -
                              DistanceCalculator.Calculate(previous, next) +
                              DistanceCalculator.Calculate(previous, candidate) +
                              DistanceCalculator.Calculate(candidate, next);
            _candidateCustomers.Remove(candidate);
        }

        private bool IsFeasibleToInsert(Customer previous, Customer candidate, Customer next)
        {
            if (_route.Load + candidate.Demand > _routeMaxCapacity)
            {
                return false;
            }

            var previousServiceTime = CalculateServiceStart(previous, candidate);
            if (previousServiceTime < candidate.TimeStart || previousServiceTime > candidate.TimeEnd)
            {
                return false;
            }
            var previousCustomer = candidate;

            for (var p = _route.Customers.IndexOf(next); p < _route.Customers.Count; p++)
            {
                var newServiceStartTime = CalculateServiceStart(previousCustomer, previousServiceTime, _route.Customers[p]);
                if (newServiceStartTime < _route.Customers[p].TimeStart || newServiceStartTime > _route.Customers[p].TimeEnd)
                {
                    return false;
                }
                previousServiceTime = newServiceStartTime;
                previousCustomer = _route.Customers[p];
            }
            return true;
        }

        private double CalculateServiceStart(Customer previous, Customer next)
        {
            return Math.Max(next.TimeStart,
                   previous.ServiceStart +
                   previous.ServiceTime +
                   DistanceCalculator.Calculate(previous, next));
        }

        private double CalculateServiceStart(Customer previous, double previousServiceStart, Customer next)
        {
            return Math.Max(next.TimeStart,
                   previousServiceStart +
                   previous.ServiceTime +
                   DistanceCalculator.Calculate(previous, next));
        }

        private double InsertionValueOfCustomer(Customer previous, Customer candidate, Customer next)
        {
            var alpha1 = Config.GetHeuristicsParam().InitialSolutionParam.Alpha1;
            var alpha2 = Config.GetHeuristicsParam().InitialSolutionParam.Alpha2;
            var mu = Config.GetHeuristicsParam().InitialSolutionParam.Mu;
            var lambda = Config.GetHeuristicsParam().InitialSolutionParam.Lambda;
            var c11 = DistanceCalculator.Calculate(previous, candidate) +
                      DistanceCalculator.Calculate(candidate, next) -
                      DistanceCalculator.Calculate(previous, next) * mu;
            var candidateStartTime = Math.Max(previous.ServiceStart +
                                     previous.ServiceTime +
                                     DistanceCalculator.Calculate(previous, candidate),
                                     candidate.TimeStart);
            var c12 = Math.Max(candidateStartTime +
                      candidate.ServiceTime +
                      DistanceCalculator.Calculate(candidate, next),
                      candidate.TimeStart) -
                      next.ServiceStart;
            var c1 = alpha1 * c11 + alpha2 * c12;
            return lambda * DistanceCalculator.Calculate(_depot, candidate) - c1;
        }

        private Customer GetSeedCustomer()
        {
            var maxDistance = 0.0;
            var seedCustomer = new Customer();
            foreach (var customer in _candidateCustomers)
            {
                var distance = DistanceCalculator.Calculate(_depot, customer);
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
