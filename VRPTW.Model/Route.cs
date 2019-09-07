using System.Collections.Generic;

namespace VRPTW.Model
{
    public class Route
    {
        public List<Customer> Customers { get; set; }
        public double Load { get; set; }
        public double Distance { get; set; }
    }
}
