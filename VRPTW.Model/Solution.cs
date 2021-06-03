using System;
using System.Collections.Generic;

namespace VRPTW.Model
{
    [Serializable]
    public class Solution : IClonable<Solution>
    {
        public List<Route> Routes { get; set; }
        public double Cost { get; set; }

        public Solution Clone()
        {
            var solution = new Solution
            {
                Routes = new List<Route>()
            };

            foreach (var route in Routes)
            {
                solution.Routes.Add(route);
            }
            solution.Cost = Cost;

            return solution;
        }
    }
}
