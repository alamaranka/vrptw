using Gurobi;
using System;
using System.Collections.Generic;
using System.Text;
using VRPTW.Model;

namespace VRPTW.Algorithm
{
    public interface IAlgorithm
    {
        public Solution Run(); 
    }
}
