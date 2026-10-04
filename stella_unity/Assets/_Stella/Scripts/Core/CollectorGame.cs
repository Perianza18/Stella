using System;

namespace Stella.Level03
{
    public enum Participant
    {
        Stella,
        Collector
    }

    /// <summary>
    /// Owns the rules and turn state for the Collector's two-coordinate duel.
    /// This class intentionally has no Unity dependencies, so its behavior is easy to test.
    /// </summary>
    public sealed class CollectorGame
    {
        public const int StartingRow = 7;
        public const int StartingColumn = 10;

        private readonly Random random;

        public int Row { get; private set; }
        public int Column { get; private set; }
        public Participant CurrentParticipant { get; private set; }
        public Participant? Winner { get; private set; }
        public bool IsGameOver
        {
            get { return Row == 0 && Column == 0; }
        }

        public CollectorGame(Random randomSource = null)
            : this(StartingRow, StartingColumn, Participant.Stella, randomSource)
        {
        }

        public CollectorGame(int row, int column, Participant startingParticipant, Random randomSource = null)
        {
            if (row < 0 || column < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(row), "Coordinates cannot be negative.");
            }

            Row = row;
            Column = column;
            CurrentParticipant = startingParticipant;
            Winner = null;
            random = randomSource ?? new Random();
        }

        public static bool IsLosingPosition(int row, int column)
        {
            return row == column;
        }

        /// <summary>
        /// Returns the Collector's next position. Off the diagonal, it always moves onto
        /// the diagonal by reducing the larger coordinate to match the smaller one. On the
        /// diagonal, no winning move exists, so it reduces a random coordinate instead.
        /// </summary>
        public (int Row, int Column) OptimalMove(int row, int column)
        {
            if (column > row) return (row, row);
            if (row > column) return (column, column);

            if (random.Next(2) == 0) return (random.Next(0, row), column);
            return (row, random.Next(0, column));
        }

        public void ApplyMove(int newRow, int newColumn)
        {
            if (IsGameOver)
            {
                throw new InvalidOperationException("The duel has already ended.");
            }

            if (newRow < 0 || newColumn < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newRow), "Coordinates cannot be negative.");
            }

            if (newRow > Row || newColumn > Column)
            {
                throw new ArgumentException("A move can only reduce a coordinate, never increase it.");
            }

            bool rowChanged = newRow != Row;
            bool columnChanged = newColumn != Column;
            if (rowChanged == columnChanged)
            {
                throw new ArgumentException("A move must change exactly one coordinate.");
            }

            Row = newRow;
            Column = newColumn;

            if (IsGameOver)
            {
                Winner = CurrentParticipant;
                return;
            }

            CurrentParticipant = CurrentParticipant == Participant.Stella
                ? Participant.Collector
                : Participant.Stella;
        }
    }
}
