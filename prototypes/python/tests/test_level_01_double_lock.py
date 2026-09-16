"""Regression tests for Level 1: Double Lock."""
from pathlib import Path
import sys
import unittest

LEVEL_DIR = Path(__file__).resolve().parents[1] / "level_01_double_lock"
sys.path.insert(0, str(LEVEL_DIR))

from mastermind import evaluar_intento


class DoubleLockTests(unittest.TestCase):
    def test_exact_matches_are_counted(self) -> None:
        code = ["Red", "Blue", "Green", "Yellow"]
        self.assertEqual(evaluar_intento(code, code), (4, 0))

    def test_misplaced_colours_are_counted(self) -> None:
        code = ["Red", "Blue", "Green", "Yellow"]
        guess = ["Blue", "Green", "Yellow", "Red"]
        self.assertEqual(evaluar_intento(guess, code), (0, 4))

    def test_duplicate_colours_are_not_double_counted(self) -> None:
        code = ["Red", "Red", "Blue", "Green"]
        guess = ["Red", "Blue", "Red", "Red"]
        self.assertEqual(evaluar_intento(guess, code), (1, 2))


if __name__ == "__main__":
    unittest.main()
