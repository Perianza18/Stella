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


if __name__ == "__main__":
    unittest.main()
