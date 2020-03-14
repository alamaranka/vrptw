using Gurobi;
using System;
using System.Collections.Generic;
using System.Text;
using VRPTW.Data;

namespace VRPTW.Algorithm
{
    public interface IAlgorithm
    {
        Solution Run(); 
    }
}
