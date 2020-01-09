using System;
using System.Collections.Generic;
using System.Text;

namespace VRPTW.Configuration
{
    public class DiversificationParam
    {
        public int DiversifyForEachNIteration { get; set; }
        public int MinCustomersToRemove { get; set; }
        public int MaxCustomersToRemove { get; set; }
    }
}
