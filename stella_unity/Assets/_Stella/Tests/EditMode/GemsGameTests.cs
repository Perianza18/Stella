using System;
using NUnit.Framework;

namespace Stella.Level05.Tests
{
    public sealed class GemsGameTests
    {
        [Test]
        public void NewGameStartsWithTwoHundredGems()
        {
            GemsGame game = new GemsGame();

            Assert.That(game.RemainingGems, Is.EqualTo(200));
            Assert.That(game.CurrentParticipant, Is.EqualTo(Participant.Stella));
            Assert.That(game.IsGameOver, Is.False);
            Assert.That(game.Winner, Is.Null);
        }

        [Test]
        public void TurnLimitIsHalfRoundedDownExceptWhenOneGemRemains()
        {
            Assert.That(GemsGame.TurnLimit(200), Is.EqualTo(100));
            Assert.That(GemsGame.TurnLimit(7), Is.EqualTo(3));
            Assert.That(GemsGame.TurnLimit(2), Is.EqualTo(1));
            Assert.That(GemsGame.TurnLimit(1), Is.EqualTo(1));
        }

        [Test]
        public void LosingPositionsThroughTwoHundredMatchTheKnownSequence()
        {
            bool[] losing = GemsGame.CalculateLosingPositions(200);

            int[] cursedNumbers = { 2, 5, 11, 23, 47, 95, 191 };
            for (int n = 1; n <= 200; n++)
            {
                bool expected = Array.IndexOf(cursedNumbers, n) >= 0;
                Assert.That(losing[n], Is.EqualTo(expected), "Mismatch at " + n);
            }
        }

        [Test]
        public void OptimalMoveFromTwoHundredLeavesOneHundredNinetyOne()
        {
            GemsGame game = new GemsGame();

            int move = game.OptimalMove(game.RemainingGems);

            Assert.That(move, Is.EqualTo(9));
        }

        [Test]
        public void OptimalMoveAlwaysReturnsTheOpponentToALosingPositionWhenPossible()
        {
            GemsGame game = new GemsGame();

            for (int remaining = 1; remaining <= 200; remaining++)
            {
                if (game.IsLosingPosition(remaining)) continue;

                int move = game.OptimalMove(remaining);
                Assert.That(move, Is.GreaterThanOrEqualTo(1));
                Assert.That(move, Is.LessThanOrEqualTo(GemsGame.TurnLimit(remaining)));

                int afterMove = remaining - move;
                Assert.That(afterMove == 0 || game.IsLosingPosition(afterMove), Is.True, "Failed at " + remaining);
            }
        }

        [Test]
        public void ApplyMoveRejectsAmountsOutsideTheLegalRange()
        {
            GemsGame game = new GemsGame(7, Participant.Stella);

            Assert.Throws<ArgumentOutOfRangeException>(() => game.ApplyMove(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => game.ApplyMove(4));
        }

        [Test]
        public void TakingTheLastGemEndsTheGameAndRecordsTheWinner()
        {
            GemsGame game = new GemsGame(1, Participant.Guardian);

            game.ApplyMove(1);

            Assert.That(game.IsGameOver, Is.True);
            Assert.That(game.Winner, Is.EqualTo(Participant.Guardian));
        }

        [Test]
        public void ApplyMoveAlternatesTheCurrentParticipant()
        {
            GemsGame game = new GemsGame(10, Participant.Stella);

            game.ApplyMove(1);

            Assert.That(game.CurrentParticipant, Is.EqualTo(Participant.Guardian));
        }

        [Test]
        public void ApplyMoveAfterGameOverThrows()
        {
            GemsGame game = new GemsGame(1, Participant.Stella);
            game.ApplyMove(1);

            Assert.Throws<InvalidOperationException>(() => game.ApplyMove(1));
        }
    }
}
