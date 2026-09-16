using System;

namespace Stella.Level04
{
    public enum Participant
    {
        Stella,
        Krik
    }

    /// <summary>
    /// Owns the rules and turn state for the Krik Greeting subtraction game.
    /// This class intentionally has no Unity dependencies, so its behavior is easy to test.
    /// </summary>
    public sealed class KrikGreetingGame
    {
        public const int StartingCrystalCount = 20;
        public const int MinimumMove = 1;
        public const int MaximumMove = 3;

        public int RemainingCrystals { get; private set; }
        public Participant CurrentParticipant { get; private set; }
        public Participant? Winner { get; private set; }
        public bool IsGameOver
        {
            get { return RemainingCrystals == 0; }
        }

        public KrikGreetingGame(Participant startingParticipant = Participant.Stella)
            : this(StartingCrystalCount, startingParticipant)
        {
        }

        public KrikGreetingGame(int startingCrystals, Participant startingParticipant)
        {
            if (startingCrystals < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(startingCrystals),
                    "The crystal count cannot be negative.");
            }

            RemainingCrystals = startingCrystals;
            CurrentParticipant = startingParticipant;
            Winner = null;
        }

        public int[] GetLegalMoves()
        {
            int legalMoveCount = Math.Min(MaximumMove, RemainingCrystals);
            int[] legalMoves = new int[legalMoveCount];

            for (int index = 0; index < legalMoveCount; index++)
            {
                legalMoves[index] = index + MinimumMove;
            }

            return legalMoves;
        }

        public void ApplyMove(int crystalCount)
        {
            if (IsGameOver)
            {
                throw new InvalidOperationException("The greeting has already ended.");
            }

            if (crystalCount < MinimumMove || crystalCount > MaximumMove)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(crystalCount),
                    "A turn must extinguish one, two, or three crystals.");
            }

            if (crystalCount > RemainingCrystals)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(crystalCount),
                    "A turn cannot extinguish more crystals than remain.");
            }

            RemainingCrystals -= crystalCount;

            if (IsGameOver)
            {
                Winner = CurrentParticipant;
                return;
            }

            CurrentParticipant = CurrentParticipant == Participant.Stella
                ? Participant.Krik
                : Participant.Stella;
        }
    }
}
