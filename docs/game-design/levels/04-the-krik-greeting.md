# Level 4 — The Krik Greeting

## Narrative Role

The Collector identifies the anomaly signature near the outer corridor in Krik territory. To cross that territory, Stella must complete the Krik's mandatory greeting ritual.

## Player-Facing Objective

Choose who starts, then extinguish the final crystal before the Krik does.

## Interaction

The player first chooses whether Stella or the Krik starts. On Stella's turns, the player selects one, two, or three remaining crystals, clearly confirms/ends the turn, and then the Krik responds.

## Visual and UI Direction

- Stage the ritual as a close-up crystalline altar in a cold, high-technology Krik environment. All 20 crystals should be visible in a stable, easily countable arrangement.
- Give the crystals subtle idle motion or energy effects so they feel alive without making the remaining total difficult to read.
- Show torso portrait cards for Stella and the Krik beside the altar. Brighten the active participant and dim the inactive one; short thinking, nervous, frustrated, or confident reactions may reinforce turn changes.
- The player clicks or taps up to three crystals, sees the current selection clearly, then uses a prominent confirm/end-turn control. Enter may act as an optional desktop shortcut, but every essential action must remain available through the pointer.
- Lock crystal input after Stella confirms and throughout the Krik's turn. The starting-order choice appears before play and remains visible in the turn presentation.
- A visible clock and exit control may support the approved composition, but any clock is optional and non-punitive for the MVP. It must not automatically end Stella's turn; the player has time to reason and advances play only with the confirm/end-turn control. A timed challenge mode may be considered later, but it is not part of the initial MVP.

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

Implemented as a functional placeholder vertical slice in `stella_unity/Assets/_Stella/Scenes/Level04_KrikGreeting.unity`. The landscape scene includes explicit starting-player choice, a single active battle card, stable two-by-ten crystal placement, reversible 1–3 crystal selection, optimal Krik turns, win/loss reactions, and immediate Retry.
