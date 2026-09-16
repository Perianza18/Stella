# Level 4 — The Krik Greeting

## Narrative Role

The Collector identifies the anomaly signature near the outer corridor in Krik territory. To cross that territory, Stella must complete the Krik's mandatory greeting ritual.

## Player-Facing Objective

Choose who starts, then extinguish the final crystal before the Krik does.

## Interaction

The player first chooses whether Stella or the Krik starts. On Stella's turns, the player chooses to extinguish one, two, or three remaining crystals.

## Rules

- The ritual begins with 20 crystals.
- Stella chooses whether Stella or the Krik takes the first turn.
- On a turn, the active player extinguishes one, two, or three crystals, subject to the number remaining.
- Stella and the Krik alternate turns.
- The participant who extinguishes the final crystal wins.
- The Krik plays optimally.

## Win Condition

Stella extinguishes the final crystal.

## Loss Condition

The Krik extinguishes the final crystal.

## Developer Design

### Hidden Mathematical Idea

This is a subtraction game modulo four. Multiples of four are losing positions for the player whose turn begins there under perfect play.

### Optimal Strategy

Because the ritual starts at 20, Stella must let the Krik move first. After every Krik move of `k` crystals, Stella removes `4 - k`. Each pair of moves removes four crystals and returns the Krik to a multiple of four.

### Bot / System Behaviour

If a winning move exists, the Krik removes the number of crystals that leaves a multiple of four. If the Krik begins on a multiple of four, no forced winning move exists; it makes a legal move and then punishes any failure by Stella to restore the multiple-of-four pattern.

### Anti-Luck / Anti-Bruteforce Behaviour

The Krik uses the optimal modulo-four response whenever available. The starting-order choice is explicit, so the player must recognize both the correct order and the complementary move pattern.

### What the Player Is Expected to Discover

The player should discover that starting order matters, consecutive moves pair naturally to four, and maintaining multiples of four controls the game.

## Implementation Status

### Python Prototype

Implemented in [`prototypes/python/level_04_krik_greeting/krik_greeting.py`](../../../prototypes/python/level_04_krik_greeting/krik_greeting.py), with game rules separated from console input and deterministic optimal Krik play.

### Unity

Not started. The Unity project has not been created.
