using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Stella.Level01.Tests
{
    public sealed class DoubleLockGameTests
    {
        [Test]
        public void NewGameStartsOnLockOneWithFourColourSecret()
        {
            DoubleLockGame game = new DoubleLockGame(new SequenceRandom(0, 1, 2, 3));
            Assert.That(game.CurrentLock, Is.EqualTo(1));
            Assert.That(game.SecretCode.Count, Is.EqualTo(4));
            Assert.That(game.AttemptsUsed, Is.Zero);
        }

        [Test]
        public void LockOneSecretContainsNoDuplicates()
        {
            for (int seed = 0; seed < 20; seed++)
            {
                DoubleLockGame game = new DoubleLockGame(new System.Random(seed));
                Assert.That(new HashSet<DoubleLockColor>(game.SecretCode).Count, Is.EqualTo(4));
            }
        }

        [Test]
        public void LockTwoGenerationAllowsDuplicates()
        {
            DoubleLockGame game = new DoubleLockGame(new ZeroRandom());
            game.SetSecretForTests(DoubleLockColor.Blue, DoubleLockColor.Green, DoubleLockColor.Yellow, DoubleLockColor.Purple);
            game.Submit(new[] { DoubleLockColor.Red, DoubleLockColor.Red, DoubleLockColor.Red, DoubleLockColor.Red });
            game.Submit(new[] { DoubleLockColor.Blue, DoubleLockColor.Green, DoubleLockColor.Yellow, DoubleLockColor.Purple });

            Assert.That(game.CurrentLock, Is.EqualTo(2));
            Assert.That(new HashSet<DoubleLockColor>(game.SecretCode).Count, Is.LessThan(4));
        }

        [Test]
        public void ExactMatchesAreCounted()
        {
            DoubleLockFeedback feedback = DoubleLockGame.Score(
                new[] { DoubleLockColor.Red, DoubleLockColor.Blue, DoubleLockColor.Green, DoubleLockColor.Yellow },
                new[] { DoubleLockColor.Red, DoubleLockColor.Blue, DoubleLockColor.Green, DoubleLockColor.Yellow });
            Assert.That(feedback.Exact, Is.EqualTo(4));
            Assert.That(feedback.Misplaced, Is.Zero);
        }

        [Test]
        public void MisplacedMatchesAreCounted()
        {
            DoubleLockFeedback feedback = DoubleLockGame.Score(
                new[] { DoubleLockColor.Blue, DoubleLockColor.Green, DoubleLockColor.Yellow, DoubleLockColor.Red },
                new[] { DoubleLockColor.Red, DoubleLockColor.Blue, DoubleLockColor.Green, DoubleLockColor.Yellow });
            Assert.That(feedback.Exact, Is.Zero);
            Assert.That(feedback.Misplaced, Is.EqualTo(4));
        }

        [Test]
        public void DuplicateMatchesAreNotDoubleCounted()
        {
            DoubleLockFeedback feedback = DoubleLockGame.Score(
                new[] { DoubleLockColor.Red, DoubleLockColor.Blue, DoubleLockColor.Red, DoubleLockColor.Red },
                new[] { DoubleLockColor.Red, DoubleLockColor.Red, DoubleLockColor.Blue, DoubleLockColor.Green });
            Assert.That(feedback.Exact, Is.EqualTo(1));
            Assert.That(feedback.Misplaced, Is.EqualTo(2));
            Assert.That(feedback.Unmatched, Is.EqualTo(1));
        }

        [Test]
        public void IncorrectSubmissionConsumesExactlyOneAttempt()
        {
            DoubleLockGame game = NewFixedGame();
            DoubleLockSubmitResult result = game.Submit(WrongGuess);
            Assert.That(result.Status, Is.EqualTo(DoubleLockStatus.Playing));
            Assert.That(game.AttemptsUsed, Is.EqualTo(1));
        }

        [Test]
        public void BeginnerLuckReplacesSecretWithoutConsumingAttempt()
        {
            DoubleLockGame game = new DoubleLockGame(new SequenceRandom(0, 1, 2, 3, 1, 2, 3, 4));
            DoubleLockColor[] original = { DoubleLockColor.Red, DoubleLockColor.Blue, DoubleLockColor.Green, DoubleLockColor.Yellow };
            game.SetSecretForTests(original);
            DoubleLockSubmitResult result = game.Submit(original);

            Assert.That(result.BeginnerLuckTriggered, Is.True);
            Assert.That(game.AttemptsUsed, Is.Zero);
            Assert.That(game.BeginnerLuckUsed, Is.True);
            Assert.That(new List<DoubleLockColor>(game.SecretCode), Is.Not.EqualTo(original));
        }

        [Test]
        public void BeginnerLuckCannotTriggerTwiceOnOneLock()
        {
            DoubleLockGame game = new DoubleLockGame(new SequenceRandom(0, 1, 2, 3, 1, 2, 3, 4));
            DoubleLockColor[] original = { DoubleLockColor.Red, DoubleLockColor.Blue, DoubleLockColor.Green, DoubleLockColor.Yellow };
            game.SetSecretForTests(original);
            game.Submit(original);
            game.SetSecretForTests(original);

            DoubleLockSubmitResult result = game.Submit(original);
            Assert.That(result.Status, Is.EqualTo(DoubleLockStatus.LockAdvanced));
            Assert.That(result.BeginnerLuckTriggered, Is.False);
        }

        [Test]
        public void LaterCorrectGuessAfterBeginnerLuckCountsAsRealSuccess()
        {
            DoubleLockGame game = new DoubleLockGame(new SequenceRandom(0, 1, 2, 3, 1, 2, 3, 4));
            DoubleLockColor[] original = { DoubleLockColor.Red, DoubleLockColor.Blue, DoubleLockColor.Green, DoubleLockColor.Yellow };
            game.SetSecretForTests(original);
            game.Submit(original);
            game.SetSecretForTests(DoubleLockColor.Purple, DoubleLockColor.Orange, DoubleLockColor.Red, DoubleLockColor.Blue);

            DoubleLockSubmitResult result = game.Submit(new[] { DoubleLockColor.Purple, DoubleLockColor.Orange, DoubleLockColor.Red, DoubleLockColor.Blue });
            Assert.That(result.Status, Is.EqualTo(DoubleLockStatus.LockAdvanced));
        }

        [Test]
        public void SixIncorrectGuessesFailCurrentSequence()
        {
            DoubleLockGame game = NewFixedGame();
            DoubleLockSubmitResult result = null;
            for (int attempt = 0; attempt < DoubleLockGame.MaxAttempts; attempt++) result = game.Submit(WrongGuess);
            Assert.That(result.Failed, Is.True);
            Assert.That(game.AttemptsUsed, Is.EqualTo(6));
        }

        [Test]
        public void SolvingLockOneStartsLockTwoWithFreshAttempts()
        {
            DoubleLockGame game = NewFixedGame();
            game.Submit(WrongGuess);
            game.Submit(new[] { DoubleLockColor.Red, DoubleLockColor.Blue, DoubleLockColor.Green, DoubleLockColor.Yellow });

            Assert.That(game.CurrentLock, Is.EqualTo(2));
            Assert.That(game.AttemptsUsed, Is.Zero);
        }

        [Test]
        public void LockTwoFailureRequiresFullSequenceReset()
        {
            DoubleLockGame game = NewFixedGame();
            game.Submit(WrongGuess);
            game.Submit(new[] { DoubleLockColor.Red, DoubleLockColor.Blue, DoubleLockColor.Green, DoubleLockColor.Yellow });
            game.SetSecretForTests(DoubleLockColor.Red, DoubleLockColor.Blue, DoubleLockColor.Green, DoubleLockColor.Yellow);
            for (int attempt = 0; attempt < DoubleLockGame.MaxAttempts - 1; attempt++) game.Submit(WrongGuess);
            Assert.That(game.AttemptsUsed, Is.EqualTo(DoubleLockGame.MaxAttempts - 1));
            game.Submit(WrongGuess);

            Assert.That(game.IsFailed, Is.True);
            game.ResetSequence();
            Assert.That(game.CurrentLock, Is.EqualTo(1));
            Assert.That(game.AttemptsUsed, Is.Zero);
            Assert.That(game.BeginnerLuckUsed, Is.False);
        }

        [Test]
        public void SolvingLockTwoCompletesLevel()
        {
            DoubleLockGame game = NewFixedGame();
            game.Submit(WrongGuess);
            game.Submit(new[] { DoubleLockColor.Red, DoubleLockColor.Blue, DoubleLockColor.Green, DoubleLockColor.Yellow });
            game.SetSecretForTests(DoubleLockColor.Red, DoubleLockColor.Blue, DoubleLockColor.Green, DoubleLockColor.Yellow);
            game.Submit(WrongGuess);
            DoubleLockSubmitResult result = game.Submit(new[] { DoubleLockColor.Red, DoubleLockColor.Blue, DoubleLockColor.Green, DoubleLockColor.Yellow });
            Assert.That(result.Completed, Is.True);
            Assert.That(game.IsCompleted, Is.True);
        }

        private static DoubleLockGame NewFixedGame()
        {
            DoubleLockGame game = new DoubleLockGame(new ZeroRandom());
            game.SetSecretForTests(DoubleLockColor.Red, DoubleLockColor.Blue, DoubleLockColor.Green, DoubleLockColor.Yellow);
            return game;
        }

        private static readonly DoubleLockColor[] WrongGuess =
        {
            DoubleLockColor.Orange, DoubleLockColor.Orange, DoubleLockColor.Orange, DoubleLockColor.Orange
        };

        private sealed class ZeroRandom : System.Random
        {
            public override int Next(int minValue, int maxValue) { return minValue; }
        }

        private sealed class SequenceRandom : System.Random
        {
            private readonly int[] values;
            private int index;
            public SequenceRandom(params int[] values) { this.values = values; }
            public override int Next(int minValue, int maxValue)
            {
                int value = values[Math.Min(index++, values.Length - 1)];
                return Math.Min(Math.Max(value, minValue), maxValue - 1);
            }
        }
    }
}
