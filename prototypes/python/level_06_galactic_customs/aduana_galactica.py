"""
Stella
Level 6 - Galactic Customs (Ternary Search)
"""
from __future__ import annotations

TOTAL_HUEVOS = 27
USOS_BATERIA = 3


def leer_grupo(mensaje: str, ya_usados: set[int]) -> set[int]:
    """Ask for a group of eggs (1 to TOTAL_HUEVOS) separated by spaces.

    They can't repeat within the group, nor overlap with `ya_usados` (the eggs
    already assigned to the other pan of the scale).
    """
    while True:
        entrada = input(mensaje).strip()
        try:
            numeros = {int(n) for n in entrada.split()}
        except ValueError:
            print("Use only numbers separated by spaces.")
            continue

        if not numeros:
            print("You must name at least one egg.")
            continue
        if not all(1 <= n <= TOTAL_HUEVOS for n in numeros):
            print(f"Eggs are numbered 1 to {TOTAL_HUEVOS}.")
            continue
        if numeros & ya_usados:
            print("An egg can't be on both pans of the scale at once.")
            continue
        return numeros


def pedir_pesada() -> tuple[set[int], set[int]]:
    """Ask for the two groups (left and right) of one weighing.

    They must be the same size, otherwise the scale gives no useful reading.
    """
    while True:
        izquierda = leer_grupo("Eggs on the LEFT pan: ", set())
        derecha = leer_grupo("Eggs on the RIGHT pan: ", izquierda)
        if len(izquierda) != len(derecha):
            print("Both pans must hold the same number of eggs.\n")
            continue
        return izquierda, derecha


def resolver_pesada(candidatos: set[int], izquierda: set[int],
                     derecha: set[int]) -> tuple[str, set[int]]:
    """Agent Glip decides the outcome of the weighing adversarially.

    The stowaway isn't fixed in advance: Glip picks the reading (balanced, left,
    or right) that keeps the largest subgroup of suspects alive, punishing any
    split that isn't into equal thirds.
    """
    en_izquierda = candidatos & izquierda
    en_derecha = candidatos & derecha
    en_ninguno = candidatos - izquierda - derecha

    mas_grande = max(len(en_ninguno), len(en_izquierda), len(en_derecha))
    if len(en_ninguno) == mas_grande:
        return 'balanced', en_ninguno
    if len(en_izquierda) == mas_grande:
        return 'left', en_izquierda
    return 'right', en_derecha


def mostrar_resultado(resultado: str) -> None:
    if resultado == 'balanced':
        print("⚖️  The scale stays balanced.")
    elif resultado == 'left':
        print("⚖️  The LEFT pan is heavier.")
    else:
        print("⚖️  The RIGHT pan is heavier.")


def pedir_adivinanza() -> int:
    while True:
        try:
            huevo = int(input(f"Which of the {TOTAL_HUEVOS} eggs is the stowaway? "))
        except ValueError:
            print("Enter a valid number.")
            continue
        if 1 <= huevo <= TOTAL_HUEVOS:
            return huevo
        print(f"Choose an egg between 1 and {TOTAL_HUEVOS}.")


def jugar_nivel_6() -> str:
    """Play the complete Level 6. Returns 'Stella' or 'Customs' depending on who wins."""
    print("=== Level 6: Galactic Customs ===")
    print(f"Agent Glip: 'Among these {TOTAL_HUEVOS} meteor eggs hides a heavier stowaway.'")
    print(f"Your weight scanner only has {USOS_BATERIA} uses before the battery dies.\n")

    candidatos = set(range(1, TOTAL_HUEVOS + 1))
    usos_restantes = USOS_BATERIA

    while usos_restantes > 0 and len(candidatos) > 1:
        print(f"\n{usos_restantes} battery uses and {len(candidatos)} suspects left.")
        izquierda, derecha = pedir_pesada()
        resultado, candidatos = resolver_pesada(candidatos, izquierda, derecha)
        mostrar_resultado(resultado)
        usos_restantes -= 1

    print(f"\nThe battery is dead (or there's no doubt left): {len(candidatos)} suspect(s) standing.")
    adivinanza = pedir_adivinanza()

    acierto = len(candidatos) == 1 and adivinanza in candidatos
    if acierto:
        print("\nCorrect! Agent Glip opens the way into the restricted zone.")
        return 'Stella'

    print(f"\nIncorrect. The stowaway was never pinned down with certainty (you still had {len(candidatos)} suspects).")
    print("Agent Glip: 'Come back when you can corner it with absolute certainty.'")
    return 'Customs'


if __name__ == '__main__':
    jugar_nivel_6()
