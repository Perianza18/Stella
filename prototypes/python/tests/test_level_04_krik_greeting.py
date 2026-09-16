"""Regression tests for Level 4: The Krik Greeting."""
from pathlib import Path
import sys
import unittest

LEVEL_DIR = Path(__file__).resolve().parents[1] / "level_04_krik_greeting"
sys.path.insert(0, str(LEVEL_DIR))

from krik_greeting import KrikGreetingGame


class KrikGreetingTests(unittest.TestCase):
    def test_legal_moves_are_one_to_three_subject_to_remaining(self) -> None:
        self.assertEqual(KrikGreetingGame(20).legal_moves(), (1, 2, 3))
        self.assertEqual(KrikGreetingGame(2).legal_moves(), (1, 2))
        self.assertEqual(KrikGreetingGame(1).legal_moves(), (1,))

    def test_twenty_is_losing_for_player_to_move(self) -> None:
        self.assertTrue(KrikGreetingGame.is_losing_position(20))

    def test_optimal_move_uses_modulo_four_strategy(self) -> None:
        for remaining in range(1, 21):
            move = KrikGreetingGame.optimal_move(remaining)
            self.assertIn(move, KrikGreetingGame(remaining).legal_moves())
            if remaining % 4:
                self.assertEqual((remaining - move) % 4, 0)

    def test_complementary_responses_sum_to_four(self) -> None:
        for opponent_move in (1, 2, 3):
            response = KrikGreetingGame.optimal_move(20 - opponent_move)
            self.assertEqual(opponent_move + response, 4)


if __name__ == "__main__":
    unittest.main()
