using System;
using System.Collections.Generic;
using System.Linq;

namespace Stella.Level02
{
    public enum RuinsParticipant { Stella, Alien }
    public enum RuinsCellState { Free, Blocked, Occupied }
    public enum PlacementValidation { Valid, WrongCellCount, DuplicateCells, OutOfBounds, Occupied, NotLTetromino }

    public readonly struct BoardCoordinate : IEquatable<BoardCoordinate>, IComparable<BoardCoordinate>
    {
        public BoardCoordinate(int row, int column) { Row = row; Column = column; }
        public int Row { get; }
        public int Column { get; }
        public int CompareTo(BoardCoordinate other)
        {
            int row = Row.CompareTo(other.Row);
            return row != 0 ? row : Column.CompareTo(other.Column);
        }
        public bool Equals(BoardCoordinate other) => Row == other.Row && Column == other.Column;
        public override bool Equals(object obj) => obj is BoardCoordinate other && Equals(other);
        public override int GetHashCode() { unchecked { return (Row * 397) ^ Column; } }
        public override string ToString() => "(" + Row + "," + Column + ")";
    }

    /// <summary>Pure rules and strategic state for one ruins-board match.</summary>
    public sealed class RuinsBoardGame
    {
        public const int BoardSize = 7;
        public const int CentreIndex = 3;
        public const int CellsPerPiece = 4;

        private static readonly BoardCoordinate[][] LegalOrientations =
        {
            Shape((0, 0), (0, 1), (0, 2), (1, 0)), Shape((0, 0), (0, 1), (0, 2), (1, 2)),
            Shape((0, 0), (0, 1), (1, 0), (2, 0)), Shape((0, 0), (0, 1), (1, 1), (2, 1)),
            Shape((0, 0), (1, 0), (1, 1), (1, 2)), Shape((0, 0), (1, 0), (2, 0), (2, 1)),
            Shape((0, 1), (1, 1), (2, 0), (2, 1)), Shape((0, 2), (1, 0), (1, 1), (1, 2))
        };

        private static readonly BoardCoordinate[][] ProofPresetData =
        {
            Shape((0,0),(0,1),(0,2),(1,6),(2,3),(2,4),(3,0),(3,2),(3,4),(3,6),(4,2),(4,3),(5,0),(6,4),(6,5),(6,6)),
            Shape((0,0),(0,6),(1,0),(1,6),(2,2),(2,3),(2,5),(3,2),(3,4),(4,1),(4,3),(4,4),(5,0),(5,6),(6,0),(6,6)),
            Shape((0,2),(0,3),(0,4),(0,6),(1,1),(1,3),(1,5),(2,5),(4,1),(5,1),(5,3),(5,5),(6,0),(6,2),(6,3),(6,4))
        };

        private readonly RuinsCellState[,] board = new RuinsCellState[BoardSize, BoardSize];
        private readonly Random random;
        private readonly RuinsBoardSolver solver;
        private List<BoardCoordinate> lastStellaMove;

        static RuinsBoardGame()
        {
            List<ulong> masks = new List<ulong>();
            foreach (BoardCoordinate[] orientation in LegalOrientations)
            {
                int height = orientation.Max(cell => cell.Row) + 1;
                int width = orientation.Max(cell => cell.Column) + 1;
                for (int row = 0; row <= BoardSize - height; row++)
                {
                    for (int column = 0; column <= BoardSize - width; column++)
                    {
                        ulong mask = MaskForCoordinates(Offset(orientation, row, column));
                        if ((mask & CentreMask) == 0) masks.Add(mask);
                    }
                }
            }
            PlacementMasks = masks.Distinct().OrderBy(mask => mask).ToArray();
        }

        public RuinsBoardGame(RuinsParticipant starter = RuinsParticipant.Stella)
            : this(starter, new Random(), null, null) { }

        public RuinsBoardGame(RuinsParticipant starter, Random randomSource)
            : this(starter, randomSource, null, null) { }

