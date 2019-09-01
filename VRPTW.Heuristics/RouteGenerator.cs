using System;
using System.Collections.Generic;
using System.Text;
using VRPTW.Model;

namespace VRPTW.Heuristics
{
    public class RouteGenerator
    {
        private List<Customer> _unRoutedCustomers;

        public RouteGenerator(List<Customer> unRoutedCustomers)
        {
            _unRoutedCustomers = unRoutedCustomers;
        }

        public Route Generate()
        {
            return new Route();
        }
    }
}
