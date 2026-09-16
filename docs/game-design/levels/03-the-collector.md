# Level 3 — The Collector

## Narrative Role

The nomad sends Stella to The Vault. The Collector may understand the anomaly but shares information only after Stella wins a gravitational duel. He does not possess Stella's rock; after the duel, he identifies the anomaly signature and points her toward Krik territory.

## Player-Facing Objective

Move a neutral holographic token to the origin before the Collector does.

## Interaction

On each turn, the active player selects one of the token's two coordinates and reduces it by a positive amount.

## Rules

- The token occupies a position `(x, y)` on a two-coordinate grid.
- A move reduces exactly one coordinate.
- Coordinates may never increase or go below zero.
- Stella and the Collector alternate moves.
- Landing exactly at `(0, 0)` ends the duel.

## Win Condition

Stella moves the token to `(0, 0)`.

## Loss Condition

The Collector moves the token to `(0, 0)` first.

## Developer Design

### Hidden Mathematical Idea

Positions on the main diagonal, where `x = y`, are losing positions for the player whose turn begins there under correct play.

### Optimal Strategy

From an off-diagonal position, reduce the larger coordinate until it equals the smaller coordinate. After the opponent changes one coordinate and leaves the diagonal, restore equality. This eventually forces the opponent to expose the final move to `(0, 0)`.

### Bot / System Behaviour

From an off-diagonal position, the Collector always returns the token to the diagonal. From a diagonal position, no forced winning move exists, so any legal coordinate reduction is acceptable.

### Anti-Luck / Anti-Bruteforce Behaviour

The Collector applies the diagonal strategy whenever it is available. Random moves cannot defeat correct play; the player must recognize and maintain the invariant.

### What the Player Is Expected to Discover

The player should discover that the two coordinates can be balanced, that equal-coordinate positions transfer the disadvantage to the next player, and that restoring equality controls the duel.

## Implementation Status

### Python Prototype

Implemented in [`prototypes/python/level_03_collector/coleccionista.py`](../../../prototypes/python/level_03_collector/coleccionista.py). The prototype uses a neutral game token.

### Unity

Not started. The Unity project has not been created.
