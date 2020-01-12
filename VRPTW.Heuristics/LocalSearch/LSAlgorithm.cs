using VRPTW.Heuristics.LocalSearch;
using VRPTW.Model;

namespace VRPTW.Heuristics
{
    public class LSAlgorithm
    {
        private readonly Dataset _dataset;

        public LSAlgorithm(Dataset dataset)
        {
            _dataset = dataset;
        }

        public Solution Run()
        {
            var solution = new InitialSolution(_dataset).Run();
            var improved = true;

            while (improved)
            {
                improved = false;
                var cost = solution.Cost;

                solution = new TwoOptOperator(solution).Apply2OptOperator();
                solution = new ExchangeOperator(solution).ApplyExchangeOperator();
                solution = new RelocateOperator(solution).ApplyRelocateOperator();
                solution = new CrossOperator(solution).ApplyCrossOperator();

                improved |= solution.Cost < cost;
            }

            return solution;
        }
    }
}
