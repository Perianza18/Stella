using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Stella.Level02.Tests
{
    public sealed class RuinsBoardGameTests
    {
        private static readonly BoardCoordinate[] LegalL = Cells((0, 0), (0, 1), (0, 2), (1, 0));

        [Test]
        public void NewBoardIsSevenBySevenWithOnlyCentreBlocked()
        {
            RuinsBoardGame game = new RuinsBoardGame();
            int freeCount = 0;

            for (int row = 0; row < RuinsBoardGame.BoardSize; row++)
            {
                for (int column = 0; column < RuinsBoardGame.BoardSize; column++)
                {
                    RuinsCellState state = game.GetCellState(row, column);
                    if (row == 3 && column == 3)
                    {
                        Assert.That(state, Is.EqualTo(RuinsCellState.Blocked));
                    }
                    else
                    {
                        Assert.That(state, Is.EqualTo(RuinsCellState.Free));
                        freeCount++;
                    }
                }
            }

            Assert.That(freeCount, Is.EqualTo(48));
        }

        [Test]
        public void ExactlyEightPythonPrototypeOrientationsAreLegal()
        {
            BoardCoordinate[][] expected =
            {
                Cells((0, 0), (0, 1), (0, 2), (1, 0)),
                Cells((0, 0), (0, 1), (0, 2), (1, 2)),
                Cells((0, 0), (0, 1), (1, 0), (2, 0)),
                Cells((0, 0), (0, 1), (1, 1), (2, 1)),
                Cells((0, 0), (1, 0), (1, 1), (1, 2)),
                Cells((0, 0), (1, 0), (2, 0), (2, 1)),
                Cells((0, 1), (1, 1), (2, 0), (2, 1)),
                Cells((0, 2), (1, 0), (1, 1), (1, 2))
            };

            Assert.That(RuinsBoardGame.OrientationCount, Is.EqualTo(8));
            RuinsBoardGame game = new RuinsBoardGame();
            foreach (BoardCoordinate[] orientation in expected)
            {
                Assert.That(game.ValidatePlacement(orientation), Is.EqualTo(PlacementValidation.Valid));
            }
        }

        [Test]
        public void PlacementValidationRejectsMalformedCountsDuplicatesBoundsCentreAndOverlap()
        {
            RuinsBoardGame game = new RuinsBoardGame();

            Assert.That(game.ValidatePlacement(LegalL), Is.EqualTo(PlacementValidation.Valid));
            Assert.That(game.ValidatePlacement(Cells((0, 0), (0, 1), (1, 0), (1, 1))), Is.EqualTo(PlacementValidation.NotLTetromino));
            Assert.That(game.ValidatePlacement(Cells((0, 0), (0, 1), (0, 2))), Is.EqualTo(PlacementValidation.WrongCellCount));
            Assert.That(game.ValidatePlacement(Cells((0, 0), (0, 1), (0, 2), (1, 0), (1, 1))), Is.EqualTo(PlacementValidation.WrongCellCount));
            Assert.That(game.ValidatePlacement(Cells((0, 0), (0, 0), (0, 1), (1, 0))), Is.EqualTo(PlacementValidation.DuplicateCells));
            Assert.That(game.ValidatePlacement(Cells((-1, 0), (0, 0), (1, 0), (1, 1))), Is.EqualTo(PlacementValidation.OutOfBounds));
            Assert.That(game.ValidatePlacement(Cells((2, 3), (3, 3), (4, 3), (4, 4))), Is.EqualTo(PlacementValidation.Occupied));

            game.ApplyPlacement(LegalL);
            Assert.That(game.ValidatePlacement(LegalL), Is.EqualTo(PlacementValidation.Occupied));
        }

        [Test]
        public void ApplyingMoveClaimsAllFourCellsAndAlternatesTurn()
        {
            RuinsBoardGame game = new RuinsBoardGame(RuinsParticipant.Stella, new Random(3));

            game.ApplyPlacement(LegalL);

            Assert.That(LegalL.All(cell => game.GetCellState(cell.Row, cell.Column) == RuinsCellState.Occupied), Is.True);
            Assert.That(game.CurrentParticipant, Is.EqualTo(RuinsParticipant.Alien));
            Assert.Throws<ArgumentException>(() => game.ApplyPlacement(LegalL));
        }

        [Test]
        public void RotationalMirrorMatchesFormulaAndIsDisjoint()
        {
            IReadOnlyList<BoardCoordinate> mirrored = RuinsBoardGame.Mirror(LegalL);
            HashSet<BoardCoordinate> expected = new HashSet<BoardCoordinate>(
                LegalL.Select(cell => new BoardCoordinate(6 - cell.Row, 6 - cell.Column)));

            CollectionAssert.AreEquivalent(expected, mirrored);
            Assert.That(new HashSet<BoardCoordinate>(LegalL).Overlaps(mirrored), Is.False);
        }

        [Test]
        public void SecondPlayerKeeperChoosesAProvenLegalResponse()
        {
            RuinsBoardGame game = new RuinsBoardGame(RuinsParticipant.Stella, new Random(7));
            game.ApplyPlacement(LegalL);

            IReadOnlyList<BoardCoordinate> move = game.ChooseAlienMove();

            CollectionAssert.AreEquivalent(RuinsBoardGame.Mirror(LegalL), move);
            Assert.That(game.ValidatePlacement(move), Is.EqualTo(PlacementValidation.Valid));
        }

        [Test]
        public void FirstPlayerKeeperChoosesDeterministicLegalL()
        {
            RuinsBoardGame game = new RuinsBoardGame(RuinsParticipant.Alien, new FixedRandom(0));

            IReadOnlyList<BoardCoordinate> move = game.ChooseAlienMove();

            Assert.That(move.Count, Is.EqualTo(4));
            Assert.That(game.ValidatePlacement(move), Is.EqualTo(PlacementValidation.Valid));
        }

        [Test]
        public void LegalMoveDetectionAndTurnStartLossRecordWinner()
        {
            RuinsBoardGame game = new RuinsBoardGame(RuinsParticipant.Stella);
            Assert.That(game.HasLegalMove(), Is.True);

            game.FillAllFreeCellsForTests();
            Assert.That(game.HasLegalMove(), Is.False);
            game.ResolveTurnWithoutMoveIfNecessary();

            Assert.That(game.IsGameOver, Is.True);
            Assert.That(game.Winner, Is.EqualTo(RuinsParticipant.Alien));
        }

        [Test]
        public void ProofPresetsAreNeutralSymmetricAndLosingForKeeperToMove()
        {
            RuinsBoardSolver solver = new RuinsBoardSolver();
            Assert.That(RuinsBoardGame.ProofPresetCount, Is.GreaterThanOrEqualTo(3));

            for (int index = 0; index < RuinsBoardGame.ProofPresetCount; index++)
            {
                IReadOnlyList<BoardCoordinate> preset = RuinsBoardGame.GetProofPreset(index);
                RuinsBoardGame game = new RuinsBoardGame(
                    RuinsParticipant.Alien,
                    new Random(0),
                    preset,
                    solver);

                Assert.That(preset.Count, Is.EqualTo(16));
                Assert.That(RuinsBoardSolver.IsRotationallyBalanced(game.OccupancyMask), Is.True);
                Assert.That(game.HasLegalMove(), Is.True);
                Assert.That(solver.Solve(game.OccupancyMask).IsWinning, Is.False);
                Assert.That(preset.All(cell => game.GetCellState(cell.Row, cell.Column) == RuinsCellState.Occupied), Is.True);
            }
        }

        [Test]
        public void ExactSolverClassifiesTerminalAndSingleMovePositions()
        {
            RuinsBoardSolver solver = new RuinsBoardSolver();
            ulong full = (1UL << 49) - 1;
            Assert.That(solver.Solve(full).IsWinning, Is.False);

            ulong onlyMove = RuinsBoardGame.PlacementMasks[0];
            RuinsSolveResult result = solver.Solve(full ^ onlyMove);
            Assert.That(result.IsWinning, Is.True);
            Assert.That(result.WinningMoves.Count, Is.GreaterThan(0));
            Assert.That(result.WinningMoves.All(move => (move & (full ^ onlyMove)) == 0), Is.True);
        }

        [Test]
        public void EveryPlacementIsDisjointFromItsHalfTurnPartner()
        {
            Assert.That(
                RuinsBoardGame.PlacementMasks.All(move =>
                    (move & RuinsBoardSolver.Transform(move, 2)) == 0),
                Is.True);
        }

        [Test]
        public void KeeperPrefersNonMirrorWhenSeveralProvenWinningMovesExist()
        {
            const ulong occupied = 561010424938495UL;
            BoardCoordinate[] lastStella = Cells((1, 1), (1, 2), (1, 3), (2, 3));
            RuinsBoardSolver solver = new RuinsBoardSolver();
            ulong literalMirror = RuinsBoardSolver.Transform(RuinsBoardGame.MaskForCoordinates(lastStella), 2);

            IReadOnlyList<BoardCoordinate> chosen = solver.ChooseKeeperMove(
                occupied,
                lastStella,
                new FixedRandom(0));
            ulong chosenMask = RuinsBoardGame.MaskForCoordinates(chosen);

            Assert.That(chosenMask, Is.Not.EqualTo(literalMirror));
            Assert.That((chosenMask & occupied), Is.Zero);
            Assert.That(solver.Solve(occupied | chosenMask).IsWinning, Is.False);
        }

        [Test]
        public void KeeperMayUseMirrorWhenItIsOnlyWinningMove()
        {
            ulong full = (1UL << 49) - 1;
            ulong onlyMove = RuinsBoardGame.PlacementMasks[0];
            ulong occupied = full ^ onlyMove;
            BoardCoordinate[] lastStella = RuinsBoardGame.CoordinatesForMask(
                RuinsBoardSolver.Transform(onlyMove, 2));
            RuinsBoardSolver solver = new RuinsBoardSolver();

            ulong chosen = RuinsBoardGame.MaskForCoordinates(
                solver.ChooseKeeperMove(occupied, lastStella, new FixedRandom(0)));

            Assert.That(chosen, Is.EqualTo(onlyMove));
        }

        private static BoardCoordinate[] Cells(params (int row, int column)[] coordinates)
        {
            return coordinates.Select(cell => new BoardCoordinate(cell.row, cell.column)).ToArray();
        }

        private sealed class FixedRandom : Random
        {
            private readonly int value;
            public FixedRandom(int value) { this.value = value; }

            public override int Next(int minValue, int maxValue)
            {
                return Math.Min(Math.Max(value, minValue), maxValue - 1);
            }
        }
    }
}
