"""Regression tests for Level 3: The Collector."""
from pathlib import Path
import sys
import unittest

LEVEL_DIR = Path(__file__).resolve().parents[1] / "level_03_collector"
sys.path.insert(0, str(LEVEL_DIR))

from coleccionista import es_posicion_perdedora, movimiento_optimo


class CollectorTests(unittest.TestCase):
    def test_off_diagonal_position_moves_to_diagonal(self) -> None:
        self.assertEqual(movimiento_optimo(7, 10), (7, 7))
        self.assertEqual(movimiento_optimo(9, 4), (4, 4))

    def test_diagonal_positions_are_losing_for_player_to_move(self) -> None:
        self.assertTrue(es_posicion_perdedora(0, 0))
        self.assertTrue(es_posicion_perdedora(5, 5))
        self.assertFalse(es_posicion_perdedora(5, 6))


if __name__ == "__main__":
    unittest.main()
