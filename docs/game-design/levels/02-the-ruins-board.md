# Level 2 — The Ruins Board

## Narrative Role

The station archive leads Stella to the ruins of an extinct civilization. Its last nomadic keeper offers a new lead only if Stella plays the sacred game of the ancient builders.

## Player-Facing Objective

Choose who starts, then outlast the alien by placing legal stone pieces until the alien has no move.

## Interaction

Before play, the player chooses whether Stella or the alien takes the first turn. On Stella's turns, the player selects four board cells that form an L tetromino.

## Visual and UI Direction

- Frame the board as a close-up ceremonial stone surface inside forest-covered alien ruins, using Palenque-inspired architecture, carved reliefs, and geometric motifs as the approved visual reference.
- The 7×7 grid must remain immediately readable above the environmental detail. Make the blocked centre look like an immovable carved stone, visually distinct from both free cells and placed pieces.
- Present the starting-order choice before the first placement and keep the chosen order visible once play begins.
- Use pointer selection for the four cells, with a clear L-shaped placement preview and an explicit confirm action. Invalid footprints, overlaps, and out-of-bounds cells should be rejected visually before confirmation.
- Keep Stella's and the alien's pieces visually distinct, and make the active participant obvious without highlighting rotational pairs or otherwise revealing the mirror strategy.
- The alien nomad and ruin scenery may frame the board, but neither should obscure cell boundaries or legal placement feedback.

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
