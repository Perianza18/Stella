using System;
using System.Collections.Generic;
using System.Linq;

namespace Stella.Level02
{
    public sealed class RuinsSolveResult
    {
        public RuinsSolveResult(bool isWinning, int? distance, IReadOnlyList<ulong> winningMoves)
        { IsWinning = isWinning; Distance = distance; WinningMoves = winningMoves; }
        public bool IsWinning { get; }
        public int? Distance { get; }
        public IReadOnlyList<ulong> WinningMoves { get; }
    }

    /// <summary>Pure bitboard outcome solver and deterministic Keeper policy.</summary>
    public sealed class RuinsBoardSolver
    {
        public const int ExactFreeCellLimit = 28;
        private readonly Dictionary<ulong, RuinsSolveResult> cache = new Dictionary<ulong, RuinsSolveResult>();

        public int CachedStateCount => cache.Count;

        public RuinsSolveResult Solve(ulong occupied)
        {
            ulong canonical = Canonicalize(occupied | RuinsBoardGame.CentreMask, out int transform);
            RuinsSolveResult result = SolveCanonical(canonical);
            if (result.WinningMoves.Count == 0) return result;
            int[] inverse = { 0, 3, 2, 1, 4, 5, 6, 7 };
            ulong[] restored = result.WinningMoves.Select(move => Transform(move, inverse[transform])).ToArray();
            return new RuinsSolveResult(result.IsWinning, result.Distance, restored);
        }

        public IReadOnlyList<BoardCoordinate> ChooseKeeperMove(
            ulong occupied,
            IReadOnlyList<BoardCoordinate> lastStellaMove,
            Random random)
        {
            List<ulong> legal = LegalMoves(occupied);
            if (legal.Count == 0) throw new InvalidOperationException("The Keeper has no legal placement.");

            ulong mirrorMask = lastStellaMove == null ? 0 : Transform(RuinsBoardGame.MaskForCoordinates(lastStellaMove), 2);
            List<ulong> winning = legal.Where(move => IsRotationallyBalanced(occupied | move)).ToList();
            int freeCells = RuinsBoardGame.BoardSize * RuinsBoardGame.BoardSize - CountBits(occupied);
            if (winning.Count == 0 && freeCells <= ExactFreeCellLimit)
            {
                winning = legal.Where(move => !Solve(occupied | move).IsWinning).ToList();
            }

            List<ulong> candidates = winning.Count > 0 ? winning : legal;
            if (winning.Count > 0)
            {
                List<ulong> nonMirror = candidates.Where(move => move != mirrorMask).ToList();
                if (nonMirror.Count > 0) candidates = nonMirror;
            }
            else if (IsRotationallyBalanced(occupied))
            {
                int best = candidates.Max(move => LegalMoves(occupied | move | Transform(move, 2)).Count);
                candidates = candidates.Where(move => LegalMoves(occupied | move | Transform(move, 2)).Count == best).ToList();
            }
            else
            {
                int best = candidates.Min(move => LegalMoves(occupied | move).Count);
                candidates = candidates.Where(move => LegalMoves(occupied | move).Count == best).ToList();
            }

            return RuinsBoardGame.CoordinatesForMask(candidates[random.Next(0, candidates.Count)]);
        }

        public static bool IsRotationallyBalanced(ulong mask) => Transform(mask, 2) == mask;

        public static ulong Transform(ulong mask, int transform)
        {
            ulong result = 0;
            int size = RuinsBoardGame.BoardSize, last = size - 1;
            for (int index = 0; index < size * size; index++)
            {
                if ((mask & (1UL << index)) == 0) continue;
                int row = index / size, column = index % size, targetRow, targetColumn;
                switch (transform)
                {
                    case 1: targetRow = column; targetColumn = last - row; break;
                    case 2: targetRow = last - row; targetColumn = last - column; break;
                    case 3: targetRow = last - column; targetColumn = row; break;
                    case 4: targetRow = row; targetColumn = last - column; break;
                    case 5: targetRow = last - row; targetColumn = column; break;
                    case 6: targetRow = column; targetColumn = row; break;
                    case 7: targetRow = last - column; targetColumn = last - row; break;
                    default: targetRow = row; targetColumn = column; break;
                }
                result |= 1UL << (targetRow * size + targetColumn);
            }
            return result;
        }

        private RuinsSolveResult SolveCanonical(ulong occupied)
        {
            if (cache.TryGetValue(occupied, out RuinsSolveResult stored)) return stored;
            if (IsRotationallyBalanced(occupied)) return Store(occupied, new RuinsSolveResult(false, null, Array.Empty<ulong>()));

            List<ulong> legal = LegalMoves(occupied);
            if (legal.Count == 0) return Store(occupied, new RuinsSolveResult(false, 0, Array.Empty<ulong>()));
            List<ulong> symmetryMoves = legal.Where(move => IsRotationallyBalanced(occupied | move)).ToList();
            if (symmetryMoves.Count > 0) return Store(occupied, new RuinsSolveResult(true, null, symmetryMoves));

            List<ulong> winningMoves = new List<ulong>();
            List<int> losingDistances = new List<int>();
            List<int> childDistances = new List<int>();
            bool everyDistanceKnown = true;
            foreach (ulong move in legal)
            {
                RuinsSolveResult child = SolveCanonical(Canonicalize(occupied | move));
                if (!child.IsWinning)
                {
                    winningMoves.Add(move);
                    if (child.Distance.HasValue) losingDistances.Add(child.Distance.Value);
                }
                if (child.Distance.HasValue) childDistances.Add(child.Distance.Value); else everyDistanceKnown = false;
            }
            if (winningMoves.Count > 0)
            {
                int? distance = losingDistances.Count > 0 ? 1 + losingDistances.Min() : (int?)null;
                return Store(occupied, new RuinsSolveResult(true, distance, winningMoves));
            }
            int? losingDistance = everyDistanceKnown ? 1 + childDistances.Max() : (int?)null;
            return Store(occupied, new RuinsSolveResult(false, losingDistance, Array.Empty<ulong>()));
        }

        private static List<ulong> LegalMoves(ulong occupied) =>
            RuinsBoardGame.PlacementMasks.Where(move => (move & occupied) == 0).ToList();

        private static ulong Canonicalize(ulong mask)
        {
            return Canonicalize(mask, out _);
        }

        private static ulong Canonicalize(ulong mask, out int selectedTransform)
        {
            ulong canonical = ulong.MaxValue;
            selectedTransform = 0;
            for (int transform = 0; transform < 8; transform++)
            {
                ulong candidate = Transform(mask, transform);
                if (candidate < canonical) { canonical = candidate; selectedTransform = transform; }
            }
            return canonical;
        }

        private RuinsSolveResult Store(ulong key, RuinsSolveResult result) { cache[key] = result; return result; }
        private static int CountBits(ulong value)
        {
            int count = 0;
            while (value != 0) { value &= value - 1; count++; }
            return count;
        }
    }
}
