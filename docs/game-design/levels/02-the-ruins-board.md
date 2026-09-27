# Level 2 — The Ruins Board

## Narrative Role

The station archive leads Stella to the ruins of an extinct civilization. Its last nomadic keeper offers a new lead only if Stella plays the sacred game of the ancient builders.

## Player-Facing Objective

Win a Discovery Match, then prove the same structural idea on a shorter prepared ruin board. One victory is not enough to complete the level.

## Interaction

The Discovery Match begins with a choice between Stella and the Keeper starting. On Stella's turns, the player selects four board cells that form an L tetromino. Winning Discovery advances immediately to a shorter Proof Trial in which the Keeper starts from a partially occupied board; losing Proof retries Proof without replaying Discovery.

## Visual and UI Direction

- Frame the board as a close-up ceremonial stone surface inside forest-covered alien ruins, using Palenque-inspired architecture, carved reliefs, and geometric motifs as the approved visual reference.
- The 7×7 grid must remain immediately readable above the environmental detail. Make the blocked centre look like an immovable carved stone, visually distinct from both free cells and placed pieces.
- Present the starting-order choice before Discovery and keep the chosen order visible once play begins. Proof always shows `Keeper → Stella` and does not ask again.
- Use pointer selection for the four cells, with a clear L-shaped placement preview and an explicit confirm action. Invalid footprints, overlaps, and out-of-bounds cells should be rejected visually before confirmation.
- Use Stella and Keeper colours only while the most recent move is being previewed and read. Then fade every committed piece to the same neutral ancient-stone state so ownership does not expose rotational pairs. Keep the active participant obvious in the status card.
- The alien nomad and ruin scenery may frame the board, but neither should obscure cell boundaries or legal placement feedback.

## Rules

- The board is a 7×7 grid with its centre square blocked.
- Every piece is an L tetromino covering four cells.
- Pieces may be rotated and reflected.
- Players alternate placing one piece in unoccupied cells.
- Pieces may not overlap, cover the blocked centre, or extend beyond the board.
- The player chooses whether Stella or the Keeper starts in Discovery; the Keeper always starts Proof.
- A player with no legal placement loses.
- Winning Discovery advances to Proof rather than completing the level.
- Proof begins from one of several verified 180-degree-balanced partially occupied boards.

## Win Condition

Leave the Keeper with no legal placement in both Discovery and the subsequent Proof Trial.

## Loss Condition

Have no legal placement on Stella's turn. Discovery failure restarts at its starting-order choice; Proof failure offers a cheap Proof retry.

## Developer Design

### Hidden Mathematical Idea

The board and blocked centre create 180-degree rotational symmetry. Turn order determines who can control that symmetry.

### Optimal Strategy

Moving second on a 180-degree-balanced board guarantees a response by rotating the opponent's placement around the blocked centre. That pairing is a guaranteed strategy, not a claim that the literal mirror is the only winning move in every reachable position. Any legal move that preserves a forced win remains valid.

### Bot / System Behaviour

The Keeper evaluates strategic occupancy rather than ownership colours. A bitboard solver classifies reduced positions exactly, uses square symmetries for memoization, and recognizes balanced boards through the proven 180-degree pairing theorem. On early positions too large for unrestricted runtime search, the Keeper uses theorem-backed symmetry restoration when available and a deterministic resistance policy otherwise. It prefers a non-obvious proven winning move over a literal mirror whenever the solver has one, but never sacrifices a forced win merely to hide the pattern.

### Anti-Luck / Anti-Bruteforce Behaviour

The player explicitly chooses the Discovery order; it is never assigned randomly. A single Discovery victory cannot complete the level: the Keeper asks for Proof on one of three shorter, solver-verified balanced boards. Proof always gives the Keeper first move, forcing Stella to transfer the structural idea instead of memorizing one opening.

### What the Player Is Expected to Discover

The player should discover that starting order and balanced occupancy matter, the blocked centre makes opposite positions correspond, and a structural response can preserve a move whenever the opponent has one. The interface and Keeper dialogue do not state the rule.

## Implementation Status

### Python Prototype

Implemented in [`prototypes/python/level_02_ruins_board/tablero_ruinas.py`](../../../prototypes/python/level_02_ruins_board/tablero_ruinas.py), including bitboard placement generation, dihedral canonicalization, exact reduced-position evaluation, deterministic Keeper policy, three verified Proof presets, and Discovery → Proof → Completed progression.

### Unity

Implemented as a functional placeholder vertical slice in `Level02_RuinsBoard.unity`. `RuinsBoardGame` and `RuinsBoardSolver` have no Unity dependencies and own occupancy, rules, presets, turn/winner state, exact reduced-position search, symmetry proofs, and Keeper move selection. `Level02Controller` explicitly owns the Discovery, Proof, and Completed progression plus selection, previews, starting order, neutralized committed stones, result UI, and phase-appropriate retry.

The placeholder scene uses a built-in `GridLayoutGroup` with 49 plain `Button` cells generated and wired by the editor-only `Level02SceneBuilder`. `BoardCellView` stores only coordinate and semantic visual state. EditMode coverage exercises the model and strategy, non-mirror preference, phase transitions, neutral stones, Proof retry, controller interaction, and serialized scene structure.
