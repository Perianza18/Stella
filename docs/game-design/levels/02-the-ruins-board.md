# Level 2 — The Ruins Board

## Narrative Role

The station archive leads Stella to the ruins of an extinct civilization. Its last nomadic keeper offers a new lead only if Stella plays the sacred game of the ancient builders.

## Player-Facing Objective

Choose who starts, then outlast the alien by placing legal stone pieces until the alien has no move.

## Interaction

Before play, the player chooses whether Stella or the alien takes the first turn. On Stella's turns, the player selects four board cells that form an L tetromino.

## Rules

- The board is a 7×7 grid with its centre square blocked.
- Every piece is an L tetromino covering four cells.
- Pieces may be rotated and reflected.
- Players alternate placing one piece in unoccupied cells.
- Pieces may not overlap, cover the blocked centre, or extend beyond the board.
- The player chooses whether Stella or the alien starts.
- A player with no legal placement loses.

## Win Condition

Leave the alien with no legal placement on its turn.

## Loss Condition

Have no legal placement on Stella's turn.

## Developer Design

### Hidden Mathematical Idea

The board and blocked centre create 180-degree rotational symmetry. Turn order determines who can control that symmetry.

### Optimal Strategy

Choose to move second. After every opposing placement, place the matching L tetromino rotated 180 degrees around the blocked centre. The centre prevents a legal piece from occupying the symmetry point, so the mirrored response remains distinct and available under correct play.

### Bot / System Behaviour

When the alien is second, it mirrors Stella's previous move and therefore plays optimally. When the alien is first, it chooses a legal opening but cannot defeat a player who maintains the rotational pairing.

### Anti-Luck / Anti-Bruteforce Behaviour

The player explicitly chooses the starting order; it is never assigned randomly. If Stella chooses the strategically weaker order, the alien's mirrored responses consistently expose that decision rather than allowing a lucky sequence of weak bot moves.

### What the Player Is Expected to Discover

The player should discover that starting order matters, the blocked centre makes opposite positions correspond, and responding symmetrically preserves a move whenever the opponent has one.

## Implementation Status

### Python Prototype

Implemented in [`prototypes/python/level_02_ruins_board/tablero_ruinas.py`](../../../prototypes/python/level_02_ruins_board/tablero_ruinas.py), including the starting-order choice and mirrored response logic.

### Unity

Not started. The Unity project has not been created.
