using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRPTW.Helper;
using VRPTW.Model;

namespace VRPTW.Heuristics
{
    public class TwoOptOperator
    {
        public Solution _solution;
        private Route _route;

        public TwoOptOperator(Solution solution)
        {
            _solution = solution;
            _route = new Route();
            Apply2OptOperator();
        }

        private void Apply2OptOperator()
        {
            foreach (var route in _solution.Routes.Where(r => r.Customers.Count > 2))
            {
                var improved = true;
                while (improved)
                {
                    for (var i = 1; i < route.Customers.Count - 2; i++)
                    {
                        for (var j = i + 1; j < route.Customers.Count - 1; j++)
                        {
                            var clone = Helpers.Clone(route);
                            var currentDistance = clone.Distance;
                            var newCustomerList = GenerateNewCustomerList(clone, i, j);
                            if (GenerateNewRoute(newCustomerList))
                            {

                            }
                        }
                    }
                }
            }
        }
        
        private List<Customer> GenerateNewCustomerList(Route route, int i, int j)
        {
            var newCustomerList = new List<Customer>();
            newCustomerList.AddRange(route.Customers.GetRange(0, i));
            var opt = route.Customers.GetRange(i, j - i + 1);
            opt.Reverse();
            newCustomerList.AddRange(opt);
            newCustomerList.AddRange(route.Customers.GetRange(j + 1, route.Customers.Count - j - 1));
            return newCustomerList;
        }

        private bool GenerateNewRoute(List<Customer> customers)
        {
            var isFeasible = false;

            return isFeasible;
        }

        private bool CheckRouteFeasibility()
        {
            return false;
        }
    }
}
