"""Regression tests for Level 5: The 200 Gems."""
from pathlib import Path
import sys
import unittest

LEVEL_DIR = Path(__file__).resolve().parents[1] / "level_05_200_gems"
sys.path.insert(0, str(LEVEL_DIR))

from gemas import calcular_posiciones_perdedoras, limite_de_turno, movimiento_optimo


class TwoHundredGemsTests(unittest.TestCase):
    def test_losing_positions_through_two_hundred(self) -> None:
        losing = calcular_posiciones_perdedoras(200)
        actual = [remaining for remaining in range(1, 201) if losing[remaining]]
        self.assertEqual(actual, [2, 5, 11, 23, 47, 95, 191])

    def test_guardian_returns_opponent_to_losing_position_when_possible(self) -> None:
        losing = calcular_posiciones_perdedoras(200)
        for remaining in range(1, 201):
            if losing[remaining]:
                continue

            move = movimiento_optimo(remaining)
            self.assertGreaterEqual(move, 1)
            self.assertLessEqual(move, limite_de_turno(remaining))
            after_move = remaining - move
            self.assertTrue(after_move == 0 or losing[after_move])


if __name__ == "__main__":
    unittest.main()
