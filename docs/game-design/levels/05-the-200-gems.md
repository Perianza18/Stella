# Level 5 — The 200 Gems

## Narrative Role

The Krik direct Stella to the Vrex sector, where an ancient Guardian records passing objects and events. The Guardian will share what he observed only if Stella defeats him in the Game of 200 Gems.

## Player-Facing Objective

Take the final gem before the Guardian does.

## Interaction

On each of Stella's turns, the player chooses how many gems to take from the remaining pile. The interface should retain useful numerical attempt history so patterns can emerge across retries.

## Visual and UI Direction

- Set the game at a Vrex temple or mineral market that also feels like an ancient archive: carved structures, stored valuables, robed figures, and old technological displays can coexist around the Guardian's altar.
- Treat the gems as mineral coins or catalogued offerings. Represent the large pile symbolically while keeping the exact remaining total prominent and unambiguous; the player should not need to count 200 individual sprites.
- Show the current legal range and selected amount beside large pointer-friendly amount controls and an explicit confirm action.
- Keep a visible record of Stella's and the Guardian's previous moves and the totals they left behind. This history supports observation without identifying special totals or revealing the recurrence.
- Use an old terminal or LED-like inventory display to reinforce the Guardian's record-keeping role. The Guardian may react to moves, but the display must not grade a move as strategically good or bad.
- Prioritize numerical legibility over decorative piles, market props, or animation, especially on mobile screens.

## Rules

- The game begins with 200 gems.
- Stella takes the first turn.
- A turn removes at least one gem and at most half of the remaining gems, rounded down.
- If only one gem remains, it may be taken.
- Stella and the Guardian alternate turns.
- The participant who takes the final gem wins.
- The Guardian plays perfectly.

## Win Condition

Stella takes the final gem.

## Loss Condition

The Guardian takes the final gem.

## Developer Design

### Hidden Mathematical Idea

The positive losing positions through 200 are `2, 5, 11, 23, 47, 95, 191`. Each follows `next = 2 × previous + 1`.

### Optimal Strategy

From 200, take nine gems to leave 191. After every Guardian move, take enough to leave the next lower losing position. More generally, move to a losing position whenever the legal range permits it.

### Bot / System Behaviour

The Guardian evaluates legal moves and chooses one that leaves Stella a losing position whenever possible. If the Guardian begins in a losing position, no move can preserve control against perfect play, so any legal move is acceptable.

### Anti-Luck / Anti-Bruteforce Behaviour

Perfect Guardian play converts most unstructured choices into a loss. Attempt and remaining-total history should be preserved where possible, allowing discovery through evidence without directly stating the sequence.

### What the Player Is Expected to Discover

The player should identify recurring losing totals, infer how consecutive totals are related, and deliberately steer the Guardian back through that sequence.

## Implementation Status

### Python Prototype

Implemented in [`prototypes/python/level_05_200_gems/gemas.py`](../../../prototypes/python/level_05_200_gems/gemas.py), including dynamic losing-position calculation and optimal Guardian moves.

### Unity

Not started. The Unity project has not been created.
