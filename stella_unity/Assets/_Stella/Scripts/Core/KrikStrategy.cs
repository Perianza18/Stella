using System;

namespace Stella.Level04
{
    /// <summary>
    /// Chooses the Krik's move without knowing anything about scene objects or UI.
    /// </summary>
    public sealed class KrikStrategy
    {
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

            // Every move from a multiple of four can be answered perfectly.
            // Removing one is a predictable fallback that makes tests and replays stable.
            return KrikGreetingGame.MinimumMove;
        }
    }
}
