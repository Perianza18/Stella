"""Stella Level 2: bitboard rules plus Discovery -> Proof progression."""
from __future__ import annotations

from dataclasses import dataclass
from enum import Enum
from functools import lru_cache
import random
from typing import Iterable

TAMANO = 7
CENTRO = (3, 3)
LIBRE, BLOQUEADO, OCUPADO, STELLA, KEEPER = ".", "#", "O", "S", "K"
FORMA_BASE = [(0, 0), (1, 0), (2, 0), (2, 1)]
Celda = tuple[int, int]
Orientacion = tuple[Celda, ...]


class LevelPhase(Enum):
    DISCOVERY = "Discovery"
    PROOF = "Proof"
    COMPLETED = "Completed"


def normalizar(celdas: Iterable[Celda]) -> Orientacion:
    values = list(celdas)
    min_r = min(r for r, _ in values)
    min_c = min(c for _, c in values)
    return tuple(sorted((r - min_r, c - min_c) for r, c in values))


def rotar(celdas: Iterable[Celda]) -> list[Celda]:
    return [(c, -r) for r, c in celdas]


def reflejar(celdas: Iterable[Celda]) -> list[Celda]:
    return [(r, -c) for r, c in celdas]


def generar_orientaciones() -> list[Orientacion]:
    formas: set[Orientacion] = set()
    for base in (FORMA_BASE, reflejar(FORMA_BASE)):
        actual = list(base)
        for _ in range(4):
            formas.add(normalizar(actual))
            actual = rotar(actual)
    return sorted(formas)


ORIENTACIONES = generar_orientaciones()
ORIENTACIONES_VALIDAS = set(ORIENTACIONES)


def celdas_de_movimiento(orientacion: Orientacion, ancla: Celda) -> tuple[Celda, ...]:
    fr, fc = ancla
    return tuple((fr + dr, fc + dc) for dr, dc in orientacion)


def _bit(celda: Celda) -> int:
    return 1 << (celda[0] * TAMANO + celda[1])


CENTRE_MASK = _bit(CENTRO)


def cells_to_mask(celdas: Iterable[Celda]) -> int:
    result = 0
    for celda in celdas:
        result |= _bit(celda)
    return result


