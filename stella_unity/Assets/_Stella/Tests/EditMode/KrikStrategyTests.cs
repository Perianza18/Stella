using System;
using NUnit.Framework;

namespace Stella.Level04.Tests
{
    public sealed class KrikStrategyTests
    {
        private KrikStrategy strategy;

        [SetUp]
        public void SetUp()
        {
            strategy = new KrikStrategy();
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
        public void LosingPositionsUseDeterministicOneCrystalFallback(int remaining)
        {
            Assert.That(strategy.ChooseMove(remaining), Is.EqualTo(1));
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
