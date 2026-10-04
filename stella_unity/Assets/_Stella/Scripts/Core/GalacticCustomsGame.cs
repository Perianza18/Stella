using System;
using System.Collections.Generic;
using System.Linq;

namespace Stella.Level06
{
    public enum WeighingOutcome
    {
        Balanced,
        Left,
        Right
    }

    /// <summary>
    /// Owns the rules for the Galactic Customs ternary-search puzzle: narrowing 27
    /// candidate eggs down to one heavier stowaway using at most three weighings.
    /// This class intentionally has no Unity dependencies, so its behavior is easy to test.
    /// </summary>
    public sealed class GalacticCustomsGame
    {
        public const int TotalEggs = 27;
        public const int BatteryUses = 3;

        private readonly HashSet<int> candidates;

        public int UsesRemaining { get; private set; }
        public bool HasGuessed { get; private set; }
        public bool? Won { get; private set; }

        public IReadOnlyCollection<int> Candidates { get { return candidates; } }
        public bool CanWeigh { get { return !HasGuessed && UsesRemaining > 0 && candidates.Count > 1; } }
        public bool ReadyToGuess { get { return !HasGuessed && (UsesRemaining == 0 || candidates.Count <= 1); } }

        public GalacticCustomsGame(int totalEggs = TotalEggs, int batteryUses = BatteryUses)
        {
            if (totalEggs < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(totalEggs), "There must be at least one egg.");
            }

            if (batteryUses < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(batteryUses), "Battery uses cannot be negative.");
            }

            candidates = new HashSet<int>(Enumerable.Range(1, totalEggs));
            UsesRemaining = batteryUses;
            HasGuessed = false;
            Won = null;
        }

        /// <summary>
        /// Weighs two equal-sized, non-overlapping groups of current suspects. Agent Glip
        /// resolves the outcome adversarially: whichever of the three resulting subgroups
        /// (left, right, or neither) is largest survives, punishing any split that is not
        /// an equal three-way partition.
        /// </summary>
        public WeighingOutcome Weigh(IEnumerable<int> left, IEnumerable<int> right)
        {
            if (!CanWeigh)
            {
                throw new InvalidOperationException("The scale cannot be used right now.");
            }

            HashSet<int> leftSet = new HashSet<int>(left);
            HashSet<int> rightSet = new HashSet<int>(right);

            if (leftSet.Count == 0)
            {
                throw new ArgumentException("Each pan needs at least one egg.");
            }

            if (leftSet.Count != rightSet.Count)
            {
                throw new ArgumentException("Both pans must hold the same number of eggs.");
            }

            if (leftSet.Overlaps(rightSet))
            {
                throw new ArgumentException("An egg cannot sit on both pans at once.");
            }

            if (leftSet.Any(egg => !candidates.Contains(egg)) || rightSet.Any(egg => !candidates.Contains(egg)))
            {
                throw new ArgumentException("Every weighed egg must still be a live suspect.");
            }

            HashSet<int> inLeft = new HashSet<int>(candidates);
            inLeft.IntersectWith(leftSet);
            HashSet<int> inRight = new HashSet<int>(candidates);
            inRight.IntersectWith(rightSet);
            HashSet<int> inNeither = new HashSet<int>(candidates);
            inNeither.ExceptWith(leftSet);
            inNeither.ExceptWith(rightSet);

            int largest = Math.Max(inNeither.Count, Math.Max(inLeft.Count, inRight.Count));

            WeighingOutcome outcome;
            HashSet<int> survivors;
            if (inNeither.Count == largest)
            {
                outcome = WeighingOutcome.Balanced;
                survivors = inNeither;
            }
            else if (inLeft.Count == largest)
            {
                outcome = WeighingOutcome.Left;
                survivors = inLeft;
            }
            else
            {
                outcome = WeighingOutcome.Right;
                survivors = inRight;
            }

            candidates.Clear();
            foreach (int egg in survivors) candidates.Add(egg);
            UsesRemaining--;
            return outcome;
        }

        public bool Guess(int egg)
        {
            if (HasGuessed)
            {
                throw new InvalidOperationException("Stella already made her guess.");
            }

            if (!ReadyToGuess)
            {
                throw new InvalidOperationException("The scale still has uses left and more than one suspect.");
            }

            HasGuessed = true;
            Won = candidates.Count == 1 && candidates.Contains(egg);
            return Won.Value;
        }
    }
}
