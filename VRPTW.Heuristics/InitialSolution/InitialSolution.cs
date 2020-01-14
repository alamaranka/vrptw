using System.Collections.Generic;
using System.Linq;
using VRPTW.Helper;
using VRPTW.Model;

namespace VRPTW.Heuristics
{
    public class InitialSolution
    {
        private readonly Dataset _dataset;
        private Customer _depot;
        private List<Customer> _unRoutedCustomers;
        private double _routeCapacity;

        public InitialSolution(Dataset dataset)
        {
            _dataset = dataset;
        }

        private void SetInputData()
        {
            _depot = _dataset.Vertices[0];
            _unRoutedCustomers = _dataset.Vertices.GetRange(1, _dataset.Vertices.Count - 1);
            _routeCapacity = _dataset.Vehicles[0].Capacity;
        }

        public Solution Run()
        {
            SetInputData();
            var routes = new List<Route>();

            while (_unRoutedCustomers.Count > 0)
            {
                var route = new InsertionHeuristics(_depot, _unRoutedCustomers, _routeCapacity).Generate();
                route.Id = routes.Count;
                route.Capacity = _routeCapacity;
                routes.Add(route);
                _unRoutedCustomers = _unRoutedCustomers.Except(route.Customers).ToList();
            }

            ResetReturningDepotName(routes);

            var solution = new Solution()
            {
                Routes = routes,
                Cost = routes.Sum(d => d.Distance)
            };

            return solution;
        }

        private void ResetReturningDepotName(List<Route> routes)
        {
            foreach (var r in routes)
            {
                r.Customers.Last().Id = _dataset.Vertices.Count;
            }
        }
    }
}
