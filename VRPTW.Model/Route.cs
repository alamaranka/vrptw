using System;
using System.Collections.Generic;

namespace VRPTW.Model
{
    [Serializable]
    public class Route
    {
        public int Id { get; set; }
        public Vehicle Vehicle { get; set; }
        public List<Customer> Customers { get; set; }
        public double Load { get; set; }
        public double Distance { get; set; }
    }
}
