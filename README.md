# Stella

Stella is a 2D pixel-art mathematical adventure about an astronaut otter searching the universe for her missing favorite rock. Each of its six puzzle levels uses logical reasoning as a shared language between Stella and an alien civilization..

> Mathematical logic is the Universal Language.

## Current Status

- Product vision, MVP scope, canonical story, and six level specifications are documented.
- All six levels have playable Python reference prototypes.
- Core mathematical behavior is covered by lightweight Python regression tests.
- Unity development has not started, and no Unity project has been created.

## Project Structure

- [`docs/`](docs/README.md) — authoritative product, game-design, technical, and art documentation.
- [`prototypes/python/`](prototypes/python/README.md) — mathematical and behavioral Python reference implementations.
- `stella_unity/` — reserved for the future Unity project and related content.

## Project Documentation

- [Game vision](docs/product/game-vision.md)
- [MVP scope](docs/product/mvp-scope.md)
- [Story and level index](docs/game-design/story-and-level-index.md)
- [Platform and input](docs/technical/platform-and-input.md)
- [Art direction](docs/art/art-direction.md)

## Level Status

| # | Level | Mathematical idea | Python reference | Unity |
|---|---|---|---|---|
| 1 | [Double Lock](docs/game-design/levels/01-double-lock.md) | Mastermind-style deduction | [`mastermind.py`](prototypes/python/level_01_double_lock/mastermind.py) | Not started |
| 2 | [The Ruins Board](docs/game-design/levels/02-the-ruins-board.md) | Rotational symmetry | [`tablero_ruinas.py`](prototypes/python/level_02_ruins_board/tablero_ruinas.py) | Not started |
| 3 | [The Collector](docs/game-design/levels/03-the-collector.md) | Two-coordinate impartial game | [`coleccionista.py`](prototypes/python/level_03_collector/coleccionista.py) | Not started |
| 4 | [The Krik Greeting](docs/game-design/levels/04-the-krik-greeting.md) | Modular subtraction strategy | [`krik_greeting.py`](prototypes/python/level_04_krik_greeting/krik_greeting.py) | Not started |
| 5 | [The 200 Gems](docs/game-design/levels/05-the-200-gems.md) | Variable-bound subtraction game | [`gemas.py`](prototypes/python/level_05_200_gems/gemas.py) | Not started |
| 6 | [Galactic Customs](docs/game-design/levels/06-galactic-customs.md) | Three-way partitioning | [`aduana_galactica.py`](prototypes/python/level_06_galactic_customs/aduana_galactica.py) | Not started |

## Run the Prototype Tests

The test suite uses only Python's standard library:

```bash
python3 -m unittest discover -s prototypes/python/tests -v
```
