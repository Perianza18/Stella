using System;
using NUnit.Framework;

namespace Stella.Level04.Tests
{
    public sealed class KrikGreetingGameTests
    {
        [Test]
        public void NewGameStartsWithTwentyCrystals()
        {
            KrikGreetingGame game = new KrikGreetingGame();

            Assert.That(game.RemainingCrystals, Is.EqualTo(20));
            Assert.That(game.IsGameOver, Is.False);
            Assert.That(game.Winner, Is.Null);
        }

        [Test]
        public void InitialLegalMovesAreOneTwoAndThree()
        {
            KrikGreetingGame game = new KrikGreetingGame();

            Assert.That(game.GetLegalMoves(), Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [TestCase(1, 19)]
        [TestCase(2, 18)]
        [TestCase(3, 17)]
        public void LegalMovesRemoveTheRequestedCrystals(int move, int expectedRemaining)
        {
            KrikGreetingGame game = new KrikGreetingGame();

            game.ApplyMove(move);

            Assert.That(game.RemainingCrystals, Is.EqualTo(expectedRemaining));
        }

        [TestCase(0)]
        [TestCase(4)]
        [TestCase(12)]
        public void MoveOutsideOneToThreeIsIllegal(int illegalMove)
        {
            KrikGreetingGame game = new KrikGreetingGame();

            Assert.Throws<ArgumentOutOfRangeException>(() => game.ApplyMove(illegalMove));
        }

        [Test]
        public void CannotRemoveMoreCrystalsThanRemain()
        {
            KrikGreetingGame game = new KrikGreetingGame(2, Participant.Stella);

            Assert.Throws<ArgumentOutOfRangeException>(() => game.ApplyMove(3));
        }

        [Test]
        public void AValidMoveSwitchesToTheOtherParticipant()
        {
            KrikGreetingGame game = new KrikGreetingGame(Participant.Stella);

            game.ApplyMove(1);

            Assert.That(game.CurrentParticipant, Is.EqualTo(Participant.Krik));
        }

        [TestCase(Participant.Stella)]
        [TestCase(Participant.Krik)]
        public void RemovingTheFinalCrystalEndsTheGameAndRecordsWinner(Participant starter)
        {
            KrikGreetingGame game = new KrikGreetingGame(3, starter);

            game.ApplyMove(3);

            Assert.That(game.IsGameOver, Is.True);
            Assert.That(game.RemainingCrystals, Is.Zero);
            Assert.That(game.Winner, Is.EqualTo(starter));
        }

        [Test]
        public void NoMoveCanBeMadeAfterTheGreetingEnds()
        {
            KrikGreetingGame game = new KrikGreetingGame(1, Participant.Stella);
            game.ApplyMove(1);

            Assert.Throws<InvalidOperationException>(() => game.ApplyMove(1));
        }
    }
}
