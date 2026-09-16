# Game Vision

## Project

**Stella / The Search for the Perfect Rock** is a 2D pixel-art mathematical adventure and puzzle game for elementary- and secondary-school students.

Its core pitch is:

> Mathematical logic is the Universal Language.

Stella is a hyperactive, chaotic astronaut otter whose favorite rock disappears after her spacecraft crosses an intergalactic anomaly. She cannot communicate normally with the alien civilizations she encounters, so logical and mathematical challenges become the shared language through which she progresses.

## Educational Purpose

Stella aims to develop logical reasoning and problem-solving ability rather than formula memorization. Its educational philosophy draws from Mathematical Olympiad problem solving and discovery learning.

The player should normally not receive the winning strategy directly. Instead, the intended learning loop is to:

1. Observe.
2. Experiment.
3. Fail cheaply.
4. Identify patterns.
5. Develop a heuristic.
6. Test it.
7. Apply it successfully.

The central design principle is: **help the player notice the mathematics rather than telling them the mathematics.**

## MVP Experience

The MVP contains six mathematical puzzle levels. It does not contain an explorable overworld. A level-selection interface lets the player enter self-contained puzzle scenes directly.

The game flow is:

> Launch → Level Selection → Puzzle → Success or Failure → Retry or Level Selection

## Failure Philosophy

Failure should be cheap, fast, and interesting. The MVP therefore uses:

- no lives, permanent penalties, or resource loss;
- short retry loops and immediate retry;
- short thematic NPC or system reactions where useful;
- preserved attempt history when it helps players notice patterns; and
- no strategy-revealing hint system initially.

## Platforms and Technology

The target platforms are browser and mobile. The production game will use Unity and C#, with GitHub for version control. The Python prototypes remain mathematical and behavioral reference implementations before Unity integration.
