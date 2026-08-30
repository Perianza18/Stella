# Nivel 3 — El Coleccionista (Teoría de Juegos — Nim 2D/Chomp)

## Gancho narrativo
El nómada la envía a La Bóveda: la colección más grande de objetos raros del universo conocido. El Coleccionista ha catalogado todo lo que el espacio ha arrojado — incluyendo objetos recuperados de anomalías. Podría tener la roca de Stella, o al menos saber dónde fue a parar. Pero no habla con nadie sin ganarle primero su duelo gravitacional.

## Contexto del nivel
En la gigantesca "Bóveda", Stella confronta a "El Coleccionista", un ser cósmico arrogante que retiene su Piedra Perfecta dentro de una Matriz de Contención holográfica. Ambos se la disputan usando rayos tractores en un duelo gravitacional.

## Overview
El enfrentamiento contra el jefe final. Un problema de Teoría de Juegos Combinatoria (Juego de la Torre) donde se mueve una ficha en una cuadrícula aplicando conceptos avanzados de posiciones ganadoras.

## Reglas
- El escenario es una cuadrícula (ej. 10x10) con la cápsula de Stella en la coordenada (0,0).
- La piedra inicia en una coordenada asimétrica superior (como 7, 10).
- En su turno, el jugador desliza la piedra cualquier cantidad de casillas solo hacia la izquierda o hacia abajo.
- Gana quien logre meter la piedra exactamente en (0,0).

## Estrategia
La heurística es mantener la diagonal principal (donde x=y). Si la piedra inicia en (7, 10), Stella la baja a (7, 7). Cuando el bot la saca de la diagonal moviéndola a la izquierda (ej. 2, 7), Stella copia el movimiento hacia abajo para devolverla a la diagonal (2, 2). Esto fuerza al bot a dejarla eventualmente en (0, X) o (X, 0), permitiendo a Stella ganar en su siguiente turno.

## Por qué funciona
Transforma coordenadas abstractas en un enfrentamiento mecánico y visual. El código del Coleccionista detecta cualquier falla inmediatamente y ejecuta su estrategia implacable, obligando al jugador a entender el patrón geométrico.
