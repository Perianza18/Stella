"""
Stella
Level 3 - The Collector (Nim 2D / Tower)
"""
from __future__ import annotations
import random

FILA_INICIAL = 7
COLUMNA_INICIAL = 10


def movimiento_optimo(fila: int, columna: int) -> tuple[int, int]:
    """Return the Collector's next position.

    If the stone isn't on the diagonal (fila != columna), move it onto the diagonal
    by reducing the larger coordinate to match the smaller one (a winning move). If
    it's already on the diagonal, the Collector is in a losing position: there is no
    winning move, so it randomly reduces one of the two coordinates instead.
    """
    if columna > fila:
        return fila, fila
    if fila > columna:
        return columna, columna

    if random.choice(('fila', 'columna')) == 'fila':
        return random.randint(0, fila - 1), columna
    return fila, random.randint(0, columna - 1)


def pedir_movimiento(fila: int, columna: int) -> tuple[int, int]:
    """Ask Stella for her move: slide the stone left or down."""
    while True:
        direccion = input("Move the stone 'left' or 'down'? ").strip().lower()
        if direccion in ('left', 'l'):
            eje_maximo = columna
        elif direccion in ('down', 'd'):
            eje_maximo = fila
        else:
            print("Answer 'left' or 'down'.")
            continue

        if eje_maximo == 0:
            print("That direction is already at 0; choose the other one.")
            continue

        try:
            cantidad = int(input(f"How many squares do you want to move (1-{eje_maximo})? "))
        except ValueError:
            print("Enter a valid number.")
            continue

        if not 1 <= cantidad <= eje_maximo:
            print(f"Choose a number between 1 and {eje_maximo}.")
            continue

        if direccion in ('left', 'l'):
            return fila, columna - cantidad
        return fila - cantidad, columna


def mostrar_posicion(fila: int, columna: int) -> None:
    """Draw the grid with Stella's capsule at (0,0) and the stone at its current spot."""
    tamano = max(FILA_INICIAL, COLUMNA_INICIAL)
    print('     ' + ''.join(f'{c:>3}' for c in range(tamano + 1)))
    for r in range(tamano + 1):
        celdas = []
        for c in range(tamano + 1):
            if (r, c) == (fila, columna):
                celdas.append('  ★')
            elif (r, c) == (0, 0):
                celdas.append('  C')
            else:
                celdas.append('  .')
        print(f'{r:>3}:' + ''.join(celdas))


def jugar_nivel_3() -> str:
    """Play the complete Level 3. Returns 'Stella' or 'Collector' depending on who wins."""
    print("=== Level 3: The Collector ===")
    print("The Collector keeps the Perfect Rock inside a holographic Containment Matrix.")
    print("Both of you use tractor beams to slide the stone: only left or down.")
    print("Whoever lands it exactly on (0,0) wins.\n")

    fila, columna = FILA_INICIAL, COLUMNA_INICIAL
    mostrar_posicion(fila, columna)
    turno_de_stella = True

    while True:
        if turno_de_stella:
            fila, columna = pedir_movimiento(fila, columna)
            print(f"\nStella slides the stone to (row={fila}, col={columna}).")
        else:
            fila, columna = movimiento_optimo(fila, columna)
            print(f"\nThe Collector slides the stone to (row={fila}, col={columna}).")

        mostrar_posicion(fila, columna)

        if fila == 0 and columna == 0:
            if turno_de_stella:
                print("\nThe stone lands on (0,0)! The Collector releases the Containment Matrix.")
                return 'Stella'
            print("\nThe Collector lands the stone on (0,0). It keeps the Perfect Rock... for now.")
            return 'Collector'

        turno_de_stella = not turno_de_stella


if __name__ == '__main__':
    jugar_nivel_3()
