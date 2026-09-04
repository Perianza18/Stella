# Level 3 — The Collector (Game Theory — Nim 2D/Chomp)

## Narrative hook
The nomad sends her to The Vault: the largest collection of rare objects in the known universe. The Collector has cataloged everything space has ever discarded — including objects recovered from anomalies. He might have Stella's rock, or at least know where it ended up. But he won't talk to anyone without first beating them in his gravitational duel.

## Level context
In the gigantic "Vault", Stella confronts "The Collector," an arrogant cosmic being who holds her Perfect Rock inside a holographic Containment Matrix. The two of them fight over it using tractor beams in a gravitational duel.

## Overview
The final boss showdown. A Combinatorial Game Theory problem (a "Tower Game") where a token is moved on a grid using advanced winning-position concepts.

## Rules
- The setting is a grid (e.g. 10x10) with Stella's capsule at coordinate (0,0).
- The stone starts at an asymmetric upper coordinate (like 7, 10).
- On their turn, the player slides the stone any number of squares, only to the left or downward.
- Whoever manages to land the stone exactly on (0,0) wins.

## Strategy
The heuristic is to keep to the main diagonal (where x=y). If the stone starts at (7, 10), Stella brings it down to (7, 7). When the bot pulls it off the diagonal by moving it left (e.g. 2, 7), Stella mirrors the move downward to return it to the diagonal (2, 2). This eventually forces the bot to leave it at (0, X) or (X, 0), letting Stella win on her next turn.

## Why it works
It turns abstract coordinates into a mechanical, visual confrontation. The Collector's code detects any mistake immediately and executes its relentless strategy, forcing the player to understand the geometric pattern.
