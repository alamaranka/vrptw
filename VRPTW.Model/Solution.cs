using System;
using System.Collections.Generic;

namespace VRPTW.Data
{
    [Serializable]
    public class Solution
    {
        public List<Route> Routes { get; set; }
        public double Cost { get; set; }
    }
}
