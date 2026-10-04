using System;

namespace Stella.Level05
{
    public enum Participant
    {
        Stella,
        Guardian
    }

    /// <summary>
    /// Owns the rules and turn state for the 200 Gems subtraction game.
    /// This class intentionally has no Unity dependencies, so its behavior is easy to test.
    /// </summary>
    public sealed class GemsGame
    {
        public const int StartingGems = 200;

        private readonly bool[] losingPositions;

        public int RemainingGems { get; private set; }
        public Participant CurrentParticipant { get; private set; }
        public Participant? Winner { get; private set; }
        public bool IsGameOver
        {
            get { return RemainingGems == 0; }
        }

        public GemsGame(Participant startingParticipant = Participant.Stella)
            : this(StartingGems, startingParticipant)
        {
        }

        public GemsGame(int startingGems, Participant startingParticipant)
        {
            if (startingGems < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(startingGems),
                    "The pile must start with at least one gem.");
            }

            RemainingGems = startingGems;
            CurrentParticipant = startingParticipant;
            Winner = null;
            losingPositions = CalculateLosingPositions(startingGems);
        }

        public static int TurnLimit(int remaining)
        {
            return Math.Max(1, remaining / 2);
        }

        public bool IsLosingPosition(int remaining)
        {
            return losingPositions[remaining];
        }

        public int OptimalMove(int remaining)
        {
            int limit = TurnLimit(remaining);
            for (int take = 1; take <= limit; take++)
            {
                int rest = remaining - take;
                if (rest == 0 || losingPositions[rest]) return take;
            }

            return 1; // already in a losing position; any legal move is equally bad
        }

        public void ApplyMove(int amount)
        {
            if (IsGameOver)
            {
                throw new InvalidOperationException("The game has already ended.");
            }

            int limit = TurnLimit(RemainingGems);
            if (amount < 1 || amount > limit)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    "A turn must take between 1 and half of the remaining gems.");
            }

            RemainingGems -= amount;

            if (IsGameOver)
            {
                Winner = CurrentParticipant;
                return;
            }

            CurrentParticipant = CurrentParticipant == Participant.Stella
                ? Participant.Guardian
                : Participant.Stella;
        }

        /// <summary>
        /// For each gem count from 0 to maximum, whether the player to move loses under
        /// optimal play from both sides (a "cursed number").
        /// </summary>
        public static bool[] CalculateLosingPositions(int maximum)
        {
            bool[] losing = new bool[maximum + 1];
            for (int n = 1; n <= maximum; n++)
            {
                int limit = Math.Max(1, n / 2);
                bool wins = false;
                for (int take = 1; take <= limit; take++)
                {
                    int rest = n - take;
                    if (rest == 0 || losing[rest])
                    {
                        wins = true;
                        break;
                    }
                }

                losing[n] = !wins;
            }

            return losing;
        }
    }
}
