using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRPTW.Model;

namespace VRPTW.Heuristics
{
    public class Diversifier
    {
        private Solution _solution;
        private int _dMin;
        private int _dMax;

        public Diversifier(Solution solution, int dMin, int dMax)
        {
            _solution = solution;
            _dMin = dMin;
            _dMax = dMax;
        }

        public void Diverisfy()
        {
            var allCustomers = _solution.Routes
                .SelectMany(c => c.Customers)
                .Where(p => p.Id != 0 && p.Id != 26)
                .GroupBy(i => i.Id)
                .Select(g => g.FirstOrDefault())
                .ToList();
            var numberOfCustomersToRemove = new Random().Next(_dMin, _dMax);
            var removedCustomers = new List<Customer>();

            for (var i = 0; i < numberOfCustomersToRemove; i++)
            {
                var randIndex = new Random().Next(allCustomers.Count);

                if (!removedCustomers.Contains(allCustomers[randIndex]))
                {
                    allCustomers.Remove(allCustomers[randIndex]);
                    removedCustomers.Add(allCustomers[randIndex]);
                }
                else
                {
                    i--;
                }
            }
        }
    }
}
