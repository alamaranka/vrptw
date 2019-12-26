using VRPTW.Heuristics.LocalSearch;
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

            var improved = true;
            while (improved)
            {
                improved = false;
                var cost = solution.Cost;
                new TwoOptOperator(solution).Apply2OptOperator();
                new ExchangeOperator(solution).ApplySwapOperator();
                new RelocateOperator(solution).ApplyRelocateOperator();
                new Diversifier(solution, 3, 7).Diverisfy();

                if (solution.Cost < cost)
                {
                    improved = true;
                }
            }

            return solution;
        }
    }
}
