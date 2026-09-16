"""
Stella
Level 1 - Double Lock (Mastermind)
"""
from __future__ import annotations
import random

COLORS = ['Red', 'Blue', 'Green', 'Yellow', 'Purple', 'Orange']
CODE_LENGTH = 4
MAX_ATTEMPTS = 6


def generar_codigo() -> list[str]:
    """Return a new random secret code of CODE_LENGTH colors (repeats are allowed)."""
    return [random.choice(COLORS) for _ in range(CODE_LENGTH)]


def evaluar_intento(intento: list[str], codigo: list[str]) -> tuple[int, int]:
    """Compare intento against codigo and return (verdes, amarillos).

    verdes: correct color and position.
    amarillos: correct color, wrong position (not counting the verdes already found).
    """
    verdes = sum(i == c for i, c in zip(intento, codigo))

    restantes_intento = [i for i, c in zip(intento, codigo) if i != c]
    restantes_codigo = [c for i, c in zip(intento, codigo) if i != c]

    amarillos = 0
    for color in restantes_intento:
        if color in restantes_codigo:
            amarillos += 1
            restantes_codigo.remove(color)

    return verdes, amarillos


def pedir_intento() -> list[str]:
    """Ask the player for a guess of CODE_LENGTH colors, validating the input."""
    print(f"Available colors: {', '.join(COLORS)}")
    while True:
        entrada = input(f"Enter {CODE_LENGTH} colors separated by spaces: ").strip()
        colores = [c.capitalize() for c in entrada.split()]
        if len(colores) != CODE_LENGTH:
            print(f"You must enter exactly {CODE_LENGTH} colors.")
            continue
        if not all(c in COLORS for c in colores):
            print("One or more colors are not valid. Try again.")
            continue
        return colores


def mostrar_feedback(verdes: int, amarillos: int) -> None:
    """Print the pegs earned by a guess, without reusing the color names."""
    grises = CODE_LENGTH - verdes - amarillos
    print(f"🟢 Correct position: {verdes}   "
          f"🟡 Correct color, wrong position: {amarillos}   "
          f"⚪ Not in the code: {grises}")


def jugar_fase(numero_fase: int) -> bool:
    """Play one lock phase (up to MAX_ATTEMPTS real attempts).

    Returns True if the player opens the lock in time.
    """
    codigo = generar_codigo()
    intentos = 0
    proteccion_usada = False

    while intentos < MAX_ATTEMPTS:
        print(f"\n--- Phase {numero_fase} | Attempt {intentos + 1}/{MAX_ATTEMPTS} ---")
        intento = pedir_intento()
        verdes, amarillos = evaluar_intento(intento, codigo)

        if verdes == CODE_LENGTH:
            if intentos == 0 and not proteccion_usada:
                print("\nBeginner's luck! The guard gets suspicious and changes the code...")
                codigo = generar_codigo()
                proteccion_usada = True
                continue
            print("\nLock opened!")
            return True

        mostrar_feedback(verdes, amarillos)
        intentos += 1

    print(f"\nOut of attempts. The code was: {' '.join(codigo)}")
    return False


def jugar_nivel_1() -> None:
    """Play the complete Level 1: two consecutive double-lock phases."""
    print("=== Level 1: Double Lock ===")
    print("Stella must open the Zyx-7 security system to reach the trader.")
    print("🟢 = correct color and position   🟡 = correct color, wrong spot   ⚪ = the color isn't in the code\n")

    while True:
        if jugar_fase(1) and jugar_fase(2):
            print("\nThe door opens. Stella can continue her search.")
            return
        print("\nBoth locks must be solved in the same round. Restarting from Phase 1...\n")


if __name__ == '__main__':
    jugar_nivel_1()
