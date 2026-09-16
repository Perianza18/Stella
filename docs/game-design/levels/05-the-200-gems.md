# Level 5 — The 200 Gems

## Narrative hook
The Krik confirm that objects from that anomaly have been seen in the Vrex sector. On that planet there's an ancient Guardian who records everything that passes through that region of space. If anyone saw Stella's rock, it was him. But the Guardian only shares information with whoever beats him at the Gem Game.

## Level context
Stella arrives on planet Vrex, where an ancient civilization of collectors guards 200 gems as a sacred offering. The Temple Guardian, an ancient entity, offers her a deal: if she can beat him at the Gem Game, he'll hand over a star map revealing the location of the Perfect Rock.

## Overview
A mathematical numeric-deduction challenge in which the player competes in a subtraction game. The goal is to empirically discover a hidden sequence in order to corner a bot that plays optimally.

## Rules
- There are 200 gems on the altar.
- On each turn, a player may take anywhere from 1 up to half of the remaining total (rounded down; for example, if 7 gems remain, up to 3 can be taken).
- The player takes the first turn.
- Whoever takes the last gem wins.
- The Guardian plays without making any mathematical mistakes.

## Strategy (the logic behind winning)
Victory is secured by forcing the opponent to be left with a sequence of "cursed numbers": 2, 5, 11, 23, 47, 95, and 191. The mathematical pattern dictates that each number is double the previous one plus one. Starting from 200 gems, the player must take 9 on their first turn to leave the Guardian with 191. No matter how many the bot decides to take, there will always be exactly enough room for the player to bring it back down to the next number in the descending sequence, trapping it completely.

## Why it works
This level raises the difficulty by requiring experimentation and analytical record-keeping. There's no obvious visual pattern; the player is forced to try, fail, and take notes on which numbers cause an irrecoverable loss in order to deduce the hidden formula. Starting at 200 (which isn't a cursed number) guarantees the player has the mathematical advantage from the start, but it punishes any random move by handing definitive control over to the AI.
