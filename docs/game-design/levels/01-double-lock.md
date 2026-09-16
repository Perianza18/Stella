# Level 1 — Double Lock

## Narrative Role

Stella retraces her route to an abandoned station where Zyx-7 archived information about anomalies. Opening the station's sealed archive gives her the lead that sends her to the ruins planet.

## Player-Facing Objective

Open both security locks by deducing each hidden four-colour code before its attempts run out.

## Interaction

The player selects one colour for each of four slots, submits the guess, and reads the positional and colour feedback returned by the lock.

## Visual and UI Direction

- Present the puzzle as a close-up, first-person view of an alien control panel built into the station's sealed door. No visible hands are required.
- Keep the four code slots central, with six large mineral-like gem controls directly below them. The controls should feel integrated into an old spacecraft console rather than placed over it as generic buttons.
- Place the submitted-guess history and its feedback indicators to the left of the active code. Preserve previous rows so the player can compare evidence across attempts.
- Use Stella and the archive security system as expressive state cards on the right. Lighting or dimming a card may show when the player is choosing and when the system is responding, but it must not imply an additional mathematical turn or place Zyx-7 physically at the station.
- Show the current lock phase and remaining attempts at the top. Attempts should be represented by readable pips rather than requiring a text-heavy HUD.
- Keep feedback visual: bright green for correct colour and position, yellow for correct colour in the wrong position, and muted grey for submitted gems not matched by either category. Do not add explanatory strategy text or reveal the code's logic.

## Rules

- Each code has four slots and uses six available colours.
- Colours may repeat.
- Each submitted guess receives separate counts for correct colour in the correct position and correct colour in the wrong position.
- Each lock allows six counted attempts.
- The player must open two consecutive locks.
- Failing either lock restarts the sequence at the first lock.

## Win Condition

Open both locks in the same run.

## Loss Condition

Use all six attempts on either lock without entering its code.

## Developer Design

### Hidden Mathematical Idea

This is a Mastermind-style constraint and elimination problem. Every response reduces the set of codes consistent with the observed evidence.

### Optimal Strategy

Begin with an information-rich guess, normally using distinct colours, then retain only combinations consistent with every exact and misplaced-colour count. Choose later guesses to distinguish among the remaining possibilities rather than guessing randomly.

### Bot / System Behaviour

The system generates a four-colour code, scores exact matches first, then scores misplaced colours without reusing matched code entries. It maintains separate attempt counts for each lock.

### Anti-Luck / Anti-Bruteforce Behaviour

If the player guesses a lock's code on its first submission, that uncounted beginner's-luck result causes the system to replace the code. Normal play then continues, requiring evidence-based deduction rather than a one-shot lucky pass.

### What the Player Is Expected to Discover

The player should discover that varied guesses reveal more information, feedback must be combined across attempts, and duplicate colours must be reasoned about carefully.

## Implementation Status

### Python Prototype

Implemented in [`prototypes/python/level_01_double_lock/mastermind.py`](../../../prototypes/python/level_01_double_lock/mastermind.py).

### Unity

Not started. The Unity project has not been created.
