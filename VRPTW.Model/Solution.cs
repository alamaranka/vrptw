using System;
using System.Collections.Generic;

namespace VRPTW.Model
{
    [Serializable]
    public class Solution
    {
        public List<Route> Routes { get; set; }
        public double Cost { get; set; }
    }
}
