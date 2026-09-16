"""Stella — Level 4: The Krik Greeting.

A focused reference implementation of the 20-crystal subtraction game.
"""
from __future__ import annotations

STARTING_CRYSTALS = 20
MIN_MOVE = 1
MAX_MOVE = 3


class KrikGreetingGame:
    """Track crystal state and enforce the Krik Greeting rules."""

    def __init__(self, remaining: int = STARTING_CRYSTALS) -> None:
        if remaining < 0:
            raise ValueError("remaining crystals cannot be negative")
        self.remaining = remaining

    def legal_moves(self) -> tuple[int, ...]:
        """Return the legal numbers of crystals removable this turn."""
        upper_bound = min(MAX_MOVE, self.remaining)
        return tuple(range(MIN_MOVE, upper_bound + 1))

    def remove_crystals(self, amount: int) -> int:
        """Apply a legal move and return the number of crystals remaining."""
        if amount not in self.legal_moves():
            raise ValueError(f"cannot remove {amount} crystals with {self.remaining} remaining")
        self.remaining -= amount
        return self.remaining

    def is_over(self) -> bool:
        """Return whether the final crystal has been removed."""
        return self.remaining == 0

    @staticmethod
    def is_losing_position(remaining: int) -> bool:
        """Return whether the player to move has no forced win under perfect play."""
        if remaining < 0:
            raise ValueError("remaining crystals cannot be negative")
        return remaining % (MAX_MOVE + 1) == 0

    @staticmethod
    def optimal_move(remaining: int) -> int:
        """Return a deterministic optimal move for the given crystal count.

        A winning position has a move to the next lower multiple of four. From
        a losing position every legal move permits a perfect response, so the
        Krik deterministically removes one crystal.
        """
        legal_moves = tuple(range(MIN_MOVE, min(MAX_MOVE, remaining) + 1))
        if not legal_moves:
            raise ValueError("there is no legal move when no crystals remain")

        winning_move = remaining % (MAX_MOVE + 1)
        if winning_move in legal_moves:
            return winning_move
        return legal_moves[0]


def ask_who_starts() -> bool:
    """Return True when Stella is chosen to start, otherwise False."""
    while True:
        answer = input("Who begins the greeting, Stella or Krik? ").strip().lower()
        if answer in {"stella", "s"}:
            return True
        if answer in {"krik", "k"}:
            return False
        print("Choose 'Stella' or 'Krik'.")


def ask_stella_move(game: KrikGreetingGame) -> int:
    """Prompt until Stella chooses one of the current legal moves."""
    legal_moves = game.legal_moves()
    while True:
        try:
            amount = int(input(f"Extinguish {legal_moves}: "))
        except ValueError:
            print("Enter a whole number.")
            continue
        if amount in legal_moves:
            return amount
        print(f"Choose one of {legal_moves}.")


def play_krik_greeting() -> str:
    """Run the console prototype and return the winner's name."""
    print("=== Level 4: The Krik Greeting ===")
    print(f"The ritual begins with {STARTING_CRYSTALS} crystals.")
    print("Each turn, extinguish 1, 2, or 3. Whoever takes the last crystal wins.\n")

    game = KrikGreetingGame()
    stella_turn = ask_who_starts()

    while not game.is_over():
        if stella_turn:
            amount = ask_stella_move(game)
            player = "Stella"
        else:
            amount = game.optimal_move(game.remaining)
            player = "Krik"
            print(f"The Krik extinguishes {amount}.")

        game.remove_crystals(amount)
        print(f"{game.remaining} crystal(s) remain.\n")

        if game.is_over():
            print(f"{player} extinguishes the final crystal and wins the greeting!")
            return player

        stella_turn = not stella_turn

    raise RuntimeError("the greeting ended without a winner")


if __name__ == "__main__":
    play_krik_greeting()