        public RuinsBoardGame(
            RuinsParticipant starter,
            Random randomSource,
            IEnumerable<BoardCoordinate> preset,
            RuinsBoardSolver positionSolver = null)
        {
            random = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
            solver = positionSolver ?? new RuinsBoardSolver();
            StartingParticipant = starter;
            CurrentParticipant = starter;
            board[CentreIndex, CentreIndex] = RuinsCellState.Blocked;
            if (preset != null)
            {
                foreach (BoardCoordinate cell in preset)
                {
                    if (!IsInBounds(cell.Row, cell.Column) || cell.Row == CentreIndex && cell.Column == CentreIndex)
                        throw new ArgumentException("Proof preset contains an invalid cell.", nameof(preset));
                    board[cell.Row, cell.Column] = RuinsCellState.Occupied;
                }
            }
            ResolveTurnWithoutMoveIfNecessary();
        }

        public RuinsParticipant StartingParticipant { get; }
        public RuinsParticipant CurrentParticipant { get; private set; }
        public RuinsParticipant? Winner { get; private set; }
        public bool IsGameOver { get; private set; }
        public IReadOnlyList<BoardCoordinate> LastPlacedCells { get; private set; }
        public RuinsParticipant? LastParticipant { get; private set; }
        public static int OrientationCount => LegalOrientations.Length;
        public static int ProofPresetCount => ProofPresetData.Length;
        internal static ulong CentreMask => 1UL << (CentreIndex * BoardSize + CentreIndex);
        internal static IReadOnlyList<ulong> PlacementMasks { get; }

        public static IReadOnlyList<BoardCoordinate> GetProofPreset(int index)
        {
            if (index < 0 || index >= ProofPresetData.Length) throw new ArgumentOutOfRangeException(nameof(index));
            return ProofPresetData[index].ToArray();
        }

        public RuinsCellState GetCellState(int row, int column)
        {
            EnsureInBounds(row, column);
            return board[row, column];
        }

        public PlacementValidation ValidatePlacement(IEnumerable<BoardCoordinate> cells)
        {
            if (cells == null) return PlacementValidation.WrongCellCount;
            List<BoardCoordinate> footprint = cells.ToList();
            if (footprint.Count != CellsPerPiece) return PlacementValidation.WrongCellCount;
            if (new HashSet<BoardCoordinate>(footprint).Count != CellsPerPiece) return PlacementValidation.DuplicateCells;
            foreach (BoardCoordinate cell in footprint)
            {
                if (!IsInBounds(cell.Row, cell.Column)) return PlacementValidation.OutOfBounds;
                if (board[cell.Row, cell.Column] != RuinsCellState.Free) return PlacementValidation.Occupied;
            }
            BoardCoordinate[] normalized = Normalize(footprint);
            return LegalOrientations.Any(orientation => orientation.SequenceEqual(normalized))
                ? PlacementValidation.Valid : PlacementValidation.NotLTetromino;
        }

        public void ApplyPlacement(IEnumerable<BoardCoordinate> cells)
        {
            if (IsGameOver) throw new InvalidOperationException("The ruins game has already ended.");
            List<BoardCoordinate> footprint = cells == null ? null : cells.ToList();
            PlacementValidation validation = ValidatePlacement(footprint);
            if (validation != PlacementValidation.Valid)
                throw new ArgumentException("The placement is not legal: " + validation, nameof(cells));

            RuinsParticipant participant = CurrentParticipant;
            foreach (BoardCoordinate cell in footprint) board[cell.Row, cell.Column] = RuinsCellState.Occupied;
            LastPlacedCells = footprint.ToArray();
            LastParticipant = participant;
            if (participant == RuinsParticipant.Stella) lastStellaMove = footprint.ToList();
            CurrentParticipant = Other(CurrentParticipant);
            ResolveTurnWithoutMoveIfNecessary();
        }

