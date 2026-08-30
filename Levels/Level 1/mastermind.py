"""
Stella
Level 1 - Doble Cerradura (Mastermind)
"""
from __future__ import annotations
import random

COLORS = ['Rojo', 'Azul', 'Verde', 'Amarillo', 'Morado', 'Naranja']
CODE_LENGTH = 4
MAX_ATTEMPTS = 6


def generar_codigo() -> list[str]:
    """Return a new random secret code of CODE_LENGTH colors (se permiten repetidos)."""
    return [random.choice(COLORS) for _ in range(CODE_LENGTH)]


def evaluar_intento(intento: list[str], codigo: list[str]) -> tuple[int, int]:
    """Compare intento against codigo and return (verdes, amarillos).

    verdes: color y posición correctos.
    amarillos: color correcto, posición incorrecta (sin contar ya los verdes).
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
    print(f"Colores disponibles: {', '.join(COLORS)}")
    while True:
        entrada = input(f"Ingresa {CODE_LENGTH} colores separados por espacio: ").strip()
        colores = [c.capitalize() for c in entrada.split()]
        if len(colores) != CODE_LENGTH:
            print(f"Debes ingresar exactamente {CODE_LENGTH} colores.")
            continue
        if not all(c in COLORS for c in colores):
            print("Uno o más colores no son válidos. Intenta de nuevo.")
            continue
        return colores


def mostrar_feedback(verdes: int, amarillos: int) -> None:
    """Print the pegs earned by a guess, sin reusar los nombres de los colores."""
    grises = CODE_LENGTH - verdes - amarillos
    print(f"🟢 Posición correcta: {verdes}   "
          f"🟡 Color correcto, lugar incorrecto: {amarillos}   "
          f"⚪ No está en el código: {grises}")


def jugar_fase(numero_fase: int) -> bool:
    """Play one lock phase (hasta MAX_ATTEMPTS intentos reales).

    Devuelve True si el jugador abre la cerradura a tiempo.
    """
    codigo = generar_codigo()
    intentos = 0
    proteccion_usada = False

    while intentos < MAX_ATTEMPTS:
        print(f"\n--- Fase {numero_fase} | Intento {intentos + 1}/{MAX_ATTEMPTS} ---")
        intento = pedir_intento()
        verdes, amarillos = evaluar_intento(intento, codigo)

        if verdes == CODE_LENGTH:
            if intentos == 0 and not proteccion_usada:
                print("\n¡Suerte de principiante! El guardia sospecha y cambia el código...")
                codigo = generar_codigo()
                proteccion_usada = True
                continue
            print("\n¡Cerradura abierta!")
            return True

        mostrar_feedback(verdes, amarillos)
        intentos += 1

    print(f"\nSe acabaron los intentos. El código era: {' '.join(codigo)}")
    return False


def jugar_nivel_1() -> None:
    """Jugar el Nivel 1 completo: dos fases consecutivas de doble cerradura."""
    print("=== Nivel 1: Doble Cerradura ===")
    print("Stella debe abrir el sistema de seguridad Zyx-7 para llegar al comerciante.")
    print("🟢 = color y posición correctos   🟡 = color correcto, lugar incorrecto   ⚪ = el color no está en el código\n")

    while True:
        if jugar_fase(1) and jugar_fase(2):
            print("\nLa puerta se abre. Stella puede continuar su búsqueda.")
            return
        print("\nAmbas cerraduras deben resolverse en la misma ronda. Reiniciando desde la Fase 1...\n")


if __name__ == '__main__':
    jugar_nivel_1()
