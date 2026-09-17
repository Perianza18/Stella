using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Stella.Level04.Tests
{
    public sealed class KrikStrategyTests
    {
        private KrikStrategy strategy;

        [SetUp]
        public void SetUp()
        {
            strategy = new KrikStrategy(new Random(12345));
        }

        [Test]
        public void TwentyIsALosingPositionForTheParticipantToMove()
        {
            Assert.That(strategy.IsLosingPosition(20), Is.True);
        }

        [TestCase(19, 3)]
        [TestCase(18, 2)]
        [TestCase(17, 1)]
        public void WinningMoveLeavesSixteen(int remaining, int expectedMove)
        {
            int move = strategy.ChooseMove(remaining);

            Assert.That(move, Is.EqualTo(expectedMove));
            Assert.That(remaining - move, Is.EqualTo(16));
        }

        [TestCase(20)]
        [TestCase(16)]
        [TestCase(12)]
        [TestCase(8)]
        [TestCase(4)]
        public void LosingPositionChoicesAreAlwaysLegal(int remaining)
        {
            for (int attempt = 0; attempt < 30; attempt++)
            {
                int move = strategy.ChooseMove(remaining);

                Assert.That(move, Is.InRange(1, Math.Min(3, remaining)));
            }
        }

        [Test]
        public void SeededLosingPositionChoicesShowVariationWithoutFlakiness()
        {
            KrikStrategy seededStrategy = new KrikStrategy(new Random(90210));
            HashSet<int> observedMoves = new HashSet<int>();

            for (int attempt = 0; attempt < 30; attempt++)
            {
                observedMoves.Add(seededStrategy.ChooseMove(20));
            }

            Assert.That(observedMoves, Is.EquivalentTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void StrategyNeverReturnsAnIllegalMove()
        {
            for (int remaining = 1; remaining <= KrikGreetingGame.StartingCrystalCount; remaining++)
            {
                int move = strategy.ChooseMove(remaining);

                Assert.That(move, Is.InRange(1, 3));
                Assert.That(move, Is.LessThanOrEqualTo(remaining));
            }
        }

        [TestCase(1, 1)]
        [TestCase(2, 2)]
        [TestCase(3, 3)]
        public void SmallFinalCountsRemainLegalAndOptimal(int remaining, int expectedMove)
        {
            Assert.That(strategy.ChooseMove(remaining), Is.EqualTo(expectedMove));
        }

        [TestCase(1, 3)]
        [TestCase(2, 2)]
        [TestCase(3, 1)]
        public void OptimalResponseComplementsThePreviousMoveToFour(
            int previousMove,
            int expectedResponse)
        {
            int remainingAfterPreviousMove = 20 - previousMove;

            int response = strategy.ChooseMove(remainingAfterPreviousMove);

            Assert.That(response, Is.EqualTo(expectedResponse));
            Assert.That(previousMove + response, Is.EqualTo(4));
        }

        [Test]
        public void NoMoveExistsWhenNoCrystalsRemain()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => strategy.ChooseMove(0));
        }
    }
}
