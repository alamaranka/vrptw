using System;
using System.Collections.Generic;
using System.Text;

namespace VRPTW.Configuration
{
    class SolverParameters
    {
        public double TimeLimit { get; set; }
        public double MIPGap { get; set; }
        public int Threads { get; set; }
    }
}
