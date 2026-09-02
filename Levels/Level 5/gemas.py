"""
Stella
Level 5 - Las 200 Gemas
"""
from __future__ import annotations

GEMAS_INICIALES = 200


def calcular_posiciones_perdedoras(maximo: int) -> list[bool]:
    """Para cada cantidad de gemas restantes de 0 a maximo, indica si el jugador en
    turno pierde jugando ambos de forma óptima (posición perdedora / "número maldito").
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
    """Máximo de gemas que se pueden tomar: la mitad redondeada hacia abajo,
    salvo que solo quede 1 gema, en cuyo caso esa última sí se puede tomar."""
    return max(1, restantes // 2)


def movimiento_optimo(restantes: int) -> int:
    """Cuántas gemas toma el Guardián jugando sin errores matemáticos."""
    limite = limite_de_turno(restantes)
    for k in range(1, limite + 1):
        resto = restantes - k
        if resto == 0 or ES_PERDEDORA[resto]:
            return k
    return 1  # el Guardián ya está en una posición perdida; cualquier jugada da igual


def pedir_cantidad(restantes: int) -> int:
    limite = limite_de_turno(restantes)
    while True:
        try:
            cantidad = int(input(f"Quedan {restantes} gemas. ¿Cuántas tomas? (1-{limite}): "))
            if 1 <= cantidad <= limite:
                return cantidad
        except ValueError:
            pass
        print(f"Elige un número entre 1 y {limite}.")


def jugar_ronda() -> str:
    """Juega una ronda completa. Devuelve 'Stella' o 'Guardian' según quién gane."""
    restantes = GEMAS_INICIALES

    while True:
        cantidad = pedir_cantidad(restantes)
        restantes -= cantidad
        print(f"Stella toma {cantidad}. Quedan {restantes} gemas.")
        if restantes == 0:
            print("\n¡Stella toma la última gema! El Guardián entrega el mapa estelar.")
            return 'Stella'

        cantidad_guardian = movimiento_optimo(restantes)
        restantes -= cantidad_guardian
        print(f"El Guardián toma {cantidad_guardian}. Quedan {restantes} gemas.")
        if restantes == 0:
            print("\nEl Guardián toma la última gema. Esta ronda es suya.")
            return 'Guardian'


def jugar_nivel_5() -> None:
    print("=== Nivel 5: Las 200 Gemas ===")
    print("El Guardián del Templo reta a Stella: quien tome la última gema se lleva el mapa estelar.")
    print(f"Hay {GEMAS_INICIALES} gemas. En cada turno puedes tomar entre 1 y la mitad de lo que quede.\n")

    while True:
        jugar_ronda()
        otra = input("\n¿Otra ronda? (s/n): ").strip().lower()
        if otra != 's':
            return
        print()


if __name__ == '__main__':
    jugar_nivel_5()
