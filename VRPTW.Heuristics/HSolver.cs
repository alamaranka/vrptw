using System;
using System.Collections.Generic;
using System.Text;
using VRPTW.Model;

namespace VRPTW.Heuristics
{
    public class HSolver
    {
        private Dataset _dataset;

        public HSolver(Dataset dataset)
        {
            _dataset = dataset;
        }

        public Solution Run()
        {
            var solution = new InitialSolution(_dataset).Get();
            new TwoOptOperator(solution);
            new SwapOperator(solution);
            return solution;
        }
    }
}
