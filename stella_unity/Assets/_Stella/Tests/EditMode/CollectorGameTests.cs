using System;
using NUnit.Framework;

namespace Stella.Level03.Tests
{
    public sealed class CollectorGameTests
    {
        [Test]
        public void NewGameStartsAtSevenTen()
        {
            CollectorGame game = new CollectorGame();

            Assert.That(game.Row, Is.EqualTo(7));
            Assert.That(game.Column, Is.EqualTo(10));
            Assert.That(game.IsGameOver, Is.False);
            Assert.That(game.Winner, Is.Null);
        }

        [Test]
        public void DiagonalPositionsAreLosingForThePlayerToMove()
        {
            Assert.That(CollectorGame.IsLosingPosition(0, 0), Is.True);
            Assert.That(CollectorGame.IsLosingPosition(5, 5), Is.True);
            Assert.That(CollectorGame.IsLosingPosition(5, 6), Is.False);
        }

        [Test]
        public void OptimalMoveFromOffDiagonalRestoresTheDiagonal()
        {
            CollectorGame game = new CollectorGame();

            Assert.That(game.OptimalMove(7, 10), Is.EqualTo((7, 7)));
            Assert.That(game.OptimalMove(9, 4), Is.EqualTo((4, 4)));
        }

        [Test]
        public void OptimalMoveFromTheDiagonalReducesOneCoordinateAndStaysLegal()
        {
            CollectorGame game = new CollectorGame(new Random(0));

            for (int trial = 0; trial < 50; trial++)
            {
                (int row, int column) = game.OptimalMove(5, 5);
                bool rowReduced = row < 5 && column == 5;
                bool columnReduced = column < 5 && row == 5;
                Assert.That(rowReduced || columnReduced, Is.True);
                Assert.That(row, Is.GreaterThanOrEqualTo(0));
                Assert.That(column, Is.GreaterThanOrEqualTo(0));
            }
        }

        [Test]
        public void ApplyMoveRejectsChangingBothCoordinatesAtOnce()
        {
            CollectorGame game = new CollectorGame(3, 3, Participant.Stella);

            Assert.Throws<ArgumentException>(() => game.ApplyMove(2, 2));
        }

        [Test]
        public void ApplyMoveRejectsIncreasingACoordinate()
        {
            CollectorGame game = new CollectorGame(3, 3, Participant.Stella);

            Assert.Throws<ArgumentException>(() => game.ApplyMove(3, 4));
        }

        [Test]
        public void ApplyMoveRejectsNoChangeAtAll()
        {
            CollectorGame game = new CollectorGame(3, 3, Participant.Stella);

            Assert.Throws<ArgumentException>(() => game.ApplyMove(3, 3));
        }

        [Test]
        public void LandingOnOriginEndsTheGameAndRecordsTheWinner()
        {
            CollectorGame game = new CollectorGame(0, 1, Participant.Collector);

            game.ApplyMove(0, 0);

            Assert.That(game.IsGameOver, Is.True);
            Assert.That(game.Winner, Is.EqualTo(Participant.Collector));
        }

        [Test]
        public void ApplyMoveAlternatesTheCurrentParticipant()
        {
            CollectorGame game = new CollectorGame(5, 5, Participant.Stella);

            game.ApplyMove(2, 5);

            Assert.That(game.CurrentParticipant, Is.EqualTo(Participant.Collector));
        }

        [Test]
        public void ApplyMoveAfterGameOverThrows()
        {
            CollectorGame game = new CollectorGame(0, 1, Participant.Stella);
            game.ApplyMove(0, 0);

            Assert.Throws<InvalidOperationException>(() => game.ApplyMove(0, 0));
        }
    }
}
