"""Stella | Level 4 (Draft 2) | 

Project: Stella
Author:  Elias Perianza Robles, Dr. Peri

Contents:
    NumberGame      - the duel itself (rules, turn order, round loop)
    Player          - base interface every competitor implements
    RandomPlayer    - picks a legal move at random
    UserPlayer      - asks a human at the console
    StrategicPlayer - aims for the losing positions
    make_player     - console factory for a single competitor
    main            - console driver for repeated rounds
"""
from __future__ import annotations
import random


class NumberGame:
    """A counting duel between two players.

    A count starts at 0. On a turn, the player to move adds to the count any
    amount between a fixed minimum and a fixed maximum. Whoever brings the
    count up to the goal takes the round. The same setup can be replayed for
    as many rounds as the players want.

    Attributes:
        goal: Amount that has to be reached to win the round.
        min_step: Smallest amount a player may add on a turn.
        max_step: Largest amount a player may add on a turn.
        current: Value of the count at this moment.
        players: The two competitors, listed in turn order.
        turn: Number of the turn about to be played, counting from 0.
            An even number means players[0] is to move; an odd number
            means players[1] is to move.

    Invariants:
        turn is never negative.
        current stays between 0 and goal, inclusive.
        0 < min_step <= max_step <= goal.
    """
    goal: int
    min_step: int
    max_step: int
    current: int
    players: tuple[Player, Player]
    turn: int

    def __init__(self, goal: int, min_step: int, max_step: int,
                 players: tuple[Player, Player]) -> None:
        """Set up a duel with the count starting at 0 on turn 0.

        Args:
            goal: Amount that has to be reached to win the round.
            min_step: Smallest amount a player may add on a turn.
            max_step: Largest amount a player may add on a turn.
            players: The two competitors, listed in turn order.

        Requires:
            0 < min_step <= max_step <= goal.
        """
        self.goal = goal
        self.min_step = min_step
        self.max_step = max_step
        self.current = 0
        self.players = players
        self.turn = 0

    def play(self) -> str:
        """Run a full round, from a count of 0 until the goal is reached.

        Returns:
            The name of the player who took the round. The player who would
            have moved next is the loser, so the winner is whoever moved on
            the turn before that.
        """
        while self.current < self.goal:
            self.play_one_turn()
        # The player whose turn would be next (if the game weren't over) is
        # the loser. The one who went one turn before that is the winner.
        winner = self.whose_turn(self.turn - 1)
        return winner.name

    def whose_turn(self, turn: int) -> Player:
        """Identify the player who moves on a given turn.

        Args:
            turn: Number of the turn being asked about.

        Returns:
            players[0] on even turns, players[1] on odd turns.
        """
        if turn % 2 == 0:
            return self.players[0]
        else:
            return self.players[1]

    def play_one_turn(self) -> None:
        """Play out a single turn.

        Works out who is to move, asks them for an amount, folds that amount
        into the count, and advances the turn number. The move and the new
        count are reported to the console.
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
    """Base interface for a competitor in a NumberGame.

    Not meant to be used on its own. Each subclass supplies its own way of
    deciding on a move.

    Attributes:
        name: Label shown for this competitor in the console output.
    """
    name: str

    def __init__(self, name: str) -> None:
        """Store the label for this competitor.

        Args:
            name: Label shown for this competitor.
        """
        self.name = name

    def move(self, current: int, min_step: int, max_step: int, goal: int) -> int:
        """Choose an amount to add to the count.

        Args:
            current: Value of the count before this move.
            min_step: Smallest amount that may be added.
            max_step: Largest amount that may be added.
            goal: Amount that has to be reached to win the round.

        Returns:
            The amount to add. Subclasses are responsible for this.
        """
        return NotImplementedError


class RandomPlayer(Player):
    """A competitor that picks any legal amount, with no plan behind it."""

    def __init__(self, name: str) -> None:
        """Store the label for this competitor.

        Args:
            name: Label shown for this competitor.
        """
        Player.__init__(self, name)

    def move(self, current: int, min_step: int, max_step: int, goal: int) -> int:
        """Choose an amount uniformly at random from the legal range.

        Args:
            current: Value of the count before this move.
            min_step: Smallest amount that may be added.
            max_step: Largest amount that may be added.
            goal: Amount that has to be reached to win the round.

        Returns:
            A random amount between min_step and max_step, inclusive.
        """
        pick = random.randint(min_step, max_step)
        return pick


class UserPlayer(Player):
    """A competitor whose moves are typed in at the console."""

    def __init__(self, name: str) -> None:
        """Store the label for this competitor.

        Args:
            name: Label shown for this competitor.
        """
        Player.__init__(self, name)

    def move(self, current: int, min_step: int, max_step: int, goal: int) -> int:
        """Prompt at the console until a legal amount is entered.

        Args:
            current: Value of the count before this move.
            min_step: Smallest amount that may be added.
            max_step: Largest amount that may be added.
            goal: Amount that has to be reached to win the round.

        Returns:
            The amount typed in, once it falls inside the legal range.
        """
        pick = 0
        while pick < min_step or pick > max_step:
            pick = int(input(f'{self.name} choose between {min_step} and {max_step}:'))
            if pick < min_step or pick > max_step:
                print(f'Player {self.name}, that number is not valid.')
        return pick


class StrategicPlayer(Player):
    """A competitor that plays for the losing positions.

    Every count that leaves the same remainder as the goal modulo
    (min_step + max_step) is a position the player to move cannot win from
    against perfect play. This competitor steps onto one of those counts
    whenever it can reach one, and otherwise moves at random.
    """

    def __init__(self, name: str) -> None:
        """Store the label for this competitor.

        Args:
            name: Label shown for this competitor.
        """
        Player.__init__(self, name)

    def move(self, current: int, min_step: int, max_step: int, goal: int) -> int:
        """Choose an amount that lands on a losing position when possible.

        Collects every count up to the goal that matches the goal's remainder
        modulo (min_step + max_step), then takes the amount that lands on the
        last such count within reach. If that amount would fall under the
        minimum, the minimum is played instead. With nothing in reach, a
        random legal amount is played.

        Args:
            current: Value of the count before this move.
            min_step: Smallest amount that may be added.
            max_step: Largest amount that may be added.
            goal: Amount that has to be reached to win the round.

        Returns:
            The amount to add to the count.
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
    """Build one competitor from console input.

    Asks for a label and then for a competitor type: 1 for RandomPlayer,
    2 for UserPlayer, 3 for StrategicPlayer.

    Args:
        generic_name: Placeholder shown in the prompt so the user can tell
            which of the two competitors is being set up.

    Returns:
        A new Player of the chosen type, carrying the chosen label.
    """
    name = input(f'Enter a name for {generic_name}: ')
    option = int(input("Choose from -> RandomPlayer = 1, UserPlayer = 2, and StrategicPlayer. = 3"))
    d = ["RandomPlayer", "UserPlayer", "StrategicPlayer"]
    return eval(d[option - 1])(name)


def main() -> None:
    """Run the duel from the console.

    Collects the goal and the move limits, sets up the two competitors, then
    replays rounds with that same setup for as long as the user answers 'y'.
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