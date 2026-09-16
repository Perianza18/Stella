"""
Stella
Level 5 - The 200 Gems
"""
from __future__ import annotations

GEMAS_INICIALES = 200


def calcular_posiciones_perdedoras(maximo: int) -> list[bool]:
    """For each amount of gems remaining from 0 to maximo, mark whether the player to
    move loses under optimal play from both sides (losing position / "cursed number").
    """
    es_perdedora = [False] * (maximo + 1)
    for n in range(1, maximo + 1):
        limite = max(1, n // 2)
        gana = False
        for k in range(1, limite + 1):
            resto = n - k
            if resto == 0 or es_perdedora[resto]:
                gana = True
                break
        es_perdedora[n] = not gana
    return es_perdedora


ES_PERDEDORA = calcular_posiciones_perdedoras(GEMAS_INICIALES)


def limite_de_turno(restantes: int) -> int:
    """Maximum gems that can be taken: half, rounded down, unless only 1 gem is
    left, in which case that last one can be taken."""
    return max(1, restantes // 2)


def movimiento_optimo(restantes: int) -> int:
    """How many gems the Guardian takes, playing without any mathematical mistakes."""
    limite = limite_de_turno(restantes)
    for k in range(1, limite + 1):
        resto = restantes - k
        if resto == 0 or ES_PERDEDORA[resto]:
            return k
    return 1  # the Guardian is already in a lost position; any move is equally bad


def pedir_cantidad(restantes: int) -> int:
    limite = limite_de_turno(restantes)
    while True:
        try:
            cantidad = int(input(f"{restantes} gems left. How many do you take? (1-{limite}): "))
            if 1 <= cantidad <= limite:
                return cantidad
        except ValueError:
            pass
        print(f"Choose a number between 1 and {limite}.")


def jugar_ronda() -> str:
    """Play one complete round. Returns 'Stella' or 'Guardian' depending on who wins."""
    restantes = GEMAS_INICIALES

    while True:
        cantidad = pedir_cantidad(restantes)
        restantes -= cantidad
        print(f"Stella takes {cantidad}. {restantes} gems left.")
        if restantes == 0:
            print("\nStella takes the last gem! The Guardian hands over the star map.")
            return 'Stella'

        cantidad_guardian = movimiento_optimo(restantes)
        restantes -= cantidad_guardian
        print(f"The Guardian takes {cantidad_guardian}. {restantes} gems left.")
        if restantes == 0:
            print("\nThe Guardian takes the last gem. This round is his.")
            return 'Guardian'


def jugar_nivel_5() -> None:
    print("=== Level 5: The 200 Gems ===")
    print("The Temple Guardian challenges Stella: whoever takes the last gem gets the star map.")
    print(f"There are {GEMAS_INICIALES} gems. Each turn you can take between 1 and half of what's left.\n")

    while True:
        jugar_ronda()
        otra = input("\nAnother round? (y/n): ").strip().lower()
        if otra != 'y':
            return
        print()


if __name__ == '__main__':
    jugar_nivel_5()
