using System.Collections.Generic;
using System.Linq;
using VRPTW.Model;

namespace VRPTW.Heuristics
{
    public class InitialSolution
    {
        private readonly Dataset _dataset;
        private Customer _depot;
        private List<Vehicle> _vehicles;
        private List<Customer> _customers;
        private List<Customer> _unRoutedCustomers;
        private double _routeMaxCapacity;

        public InitialSolution(Dataset dataset)
        {
            _dataset = dataset;
        }

        private void SetInputData()
        {
            _customers = _dataset.Vertices;
            _vehicles = _dataset.Vehicles;
            _depot = _customers[0];
            _customers.RemoveAt(0);
            _unRoutedCustomers = _customers.ToList();
            _routeMaxCapacity = _vehicles[0].Capacity;
        }

        public Solution Get()
        {
            SetInputData();
            var routes = new List<Route>();
            while (_unRoutedCustomers.Count > 0)
            {
                var route = new RouteGenerator(_depot, _unRoutedCustomers, _routeMaxCapacity).Generate();
                routes.Add(route);
                _unRoutedCustomers = _unRoutedCustomers.Except(route.Customers).ToList();
            }
            return new Solution()
            {
                Routes = routes,
                Cost = routes.Sum(d => d.Distance)
            };
        }
    }
}
