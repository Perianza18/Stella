using System;

namespace Stella.Level04
{
    /// <summary>
    /// Chooses the Krik's move without knowing anything about scene objects or UI.
    /// </summary>
    public sealed class KrikStrategy
    {
        private readonly Random random;

        public KrikStrategy()
            : this(new Random())
        {
        }

        public KrikStrategy(Random randomSource)
        {
            if (randomSource == null)
            {
                throw new ArgumentNullException(nameof(randomSource));
            }

            random = randomSource;
        }

        public bool IsLosingPosition(int remainingCrystals)
        {
            if (remainingCrystals < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(remainingCrystals));
            }

            return remainingCrystals % (KrikGreetingGame.MaximumMove + 1) == 0;
        }

        public int ChooseMove(int remainingCrystals)
        {
            if (remainingCrystals <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(remainingCrystals),
                    "The Krik needs at least one crystal to take a turn.");
            }

            int winningMove = remainingCrystals % (KrikGreetingGame.MaximumMove + 1);
            int largestLegalMove = Math.Min(KrikGreetingGame.MaximumMove, remainingCrystals);

            if (winningMove >= KrikGreetingGame.MinimumMove && winningMove <= largestLegalMove)
            {
                return winningMove;
            }

            // Every legal move from a multiple of four is equally imperfect against
            // flawless play, so variety here does not weaken the Krik's strategy.
            return random.Next(KrikGreetingGame.MinimumMove, largestLegalMove + 1);
        }
    }
}
