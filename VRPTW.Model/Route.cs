using System;
using System.Collections.Generic;
using System.Text;

namespace VRPTW.Model
{
    public class Route
    {
        public List<Customer> Customers { get; set; }
        public int Distance { get; set; }
        public int Duration { get; set; }
    }
}
