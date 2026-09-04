# Level 4 — The Krik Greeting (Game Theory — Nim)

## Narrative hook
The Collector doesn't have the rock, but he has a lead: the anomaly Stella went through has a known signature — it occurs near the outer corridor, Krik territory. To cross it, Stella must face the mandatory greeting ritual. It's non-negotiable. The Krik make no exceptions.

## Level context
Stella lands on the mystical planet of the Krik. To get directions, she must pass the ritual greeting in front of a tall, enigmatic guard. If she loses, she suffers a comedic punishment: an incomprehensible monologue of symbols.

## Overview
A Combinatorial Game Theory duel based on the classic Nim variant, where the player uses modular arithmetic to beat a perfect bot.

## Rules
- There are 20 energy crystals on an altar.
- Stella decides whether she goes first or gives up the first turn.
- On each turn, players turn off 1, 2, or 3 crystals.
- Whoever turns off the last crystal earns the right to speak.
- The bot plays optimally and punishes any mistake.

## Strategy
Force multiples of 4. Starting with 20 crystals, Stella must give up the first turn to the Krik. After that, she applies the "complement to 4": if the bot turns off 1, she turns off 3; if the bot turns off 2, she turns off 2; if the bot turns off 3, she turns off 1. This guarantees the total shrinks by 4 each round until she wins.

## Why it works
It removes luck 100%. The system punishes randomness through modular logic, forcing the player to work out the heuristic on their own.

## Implementation note
A prototype of this mechanic (a turn-based subtraction game) already exists at `Levels/Level 4/Draft1.py`.
