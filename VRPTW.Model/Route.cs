using System;
using System.Collections.Generic;
using System.Text;

namespace VRPTW.Model
{
    public class Route
    {
        public List<Customer> Customers { get; set; }
        public double Capacity { get; set; }
        public double Distance { get; set; }
        public Route Clone()
        {
            return this.MemberwiseClone() as Route;
        }
    }
}
