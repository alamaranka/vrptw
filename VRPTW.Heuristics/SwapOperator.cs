using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRPTW.Model;

namespace VRPTW.Heuristics
{
    public class SwapOperator
    {
        public Solution _solution;

        public SwapOperator(Solution solution)
        {
            _solution = solution;
            ApplySwapOperator();
        }

        private void ApplySwapOperator()
        {
            var improved = true;

            while (improved)
            {
                for (var r1 = 0; r1 < _solution.Routes.Count - 1; r1++)
                {
                    for (var r2 = r1 + 1; r2 < _solution.Routes.Count; r2++)
                    {
                        for (var i = 1; i < _solution.Routes[r1].Customers.Count - 1; i++)
                        {
                            for (var j = 1; j < _solution.Routes[r2].Customers.Count - 1; j++)
                            {
                                var currentDistance = _solution.Routes.Sum(r => r.Distance);
                            }
                        }
                    }
                }
            }

            _solution.Cost = _solution.Routes.Sum(r => r.Distance);
        }
    }
}
