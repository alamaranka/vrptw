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
                solution = new TwoOptOperator(solution).Apply2OptOperator();
                solution = new ExchangeOperator(solution).ApplySwapOperator();
                solution = new RelocateOperator(solution).ApplyRelocateOperator();
                //solution = new Diversifier(solution, 8, 15).Diverisfy();

                if (solution.Cost < cost)
                {
                    improved = true;
                }
            }

            return solution;
        }
    }
}
