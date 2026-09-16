"""
Stella
Level 2 - The Ruins Board
"""
from __future__ import annotations
import random

TAMANO = 7
CENTRO = (TAMANO // 2, TAMANO // 2)
LIBRE = '.'
BLOQUEADO = '#'

# Base shape of the "L" tetromino: (row, column) relative coordinates.
FORMA_BASE = [(0, 0), (1, 0), (2, 0), (2, 1)]

Celda = tuple[int, int]
Orientacion = tuple[Celda, ...]


def normalizar(celdas: list[Celda]) -> Orientacion:
    """Shift the given cells so the top-left corner lands on (0, 0)."""
    min_r = min(r for r, _ in celdas)
    min_c = min(c for _, c in celdas)
    return tuple(sorted((r - min_r, c - min_c) for r, c in celdas))


def rotar(celdas: list[Celda]) -> list[Celda]:
    """Rotate the given cells 90 degrees."""
    return [(c, -r) for r, c in celdas]


def reflejar(celdas: list[Celda]) -> list[Celda]:
    """Reflect the given cells (horizontal mirror)."""
    return [(r, -c) for r, c in celdas]


def generar_orientaciones() -> list[Orientacion]:
    """Generate the 8 distinct orientations of the L tetromino (4 rotations x 2 reflections)."""
    formas: set[Orientacion] = set()
    for base in (FORMA_BASE, reflejar(FORMA_BASE)):
        actual = base
        for _ in range(4):
            formas.add(normalizar(actual))
            actual = rotar(actual)
    return sorted(formas)


ORIENTACIONES = generar_orientaciones()
ORIENTACIONES_VALIDAS = set(ORIENTACIONES)


def dibujar_forma(orientacion: Orientacion) -> str:
    """Return a small ASCII drawing of one orientation, to make it easier to pick."""
    max_r = max(r for r, _ in orientacion)
    max_c = max(c for _, c in orientacion)
    celdas = set(orientacion)
    filas = [''.join('■' if (r, c) in celdas else '.' for c in range(max_c + 1))
             for r in range(max_r + 1)]
    return '\n       '.join(filas)


def tablero_nuevo() -> list[list[str]]:
    """Create the 7x7 board with the center square blocked."""
    tablero = [[LIBRE] * TAMANO for _ in range(TAMANO)]
    tablero[CENTRO[0]][CENTRO[1]] = BLOQUEADO
    return tablero


def mostrar_tablero(tablero: list[list[str]]) -> None:
    print('   ' + ' '.join(str(c) for c in range(TAMANO)))
    for r, fila in enumerate(tablero):
        print(f'{r}: ' + ' '.join(fila))


def celdas_de_movimiento(orientacion: Orientacion, ancla: Celda) -> list[Celda]:
    fr, fc = ancla
    return [(fr + dr, fc + dc) for dr, dc in orientacion]


def movimiento_valido(tablero: list[list[str]], orientacion: Orientacion, ancla: Celda) -> bool:
    for r, c in celdas_de_movimiento(orientacion, ancla):
        if not (0 <= r < TAMANO and 0 <= c < TAMANO):
            return False
        if tablero[r][c] != LIBRE:
            return False
    return True


def colocar(tablero: list[list[str]], orientacion: Orientacion, ancla: Celda, simbolo: str) -> None:
    for r, c in celdas_de_movimiento(orientacion, ancla):
        tablero[r][c] = simbolo


def movimientos_disponibles(tablero: list[list[str]]) -> list[tuple[Orientacion, Celda]]:
    disponibles = []
    for orientacion in ORIENTACIONES:
        for fr in range(TAMANO):
            for fc in range(TAMANO):
                if movimiento_valido(tablero, orientacion, (fr, fc)):
                    disponibles.append((orientacion, (fr, fc)))
    return disponibles


def reflejar_movimiento(orientacion: Orientacion, ancla: Celda) -> tuple[Orientacion, Celda]:
    """Reflect a move 180 degrees about the center of the board (Mirror Strategy)."""
    reflejadas = [(2 * CENTRO[0] - r, 2 * CENTRO[1] - c)
                  for r, c in celdas_de_movimiento(orientacion, ancla)]
    min_r = min(r for r, _ in reflejadas)
    min_c = min(c for _, c in reflejadas)
    orientacion_reflejada = normalizar(reflejadas)
    return orientacion_reflejada, (min_r, min_c)


def turno_alien(tablero: list[list[str]], ultimo_movimiento_stella: tuple[Orientacion, Celda] | None,
                 alien_es_segundo: bool) -> tuple[Orientacion, Celda] | None:
    """Play the alien's turn. Returns the move made, or None if it can't place any piece.

    If the alien ended up as the second player (because Stella went first), it applies
    the Mirror Strategy to Stella's last move. If the alien moves first, it has no
    possible winning strategy, so it places a random valid piece.
    """
    if alien_es_segundo and ultimo_movimiento_stella is not None:
        orientacion, ancla = reflejar_movimiento(*ultimo_movimiento_stella)
        if movimiento_valido(tablero, orientacion, ancla):
            colocar(tablero, orientacion, ancla, 'A')
            return orientacion, ancla

    disponibles = movimientos_disponibles(tablero)
    if not disponibles:
        return None
    orientacion, ancla = random.choice(disponibles)
    colocar(tablero, orientacion, ancla, 'A')
    return orientacion, ancla


def mostrar_ejemplo_pieza() -> None:
    """Show once, at the start of the level, what an L piece looks like."""
    print("Each piece occupies 4 connected squares in an L shape (it can be rotated and reflected). Example:")
    for fila in dibujar_forma(ORIENTACIONES[0]).split('\n       '):
        print(f"   {fila}")
    print("To play, just tell me the 4 squares (row,col) you want to occupy on the board.\n")


def elegir_celdas() -> list[Celda]:
    """Ask for the 4 squares (row,col) the player wants to occupy."""
    while True:
        entrada = input("Squares to occupy, e.g. '2,5 3,5 4,5 4,6': ").strip()
        try:
            celdas = []
            for par in entrada.split():
                fr_txt, fc_txt = par.split(',')
                celdas.append((int(fr_txt), int(fc_txt)))
            if len(celdas) != 4 or len(set(celdas)) != 4:
                print("You must give exactly 4 distinct squares.")
                continue
            return celdas
        except ValueError:
            print("Invalid format. Use 'row,col' for each square, separated by spaces (e.g. '2,5 3,5 4,5 4,6').")


def turno_stella(tablero: list[list[str]]) -> tuple[Orientacion, Celda] | None:
    """Ask Stella for her move. Returns the move made, or None if she can't place any piece."""
    if not movimientos_disponibles(tablero):
        return None

    while True:
        celdas = elegir_celdas()
        orientacion = normalizar(celdas)
        if orientacion not in ORIENTACIONES_VALIDAS:
            print("Those 4 squares don't form a valid L piece (rotated or reflected). Try again.")
            continue

        min_r = min(r for r, _ in celdas)
        min_c = min(c for _, c in celdas)
        ancla = (min_r, min_c)
        if not movimiento_valido(tablero, orientacion, ancla):
            print("One of those squares is occupied, is the blocked center, or falls outside the board. Try again.")
            continue

        colocar(tablero, orientacion, ancla, 'S')
        return orientacion, ancla


def preguntar_quien_empieza() -> bool:
    """Ask who places the first piece. Returns True if the player goes first."""
    print("\nThe nomad stares at you: 'Choose wisely who takes the first step...'")
    respuesta = input("Who places the first piece: you or the alien? (you/alien) [alien]: ")
    return respuesta.strip().lower() in ('you', 'player', 'y', 'p')


def jugar_nivel_2() -> str:
    """Play the complete Level 2. Returns 'Stella' or 'Alien' depending on who wins."""
    print("=== Level 2: The Ruins Board ===")
    print("The nomad alien challenges Stella to the sacred game of the ancient builders.")
    print(f"{TAMANO}x{TAMANO} board, center square blocked. Whoever can't place a piece loses.\n")
    mostrar_ejemplo_pieza()

    tablero = tablero_nuevo()
    mostrar_tablero(tablero)

    jugador_empieza = preguntar_quien_empieza()
    alien_es_segundo = jugador_empieza
    ultimo_movimiento_stella: tuple[Orientacion, Celda] | None = None
    turno_de_stella = jugador_empieza

    while True:
        if turno_de_stella:
            movimiento_stella = turno_stella(tablero)
            if movimiento_stella is None:
                print("\nStella can't place any more pieces. The alien wins.")
                return 'Alien'
            ultimo_movimiento_stella = movimiento_stella
            mostrar_tablero(tablero)
        else:
            movimiento_alien = turno_alien(tablero, ultimo_movimiento_stella, alien_es_segundo)
            if movimiento_alien is None:
                print("\nThe alien can't place any more pieces. Stella wins!")
                return 'Stella'
            print(f"\nThe alien places a piece at {movimiento_alien[1]}.")
            mostrar_tablero(tablero)

        turno_de_stella = not turno_de_stella


if __name__ == '__main__':
    jugar_nivel_2()
