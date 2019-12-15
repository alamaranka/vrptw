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
                new TwoOptOperator(solution);
                new ExchangeOperator(solution);

                if (solution.Cost < cost)
                {
                    improved = true;
                }
            }

            return solution;
        }
    }
}
