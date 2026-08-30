"""
Elias Perianza Robles › Dr. Peri
Stella
Level 4 - Draft 1
"""
from __future__ import annotations
import random


class NumberGame:
    """A number game for two players.

    A count starts at 0. On a player's turn, they add to the count an amount
    between a set minimum and a set maximum. The player who brings the count
    to a set goal amount is the winner.

    The game can have multiple rounds.

    === Attributes ===
    goal:
        The amount to reach in order to win the game.
    min_step:
        The minimum legal move.
    max_step:
        The maximum legal move.
    current:
        The current value of the game count.
    players:
        The two players.
    turn:
        The turn the game is on, beginning with turn 0.
        If turn is even number, it is players[0]'s turn.
        If turn is any odd number, it is player[1]'s turn.

    === Representation invariants ==
    - self.turn >= 0
    - 0 <= self.current <= self.goal
    - 0 < self.min_step <= self.max_step <= self.goal
    """
    goal: int
    min_step: int
    max_step: int
    current: int
    players: tuple[Player, Player]
    turn: int

    def __init__(self, goal: int, min_step: int, max_step: int,
                 players: tuple[Player, Player]) -> None:
        """Initialize this NumberGame.

        Preconditions:
            0 < min_step <= max_step <= goal
        """
        self.goal = goal
        self.min_step = min_step
        self.max_step = max_step
        self.current = 0
        self.players = players
        self.turn = 0

    def play(self) -> str:
        """Play one round of this NumberGame. Return the name of the winner.

        A "round" is one full run of the game, from when the count starts
        at 0 until the goal is reached.
        """
        while self.current < self.goal:
            self.play_one_turn()
        # The player whose turn would be next (if the game weren't over) is
        # the loser. The one who went one turn before that is the winner.
        winner = self.whose_turn(self.turn - 1)
        return winner.name

    def whose_turn(self, turn: int) -> Player:
        """Return the Player whose turn it is on the given turn number.
        """
        if turn % 2 == 0:
            return self.players[0]
        else:
            return self.players[1]

    def play_one_turn(self) -> None:
        """Play a single turn in this NumberGame.

        Determine whose move it is, get their move, and update the current
        total as well as the number of the turn we are on.
        Print the move and the new total.
        """
        next_player = self.whose_turn(self.turn)
        amount = next_player.move(
            self.current,
            self.min_step,
            self.max_step,
            self.goal
        )
        self.current += amount
        self.turn += 1

        print(f'{next_player.name} moves {amount}.')
        print(f'Total is now {self.current}.')



class Player:
    """
    """
    name: str

    def __init__(self, name: str) -> None:
        self.name = name

    def move(self, current: int, min_step: int, max_step: int, goal: int) -> int:
        """
        """
        return NotImplementedError

class RandomPlayer(Player):
    def __init__(self, name: str) -> None:
        Player.__init__(self, name)

    def move(self, current: int, min_step: int, max_step: int, goal: int) -> int:
        """
        """
        pick = random.randint(min_step, max_step)
        return pick

class UserPlayer(Player):
    def __init__(self, name: str) -> None:
        Player.__init__(self, name)

    def move(self, current: int, min_step: int, max_step: int, goal: int) -> int:
        """
        """
        pick = 0
        while pick < min_step or pick > max_step:
            pick = int(input(f'{self.name} choose between {min_step} and {max_step}:'))
            if pick < min_step or pick > max_step:
                print(f'Player {self.name}, that number is not valid.')
        return pick


class StrategicPlayer(Player):
    def __init__(self, name: str) -> None:
        Player.__init__(self, name)

    def move(self, current: int, min_step: int, max_step: int, goal: int) -> int:
        """
        pick = random.randint(min_step, max_step)
        mod = max_step + min_step
        list_loose_positions = []
        for n in range(goal + 1):
            if n % mod == goal % mod:
                list_loose_positions.append(n)
        return pick


        list_loose_positions = []
        i = goal % (min_step + max_step)
        while i <= goal-max_step:
            for n in range(i, i+max_step):
                list_loose_positions.append(n)
            i += min_step+1
        pick = min_step
        for e in list_loose_positions:
            if current < e <= current + max_step:
                pick = e - current
                if pick < min_step:
                    pick = min_step
        return pick
        """
        pick = random.randint(min_step, max_step)
        mod = max_step + min_step
        list_loose_positions = []
        for n in range(goal + 1):
            if n % mod == goal % mod:
                list_loose_positions.append(n)

        for e in list_loose_positions:
            if current < e <= current + max_step:
                pick = e - current
                if pick < min_step:
                    pick = min_step
        return pick


def make_player(generic_name: str) -> Player:
    """Return a new Player based on user input.

    Allow the user to choose a player name and player type.
    <generic_name> is a placeholder used to identify which player is being made.
    """
    name = input(f'Enter a name for {generic_name}: ')
    option = int(input("Choose from -> RandomPlayer = 1, UserPlayer = 2, and StrategicPlayer. = 3"))
    d = ["RandomPlayer", "UserPlayer", "StrategicPlayer"]
    return eval(d[option - 1])(name)
    #option = (input("Chose from -> RandomPlayer = A, UserPlayer = B, and StrategicPlayer. = C"))
    #eval(option)(name)
    #[RandomPlayer, UserPlayer, StrategicPlayer][option - 1]
    #A = RandomPlayer
    #B = UserPlayer
    #C = StrategicPlayer
    #return eval(option)(name)


def main() -> None:
    """Play multiple rounds of a NumberGame based on user input settings.
    """
    goal = int(input('Enter goal amount: '))
    minimum = int(input('Enter minimum move: '))
    maximum = int(input('Enter maximum move: '))
    p1 = make_player('p1')
    p2 = make_player('p2')
    while True:
        g = NumberGame(goal, minimum, maximum, (p1, p2))
        winner = g.play()
        print(f'And {winner} is the winner!!!')
        print(p1)
        print(p2)
        again = input('Again? (y/n) ')
        if again != 'y':
            return



main()