        public IReadOnlyList<BoardCoordinate> ChooseAlienMove()
        {
            if (IsGameOver || CurrentParticipant != RuinsParticipant.Alien)
                throw new InvalidOperationException("It is not an active Keeper turn.");
            return solver.ChooseKeeperMove(OccupancyMask, lastStellaMove, random);
        }

        public bool HasLegalMove() => PlacementMasks.Any(mask => (mask & OccupancyMask) == 0);

        public List<IReadOnlyList<BoardCoordinate>> GetLegalMoves()
        {
            ulong occupied = OccupancyMask;
            return PlacementMasks.Where(mask => (mask & occupied) == 0)
                .Select(mask => (IReadOnlyList<BoardCoordinate>)CoordinatesForMask(mask)).ToList();
        }

        public static IReadOnlyList<BoardCoordinate> Mirror(IEnumerable<BoardCoordinate> cells)
        {
            if (cells == null) throw new ArgumentNullException(nameof(cells));
            return cells.Select(cell => new BoardCoordinate(BoardSize - 1 - cell.Row, BoardSize - 1 - cell.Column)).ToArray();
        }

        internal ulong OccupancyMask
        {
            get
            {
                ulong mask = 0;
                for (int row = 0; row < BoardSize; row++)
                    for (int column = 0; column < BoardSize; column++)
                        if (board[row, column] != RuinsCellState.Free) mask |= 1UL << (row * BoardSize + column);
                return mask;
            }
        }

        internal static ulong MaskForCoordinates(IEnumerable<BoardCoordinate> cells)
        {
            ulong mask = 0;
            foreach (BoardCoordinate cell in cells) mask |= 1UL << (cell.Row * BoardSize + cell.Column);
            return mask;
        }

        internal static BoardCoordinate[] CoordinatesForMask(ulong mask)
        {
            List<BoardCoordinate> cells = new List<BoardCoordinate>();
            for (int index = 0; index < BoardSize * BoardSize; index++)
                if ((mask & (1UL << index)) != 0) cells.Add(new BoardCoordinate(index / BoardSize, index % BoardSize));
            return cells.ToArray();
        }

        internal void FillAllFreeCellsForTests()
        {
            for (int row = 0; row < BoardSize; row++)
                for (int column = 0; column < BoardSize; column++)
                    if (board[row, column] == RuinsCellState.Free) board[row, column] = RuinsCellState.Occupied;
        }

        internal void ResolveTurnWithoutMoveIfNecessary()
        {
            if (IsGameOver || HasLegalMove()) return;
            Winner = Other(CurrentParticipant);
            IsGameOver = true;
        }

        internal void SetWinnerForTests(RuinsParticipant winner)
        {
            Winner = winner;
            IsGameOver = true;
        }

        private static BoardCoordinate[] Normalize(IEnumerable<BoardCoordinate> cells)
        {
            List<BoardCoordinate> values = cells.ToList();
            int row = values.Min(cell => cell.Row), column = values.Min(cell => cell.Column);
            return values.Select(cell => new BoardCoordinate(cell.Row - row, cell.Column - column)).OrderBy(cell => cell).ToArray();
        }
        private static BoardCoordinate[] Offset(BoardCoordinate[] orientation, int row, int column) =>
            orientation.Select(cell => new BoardCoordinate(cell.Row + row, cell.Column + column)).ToArray();
        private static BoardCoordinate[] Shape(params (int row, int column)[] cells) =>
            cells.Select(cell => new BoardCoordinate(cell.row, cell.column)).ToArray();
        private static RuinsParticipant Other(RuinsParticipant participant) =>
            participant == RuinsParticipant.Stella ? RuinsParticipant.Alien : RuinsParticipant.Stella;
        private static bool IsInBounds(int row, int column) => row >= 0 && row < BoardSize && column >= 0 && column < BoardSize;
        private static void EnsureInBounds(int row, int column)
        {
            if (!IsInBounds(row, column)) throw new ArgumentOutOfRangeException("Board coordinates must be between zero and six.");
        }
    }
}
