"""Regression tests for Level 2: The Ruins Board."""
from pathlib import Path
import sys
import unittest

LEVEL_DIR = Path(__file__).resolve().parents[1] / "level_02_ruins_board"
sys.path.insert(0, str(LEVEL_DIR))

import tablero_ruinas as ruins


class RuinsBoardTests(unittest.TestCase):
    def test_all_eight_l_orientations_are_generated(self) -> None:
        expected = {
            ((0, 0), (0, 1), (0, 2), (1, 0)),
            ((0, 0), (0, 1), (0, 2), (1, 2)),
            ((0, 0), (0, 1), (1, 0), (2, 0)),
            ((0, 0), (0, 1), (1, 1), (2, 1)),
            ((0, 0), (1, 0), (1, 1), (1, 2)),
            ((0, 0), (1, 0), (2, 0), (2, 1)),
            ((0, 1), (1, 1), (2, 0), (2, 1)),
            ((0, 2), (1, 0), (1, 1), (1, 2)),
        }
        self.assertEqual(set(ruins.ORIENTACIONES), expected)

    def test_placement_validation_checks_board_constraints(self) -> None:
        board = ruins.tablero_nuevo()
        shape = ((0, 0), (1, 0), (2, 0), (2, 1))

        self.assertTrue(ruins.movimiento_valido(board, shape, (0, 0)))
        self.assertFalse(ruins.movimiento_valido(board, shape, (1, 3)))
        self.assertFalse(ruins.movimiento_valido(board, shape, (5, 6)))

        ruins.colocar(board, shape, (0, 0), "S")
        self.assertFalse(ruins.movimiento_valido(board, shape, (0, 0)))

    def test_rotational_mirror_corresponds_and_does_not_overlap(self) -> None:
        board = ruins.tablero_nuevo()
        shape = ((0, 0), (0, 1), (0, 2), (1, 0))
        anchor = (0, 0)
        original_cells = set(ruins.celdas_de_movimiento(shape, anchor))

        mirrored_shape, mirrored_anchor = ruins.reflejar_movimiento(shape, anchor)
        mirrored_cells = set(ruins.celdas_de_movimiento(mirrored_shape, mirrored_anchor))
        expected_cells = {
            (2 * ruins.CENTRO[0] - row, 2 * ruins.CENTRO[1] - column)
            for row, column in original_cells
        }

        self.assertEqual(mirrored_cells, expected_cells)
        self.assertTrue(original_cells.isdisjoint(mirrored_cells))

        ruins.colocar(board, shape, anchor, "S")
        self.assertTrue(ruins.movimiento_valido(board, mirrored_shape, mirrored_anchor))

    def test_terminal_position_is_losing_and_one_move_position_is_winning(self) -> None:
        solver = ruins.ExactPositionSolver()
        full_board = (1 << 49) - 1
        self.assertFalse(solver.solve(full_board).is_winning)

        only_move = ruins.PLACEMENT_MASKS[0]
        result = solver.solve(full_board ^ only_move)
        self.assertTrue(result.is_winning)
        self.assertTrue(result.winning_moves)
        self.assertTrue(all(move & (full_board ^ only_move) == 0 for move in result.winning_moves))

    def test_losing_position_has_no_move_to_a_losing_child(self) -> None:
        solver = ruins.ExactPositionSolver()
        first = ruins.PLACEMENT_MASKS[0]
        mirrored = ruins.transform_mask(first, 2)
        free_cells = first | mirrored
        occupied = ((1 << 49) - 1) ^ free_cells

        self.assertFalse(solver.solve(occupied).is_winning)
        legal = [move for move in ruins.PLACEMENT_MASKS if not move & occupied]
        self.assertTrue(legal)
        self.assertTrue(all(solver.solve(occupied | move).is_winning for move in legal))

    def test_canonicalization_preserves_exact_outcome(self) -> None:
        solver = ruins.ExactPositionSolver()
        full_board = (1 << 49) - 1
        position = full_board ^ ruins.PLACEMENT_MASKS[7]
        expected = solver.solve(position).is_winning
        for transform in range(8):
            self.assertEqual(solver.solve(ruins.transform_mask(position, transform)).is_winning, expected)

    def test_initial_board_is_losing_by_rotational_pairing_theorem(self) -> None:
        solver = ruins.ExactPositionSolver()
        result = solver.solve(ruins.CENTRE_MASK)
        self.assertFalse(result.is_winning)
        self.assertTrue(ruins.is_rotationally_balanced(ruins.CENTRE_MASK))
        self.assertTrue(all(move & ruins.transform_mask(move, 2) == 0 for move in ruins.PLACEMENT_MASKS))

    def test_keeper_exploits_mistake_in_short_exact_position(self) -> None:
        solver = ruins.ExactPositionSolver()
        stella_move = ruins.PLACEMENT_MASKS[0]
        keeper_reply = ruins.transform_mask(stella_move, 2)
        free_cells = stella_move | keeper_reply
        occupied_before_mistake = ((1 << 49) - 1) ^ free_cells
        self.assertFalse(solver.solve(occupied_before_mistake).is_winning)

        board = ruins.tablero_nuevo()
        for row in range(7):
            for column in range(7):
                bit = 1 << (row * 7 + column)
                if (row, column) != ruins.CENTRO and not free_cells & bit:
                    board[row][column] = ruins.OCUPADO
        shape, anchor = ruins._move_from_mask(stella_move)
        ruins.colocar(board, shape, anchor, ruins.STELLA)

        chosen_shape, chosen_anchor = ruins.choose_keeper_move(board, solver, (shape, anchor))
        chosen_mask = ruins.cells_to_mask(ruins.celdas_de_movimiento(chosen_shape, chosen_anchor))
        self.assertEqual(chosen_mask, keeper_reply)
        self.assertFalse(solver.solve(ruins.board_occupancy_mask(board) | chosen_mask).is_winning)

    def test_every_proof_preset_is_short_symmetric_and_solver_verified(self) -> None:
        solver = ruins.ExactPositionSolver()
        self.assertGreaterEqual(len(ruins.PROOF_PRESETS), 3)
        for preset in ruins.PROOF_PRESETS:
            board = ruins.tablero_nuevo(preset)
            occupied = ruins.board_occupancy_mask(board)
            self.assertEqual(board[3][3], ruins.BLOQUEADO)
            self.assertTrue(ruins.is_rotationally_balanced(occupied))
            self.assertGreater(len(ruins.movimientos_disponibles(board)), 0)
            self.assertLessEqual(49 - occupied.bit_count(), 32)
            self.assertFalse(solver.solve(occupied).is_winning)


if __name__ == "__main__":
    unittest.main()
