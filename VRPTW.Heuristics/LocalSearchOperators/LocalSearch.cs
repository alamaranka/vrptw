
using VRPTW.Model;

namespace VRPTW.Heuristics
{
    public class LocalSearch
    {
        private readonly Dataset _dataset;

        public LocalSearch(Dataset dataset)
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

                solution = new CrossOperator(solution).ApplyCrossOperator();
                solution = new ExchangeOperator(solution).ApplyExchangeOperator();
                solution = new TwoOptOperator(solution).Apply2OptOperator();
                solution = new RelocateOperator(solution).ApplyRelocateOperator();

                improved |= solution.Cost < cost;
            }

            return solution;
        }
    }
}