def mask_to_cells(mask: int) -> tuple[Celda, ...]:
    return tuple((index // TAMANO, index % TAMANO) for index in range(49) if mask & (1 << index))


def _generate_placements() -> tuple[int, ...]:
    placements: set[int] = set()
    for orientation in ORIENTACIONES:
        height = max(row for row, _ in orientation) + 1
        width = max(column for _, column in orientation) + 1
        for row in range(TAMANO - height + 1):
            for column in range(TAMANO - width + 1):
                placement = cells_to_mask(celdas_de_movimiento(orientation, (row, column)))
                if not placement & CENTRE_MASK:
                    placements.add(placement)
    return tuple(sorted(placements))


PLACEMENT_MASKS = _generate_placements()
PLACEMENT_CELLS = {mask: mask_to_cells(mask) for mask in PLACEMENT_MASKS}


def _transform_cell(cell: Celda, transform: int) -> Celda:
    row, column = cell
    last = TAMANO - 1
    return (
        (row, column), (column, last - row), (last - row, last - column),
        (last - column, row), (row, last - column), (last - row, column),
        (column, row), (last - column, last - row),
    )[transform]


TRANSFORMED_BITS = tuple(
    tuple(_bit(_transform_cell((index // TAMANO, index % TAMANO), transform)) for index in range(49))
    for transform in range(8)
)


def transform_mask(mask: int, transform: int) -> int:
    result = 0
    while mask:
        lowest = mask & -mask
        index = lowest.bit_length() - 1
        result |= TRANSFORMED_BITS[transform][index]
        mask ^= lowest
    return result


def canonical_mask(mask: int) -> int:
    return min(transform_mask(mask, transform) for transform in range(8))


def _canonical_with_transform(mask: int) -> tuple[int, int]:
    variants = [(transform_mask(mask, transform), transform) for transform in range(8)]
    return min(variants)


def mirror_cells(cells: Iterable[Celda]) -> tuple[Celda, ...]:
    return tuple((6 - row, 6 - column) for row, column in cells)


def is_rotationally_balanced(mask: int) -> bool:
    return transform_mask(mask, 2) == mask


@dataclass(frozen=True)
class SolveResult:
    is_winning: bool
    distance: int | None
    winning_moves: tuple[int, ...]


class ExactPositionSolver:
    """Exact memoized bitboard solver with dihedral canonicalization.

    Rotationally balanced states use the fixed-point-free 180-degree pairing
    theorem. Their exact outcome is known without expanding the huge early tree;
    distance remains None rather than being fabricated.
    """

    def __init__(self) -> None:
        self.nodes = 0

    def solve(self, occupied_mask: int) -> SolveResult:
        occupied_mask |= CENTRE_MASK
        canonical, transform = _canonical_with_transform(occupied_mask)
        result = self._solve(canonical)
        if not result.winning_moves:
            return result
        inverse = (0, 3, 2, 1, 4, 5, 6, 7)[transform]
        restored = tuple(transform_mask(move, inverse) for move in result.winning_moves)
        return SolveResult(result.is_winning, result.distance, restored)

    @lru_cache(maxsize=None)
    def _solve(self, occupied_mask: int) -> SolveResult:
        self.nodes += 1
        if is_rotationally_balanced(occupied_mask):
            return SolveResult(False, None, ())
        legal = tuple(move for move in PLACEMENT_MASKS if not move & occupied_mask)
        if not legal:
            return SolveResult(False, 0, ())

        symmetry_restoring = tuple(
            move for move in legal if is_rotationally_balanced(occupied_mask | move)
        )
        if symmetry_restoring:
            return SolveResult(True, None, symmetry_restoring)

        losing_children: list[tuple[int, SolveResult]] = []
        winning_children: list[SolveResult] = []
        for move in legal:
            child = self._solve(canonical_mask(occupied_mask | move))
            if child.is_winning:
                winning_children.append(child)
            else:
                losing_children.append((move, child))
        if losing_children:
            distances = [child.distance for _, child in losing_children if child.distance is not None]
            return SolveResult(
                True,
                1 + min(distances) if distances else None,
                tuple(move for move, _ in losing_children),
            )
        distances = [child.distance for child in winning_children if child.distance is not None]
        distance = 1 + max(distances) if len(distances) == len(winning_children) else None
        return SolveResult(False, distance, ())

    @property
    def cache_size(self) -> int:
        return self._solve.cache_info().currsize


def tablero_nuevo(preset: Iterable[Celda] = ()) -> list[list[str]]:
    board = [[LIBRE] * TAMANO for _ in range(TAMANO)]
    board[CENTRO[0]][CENTRO[1]] = BLOQUEADO
    for row, column in preset:
        if (row, column) == CENTRO or not (0 <= row < TAMANO and 0 <= column < TAMANO):
            raise ValueError("Invalid preset stone.")
        board[row][column] = OCUPADO
    return board


def board_occupancy_mask(board: list[list[str]]) -> int:
    return cells_to_mask(
        (row, column)
        for row in range(TAMANO)
        for column in range(TAMANO)
        if board[row][column] != LIBRE
    )


def movimiento_valido(board: list[list[str]], orientation: Orientacion, anchor: Celda) -> bool:
    for row, column in celdas_de_movimiento(orientation, anchor):
        if not (0 <= row < TAMANO and 0 <= column < TAMANO) or board[row][column] != LIBRE:
            return False
    return True


def colocar(board: list[list[str]], orientation: Orientacion, anchor: Celda, symbol: str) -> None:
    for row, column in celdas_de_movimiento(orientation, anchor):
        board[row][column] = symbol


def movimientos_disponibles(board: list[list[str]]) -> list[tuple[Orientacion, Celda]]:
    return [
        (orientation, (row, column))
        for orientation in ORIENTACIONES
        for row in range(TAMANO)
        for column in range(TAMANO)
        if movimiento_valido(board, orientation, (row, column))
    ]


def reflejar_movimiento(orientation: Orientacion, anchor: Celda) -> tuple[Orientacion, Celda]:
    mirrored = mirror_cells(celdas_de_movimiento(orientation, anchor))
    return normalizar(mirrored), (min(r for r, _ in mirrored), min(c for _, c in mirrored))


def _move_from_mask(mask: int) -> tuple[Orientacion, Celda]:
    cells = PLACEMENT_CELLS[mask]
    anchor = (min(r for r, _ in cells), min(c for _, c in cells))
    return normalizar(cells), anchor


def choose_keeper_move(
    board: list[list[str]],
    solver: ExactPositionSolver,
    previous_stella_move: tuple[Orientacion, Celda] | None = None,
    rng: random.Random | None = None,
    exact_free_cell_limit: int = 28,
) -> tuple[Orientacion, Celda] | None:
    occupied = board_occupancy_mask(board)
    legal = [move for move in PLACEMENT_MASKS if not move & occupied]
    if not legal:
        return None
    mirror_mask = 0
    if previous_stella_move:
        mirror_mask = cells_to_mask(mirror_cells(celdas_de_movimiento(*previous_stella_move)))

    winning = [move for move in legal if is_rotationally_balanced(occupied | move)]
    free_cells = 49 - occupied.bit_count()
    if not winning and free_cells <= exact_free_cell_limit:
        winning = [move for move in legal if not solver.solve(occupied | move).is_winning]

    candidates = winning or legal
    if winning:
        non_mirror = [move for move in candidates if move != mirror_mask]
        if non_mirror:
            candidates = non_mirror
    else:
        # In a rotationally balanced losing state, assume perfect opposition will
        # restore the pairing and choose the opening that leaves the longest
        # remaining contest. Else restrict immediate opponent mobility.
        if is_rotationally_balanced(occupied):
            resistance = {}
            for move in candidates:
                mirrored = transform_mask(move, 2)
                after_pair = occupied | move | mirrored
                resistance[move] = sum(1 for reply in PLACEMENT_MASKS if not reply & after_pair)
            best = max(resistance.values())
            candidates = [move for move in candidates if resistance[move] == best]
        else:
            mobility = {move: sum(1 for reply in PLACEMENT_MASKS if not reply & (occupied | move)) for move in candidates}
            best = min(mobility.values())
            candidates = [move for move in candidates if mobility[move] == best]

    chooser = rng or random.Random(0)
    return _move_from_mask(candidates[chooser.randrange(len(candidates))])


PROOF_PRESETS: tuple[tuple[Celda, ...], ...] = (
    ((0, 0), (0, 1), (0, 2), (1, 6), (2, 3), (2, 4), (3, 0), (3, 2), (3, 4), (3, 6), (4, 2), (4, 3), (5, 0), (6, 4), (6, 5), (6, 6)),
    ((0, 0), (0, 6), (1, 0), (1, 6), (2, 2), (2, 3), (2, 5), (3, 2), (3, 4), (4, 1), (4, 3), (4, 4), (5, 0), (5, 6), (6, 0), (6, 6)),
    ((0, 2), (0, 3), (0, 4), (0, 6), (1, 1), (1, 3), (1, 5), (2, 5), (4, 1), (5, 1), (5, 3), (5, 5), (6, 0), (6, 2), (6, 3), (6, 4)),
)


def mostrar_tablero(board: list[list[str]]) -> None:
    print("   " + " ".join(str(c) for c in range(TAMANO)))
    for row, values in enumerate(board):
        print(f"{row}: " + " ".join(values))


def elegir_celdas() -> list[Celda]:
    while True:
        try:
            cells = [tuple(map(int, pair.split(","))) for pair in input("Four row,column squares: ").split()]
            if len(cells) == 4 and len(set(cells)) == 4:
                return cells
        except ValueError:
            pass
        print("Enter exactly four distinct squares.")


def turno_stella(board: list[list[str]]) -> tuple[Orientacion, Celda] | None:
    if not movimientos_disponibles(board):
        return None
    while True:
        cells = elegir_celdas()
        orientation = normalizar(cells)
        anchor = (min(r for r, _ in cells), min(c for _, c in cells))
        if orientation in ORIENTACIONES_VALIDAS and movimiento_valido(board, orientation, anchor):
            colocar(board, orientation, anchor, STELLA)
            return orientation, anchor
        print("Those cells do not form a legal free L.")


def play_match(board: list[list[str]], stella_starts: bool, solver: ExactPositionSolver) -> str:
    stella_turn, last_stella = stella_starts, None
    while True:
        mostrar_tablero(board)
        if stella_turn:
            move = turno_stella(board)
            if move is None:
                return "Keeper"
            last_stella = move
        else:
            move = choose_keeper_move(board, solver, last_stella)
            if move is None:
                return "Stella"
            colocar(board, *move, KEEPER)
            print("\nThe keeper places an ancient stone.")
        stella_turn = not stella_turn


def jugar_nivel_2() -> None:
    solver, phase = ExactPositionSolver(), LevelPhase.DISCOVERY
    print("=== Level 2: The Ruins Board ===")
    while phase is not LevelPhase.COMPLETED:
        if phase is LevelPhase.DISCOVERY:
            stella_starts = input("Who starts? (stella/keeper) [keeper]: ").strip().lower() in {"stella", "s"}
            if play_match(tablero_nuevo(), stella_starts, solver) != "Stella":
                print("Stella has no legal placement. Try the discovery match again.\n")
                continue
            print("\nThe keeper studies the stones.\nAgain.\n")
            phase = LevelPhase.PROOF
        else:
            if play_match(tablero_nuevo(random.choice(PROOF_PRESETS)), False, solver) != "Stella":
                print("Proof failed. Retrying a short proof board.\n")
                continue
            phase = LevelPhase.COMPLETED
    print("The keeper accepts Stella's answer.")


if __name__ == "__main__":
    jugar_nivel_2()
