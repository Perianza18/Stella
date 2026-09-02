"""
Stella
Level 2 - El Tablero de las Ruinas
"""
from __future__ import annotations
import random

TAMANO = 7
CENTRO = (TAMANO // 2, TAMANO // 2)
LIBRE = '.'
BLOQUEADO = '#'

# Forma base del tetrominó en "L": (fila, columna) relativas.
FORMA_BASE = [(0, 0), (1, 0), (2, 0), (2, 1)]

Celda = tuple[int, int]
Orientacion = tuple[Celda, ...]


def normalizar(celdas: list[Celda]) -> Orientacion:
    """Desplaza las celdas para que la esquina superior-izquierda quede en (0, 0)."""
    min_r = min(r for r, _ in celdas)
    min_c = min(c for _, c in celdas)
    return tuple(sorted((r - min_r, c - min_c) for r, c in celdas))


def rotar(celdas: list[Celda]) -> list[Celda]:
    """Rota 90 grados las celdas dadas."""
    return [(c, -r) for r, c in celdas]


def reflejar(celdas: list[Celda]) -> list[Celda]:
    """Refleja las celdas dadas (espejo horizontal)."""
    return [(r, -c) for r, c in celdas]


def generar_orientaciones() -> list[Orientacion]:
    """Genera las 8 orientaciones distintas del tetrominó en L (4 rotaciones x 2 reflejos)."""
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
    """Devuelve un dibujo ASCII pequeño de una orientación, para elegirla más fácil."""
    max_r = max(r for r, _ in orientacion)
    max_c = max(c for _, c in orientacion)
    celdas = set(orientacion)
    filas = [''.join('■' if (r, c) in celdas else '.' for c in range(max_c + 1))
             for r in range(max_r + 1)]
    return '\n       '.join(filas)


def tablero_nuevo() -> list[list[str]]:
    """Crea el tablero 7x7 con la casilla central bloqueada."""
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
    """Refleja un movimiento 180 grados respecto al centro del tablero (Estrategia del Espejo)."""
    reflejadas = [(2 * CENTRO[0] - r, 2 * CENTRO[1] - c)
                  for r, c in celdas_de_movimiento(orientacion, ancla)]
    min_r = min(r for r, _ in reflejadas)
    min_c = min(c for _, c in reflejadas)
    orientacion_reflejada = normalizar(reflejadas)
    return orientacion_reflejada, (min_r, min_c)


def turno_alien(tablero: list[list[str]], ultimo_movimiento_stella: tuple[Orientacion, Celda] | None,
                 alien_es_segundo: bool) -> tuple[Orientacion, Celda] | None:
    """Juega el turno del alienígena. Devuelve el movimiento, o None si no puede colocar ninguno.

    Si el alienígena quedó de segundo jugador (porque Stella empezó), aplica la Estrategia
    del Espejo sobre el último movimiento de Stella. Si el alienígena es el primero en mover,
    no tiene ninguna estrategia ganadora posible, así que coloca una pieza válida al azar.
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
    """Muestra una sola vez, al inicio del nivel, cómo luce una pieza en L."""
    print("Cada pieza ocupa 4 casillas conectadas en forma de L (se puede rotar y reflejar). Ejemplo:")
    for fila in dibujar_forma(ORIENTACIONES[0]).split('\n       '):
        print(f"   {fila}")
    print("Para jugar, simplemente dime las 4 casillas (fila,col) que quieres ocupar en el tablero.\n")


def elegir_celdas() -> list[Celda]:
    """Pide las 4 casillas (fila,col) que el jugador quiere ocupar."""
    while True:
        entrada = input("Casillas a ocupar, ej. '2,5 3,5 4,5 4,6': ").strip()
        try:
            celdas = []
            for par in entrada.split():
                fr_txt, fc_txt = par.split(',')
                celdas.append((int(fr_txt), int(fc_txt)))
            if len(celdas) != 4 or len(set(celdas)) != 4:
                print("Debes dar exactamente 4 casillas distintas.")
                continue
            return celdas
        except ValueError:
            print("Formato inválido. Usa 'fila,col' para cada casilla, separadas por espacios (ej. '2,5 3,5 4,5 4,6').")


def turno_stella(tablero: list[list[str]]) -> tuple[Orientacion, Celda] | None:
    """Pide a Stella su movimiento. Devuelve el movimiento hecho, o None si no puede colocar ninguno."""
    if not movimientos_disponibles(tablero):
        return None

    while True:
        celdas = elegir_celdas()
        orientacion = normalizar(celdas)
        if orientacion not in ORIENTACIONES_VALIDAS:
            print("Esas 4 casillas no forman una pieza en L válida (ni rotada ni reflejada). Intenta de nuevo.")
            continue

        min_r = min(r for r, _ in celdas)
        min_c = min(c for _, c in celdas)
        ancla = (min_r, min_c)
        if not movimiento_valido(tablero, orientacion, ancla):
            print("Alguna de esas casillas está ocupada, es el centro bloqueado, o se sale del tablero. Intenta de nuevo.")
            continue

        colocar(tablero, orientacion, ancla, 'S')
        return orientacion, ancla


def preguntar_quien_empieza() -> bool:
    """Pregunta quién coloca la primera pieza. Devuelve True si empieza el jugador."""
    print("\nEl nómada te mira fijamente: 'Elige bien quién da el primer paso...'")
    respuesta = input("¿Quién coloca la primera pieza: tú o el alienígena? (jugador/alien) [alien]: ")
    return respuesta.strip().lower() in ('yo', 'jugador', 'y', 'j')


def jugar_nivel_2() -> str:
    """Juega el Nivel 2 completo. Devuelve 'Stella' o 'Alien' según quién gane."""
    print("=== Nivel 2: El Tablero de las Ruinas ===")
    print("El nómada alienígena desafía a Stella al juego sagrado de los antiguos constructores.")
    print(f"Tablero {TAMANO}x{TAMANO}, casilla central bloqueada. Pierde quien no pueda colocar una pieza.\n")
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
                print("\nStella no puede colocar más piezas. El alienígena gana.")
                return 'Alien'
            ultimo_movimiento_stella = movimiento_stella
            mostrar_tablero(tablero)
        else:
            movimiento_alien = turno_alien(tablero, ultimo_movimiento_stella, alien_es_segundo)
            if movimiento_alien is None:
                print("\nEl alienígena no puede colocar más piezas. ¡Stella gana!")
                return 'Stella'
            print(f"\nEl alienígena coloca una pieza en {movimiento_alien[1]}.")
            mostrar_tablero(tablero)

        turno_de_stella = not turno_de_stella


if __name__ == '__main__':
    jugar_nivel_2()
