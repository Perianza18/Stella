using System;
using System.Collections.Generic;

namespace Stella.Level01
{
    public enum DoubleLockColor
    {
        Red,
        Blue,
        Green,
        Yellow,
        Purple,
        Orange
    }

    public enum DoubleLockStatus
    {
        Playing,
        BeginnerLuck,
        LockAdvanced,
        Failed,
        Completed
    }

    public sealed class DoubleLockFeedback
    {
        public DoubleLockFeedback(int exact, int misplaced)
        {
            Exact = exact;
            Misplaced = misplaced;
            Unmatched = DoubleLockGame.CodeLength - exact - misplaced;
        }

        public int Exact { get; private set; }
        public int Misplaced { get; private set; }
        public int Unmatched { get; private set; }
        public bool IsSolved { get { return Exact == DoubleLockGame.CodeLength; } }
    }

    public sealed class DoubleLockSubmitResult
    {
        internal DoubleLockSubmitResult(
            DoubleLockFeedback feedback,
            DoubleLockStatus status,
            int lockNumber,
            int attemptsUsed,
            IReadOnlyList<DoubleLockColor> revealedSecret)
        {
            Feedback = feedback;
            Status = status;
            LockNumber = lockNumber;
            AttemptsUsed = attemptsUsed;
            RevealedSecret = revealedSecret;
        }

        public DoubleLockFeedback Feedback { get; private set; }
        public DoubleLockStatus Status { get; private set; }
        public int LockNumber { get; private set; }
        public int AttemptsUsed { get; private set; }
        public IReadOnlyList<DoubleLockColor> RevealedSecret { get; private set; }
        public bool BeginnerLuckTriggered { get { return Status == DoubleLockStatus.BeginnerLuck; } }
        public bool LockAdvanced { get { return Status == DoubleLockStatus.LockAdvanced; } }
        public bool Failed { get { return Status == DoubleLockStatus.Failed; } }
        public bool Completed { get { return Status == DoubleLockStatus.Completed; } }
    }

    /// <summary>Pure Mastermind rules and two-lock progression for Level 1.</summary>
    public sealed class DoubleLockGame
    {
        public const int LockCount = 2;
        public const int CodeLength = 4;
        public const int PaletteSize = 6;
        public const int MaxAttempts = 6;

        private readonly System.Random random;
        private readonly DoubleLockColor[] secret = new DoubleLockColor[CodeLength];
        private bool beginnerLuckUsed;

        public DoubleLockGame(System.Random randomSource = null)
        {
            random = randomSource ?? new System.Random();
            ResetSequence();
        }

        public int CurrentLock { get; private set; }
        public int AttemptsUsed { get; private set; }
        public DoubleLockStatus Status { get; private set; }
        public bool BeginnerLuckUsed { get { return beginnerLuckUsed; } }
        public IReadOnlyList<DoubleLockColor> SecretCode { get { return secret; } }
        public bool IsFailed { get { return Status == DoubleLockStatus.Failed; } }
        public bool IsCompleted { get { return Status == DoubleLockStatus.Completed; } }

        public DoubleLockSubmitResult Submit(IReadOnlyList<DoubleLockColor> guess)
        {
            if (guess == null || guess.Count != CodeLength)
                throw new ArgumentException("A guess must contain exactly four colours.", nameof(guess));
            if (Status != DoubleLockStatus.Playing)
                throw new InvalidOperationException("The current security sequence is not accepting guesses.");

            DoubleLockFeedback feedback = Score(guess, secret);
            if (feedback.IsSolved)
            {
                if (AttemptsUsed == 0 && !beginnerLuckUsed)
                {
                    beginnerLuckUsed = true;
                    GenerateSecret(CurrentLock == 2);
                    Status = DoubleLockStatus.BeginnerLuck;
                    return Result(feedback, Status, null);
                }

                if (CurrentLock == 1)
                {
                    CurrentLock = 2;
                    AttemptsUsed = 0;
                    beginnerLuckUsed = false;
                    GenerateSecret(true);
                    Status = DoubleLockStatus.LockAdvanced;
                    return Result(feedback, Status, null);
                }

                Status = DoubleLockStatus.Completed;
                return Result(feedback, Status, null);
            }

            AttemptsUsed++;
            Status = AttemptsUsed >= MaxAttempts ? DoubleLockStatus.Failed : DoubleLockStatus.Playing;
            IReadOnlyList<DoubleLockColor> revealed = Status == DoubleLockStatus.Failed
                ? new List<DoubleLockColor>(secret).AsReadOnly()
                : null;
            return Result(feedback, Status, revealed);
        }

        public void ResetSequence()
        {
            CurrentLock = 1;
            AttemptsUsed = 0;
            beginnerLuckUsed = false;
            Status = DoubleLockStatus.Playing;
            GenerateSecret(false);
        }

        internal void SetSecretForTests(params DoubleLockColor[] replacement)
        {
            if (replacement == null || replacement.Length != CodeLength)
                throw new ArgumentException("A secret must contain exactly four colours.", nameof(replacement));
            Array.Copy(replacement, secret, CodeLength);
            Status = DoubleLockStatus.Playing;
        }

        internal static DoubleLockFeedback Score(
            IReadOnlyList<DoubleLockColor> guess,
            IReadOnlyList<DoubleLockColor> code)
        {
            int exact = 0;
            bool[] guessMatched = new bool[CodeLength];
            bool[] codeMatched = new bool[CodeLength];
            for (int index = 0; index < CodeLength; index++)
            {
                if (guess[index] != code[index]) continue;
                exact++;
                guessMatched[index] = true;
                codeMatched[index] = true;
            }

            int misplaced = 0;
            for (int guessIndex = 0; guessIndex < CodeLength; guessIndex++)
            {
                if (guessMatched[guessIndex]) continue;
                for (int codeIndex = 0; codeIndex < CodeLength; codeIndex++)
                {
                    if (codeMatched[codeIndex] || guess[guessIndex] != code[codeIndex]) continue;
                    misplaced++;
                    codeMatched[codeIndex] = true;
                    break;
                }
            }

            return new DoubleLockFeedback(exact, misplaced);
        }

        private void GenerateSecret(bool allowDuplicates)
        {
            List<DoubleLockColor> available = new List<DoubleLockColor>();
            for (int index = 0; index < PaletteSize; index++)
                available.Add((DoubleLockColor)index);

            for (int index = 0; index < CodeLength; index++)
            {
                int randomIndex = random.Next(0, available.Count);
                secret[index] = available[randomIndex];
                if (!allowDuplicates) available.RemoveAt(randomIndex);
            }
        }

        private DoubleLockSubmitResult Result(
            DoubleLockFeedback feedback,
            DoubleLockStatus status,
            IReadOnlyList<DoubleLockColor> revealed)
        {
            return new DoubleLockSubmitResult(feedback, status, CurrentLock, AttemptsUsed, revealed);
        }
    }
}
