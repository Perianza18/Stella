using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Stella.Level06.Tests
{
    public sealed class GalacticCustomsGameTests
    {
        [Test]
        public void NewGameStartsWithTwentySevenCandidatesAndThreeUses()
        {
            GalacticCustomsGame game = new GalacticCustomsGame();

            Assert.That(game.Candidates.Count, Is.EqualTo(27));
            Assert.That(game.UsesRemaining, Is.EqualTo(3));
            Assert.That(game.CanWeigh, Is.True);
            Assert.That(game.ReadyToGuess, Is.False);
        }

        [Test]
        public void EqualThreeWayPartitionsReduceTwentySevenToOne()
        {
            GalacticCustomsGame game = new GalacticCustomsGame();
            int[] sizes = new int[3];

            for (int i = 0; i < 3; i++)
            {
                List<int> ordered = game.Candidates.OrderBy(x => x).ToList();
                int groupSize = ordered.Count / 3;
                IEnumerable<int> left = ordered.Take(groupSize);
                IEnumerable<int> right = ordered.Skip(groupSize).Take(groupSize);
                game.Weigh(left, right);
                sizes[i] = game.Candidates.Count;
            }

            Assert.That(sizes, Is.EqualTo(new[] { 9, 3, 1 }));
        }

        [Test]
        public void UnbalancedPartitionRetainsTheLargestSubgroup()
        {
            GalacticCustomsGame game = new GalacticCustomsGame();
            IEnumerable<int> left = Enumerable.Range(1, 5);
            IEnumerable<int> right = Enumerable.Range(6, 5);

            WeighingOutcome outcome = game.Weigh(left, right);

            Assert.That(outcome, Is.EqualTo(WeighingOutcome.Balanced));
            Assert.That(game.Candidates.Count, Is.EqualTo(17));
        }

        [Test]
        public void WeighRejectsUnequalGroupSizes()
        {
            GalacticCustomsGame game = new GalacticCustomsGame();

            Assert.Throws<ArgumentException>(() => game.Weigh(new[] { 1, 2 }, new[] { 3 }));
        }

        [Test]
        public void WeighRejectsOverlappingGroups()
        {
            GalacticCustomsGame game = new GalacticCustomsGame();

            Assert.Throws<ArgumentException>(() => game.Weigh(new[] { 1, 2 }, new[] { 2, 3 }));
        }

        [Test]
        public void CannotWeighAfterThreeUses()
        {
            GalacticCustomsGame game = new GalacticCustomsGame();

            for (int i = 0; i < 3; i++)
            {
                List<int> ordered = game.Candidates.OrderBy(x => x).ToList();
                int groupSize = Math.Max(1, ordered.Count / 3);
                game.Weigh(ordered.Take(groupSize), ordered.Skip(groupSize).Take(groupSize));
            }

            Assert.That(game.CanWeigh, Is.False);
            Assert.That(game.UsesRemaining, Is.EqualTo(0));
        }

        [Test]
        public void GuessingTheOnlyRemainingCandidateWins()
        {
            GalacticCustomsGame game = new GalacticCustomsGame(totalEggs: 1, batteryUses: 0);

            bool won = game.Guess(1);

            Assert.That(won, Is.True);
            Assert.That(game.Won, Is.True);
            Assert.That(game.HasGuessed, Is.True);
        }

        [Test]
        public void GuessingBeforeTheCandidateSetIsNarrowedDownThrows()
        {
            GalacticCustomsGame game = new GalacticCustomsGame();

            Assert.Throws<InvalidOperationException>(() => game.Guess(1));
        }

        [Test]
        public void GuessingTwiceThrows()
        {
            GalacticCustomsGame game = new GalacticCustomsGame(totalEggs: 1, batteryUses: 0);
            game.Guess(1);

            Assert.Throws<InvalidOperationException>(() => game.Guess(1));
        }
    }
}
