# Nivel 1 — Doble Cerradura (Mastermind)

## Gancho narrativo
Stella rastrea la ruta que siguió su nave antes de la anomalía. Los registros apuntan a una estación espacial abandonada donde un viejo broker de información llamado Zyx-7 archivó datos de anomalías similares. Si logra abrir la puerta sellada del archivo, sabrá por dónde seguir buscando.

## Contexto del nivel
Stella llega a una estación espacial abandonada buscando a un comerciante alienígena que posee información sobre la Piedra Perfecta. Para llegar a él, debe abrir una puerta sellada por un sistema de seguridad alienígena de doble cerradura (Zyx-7).

## Overview
Un puzzle introductorio de deducción lógica basado en el clásico juego de Mastermind, donde el jugador debe descifrar combinaciones ocultas interpretando retroalimentación visual.

## Reglas
- El jugador visualiza un panel de control con 4 ranuras y 6 botones de gemas de distintos colores.
- El objetivo es adivinar un código secreto de 4 gemas eligiendo entre los 6 colores disponibles.
- Tras cada intento, el sistema responde únicamente con indicadores visuales: un punto verde brillante indica color y posición correctos; amarillo indica color correcto pero mala posición; gris indica que el color no está en el código.
- El jugador tiene 6 intentos por cerradura y debe resolver dos fases consecutivas. Si falla alguna, debe reiniciar desde la primera.

## Estrategia
El objetivo no es adivinar, sino descartar opciones de manera sistemática. El primer intento ideal debe utilizar colores distintos (ej. Rojo-Azul-Verde-Amarillo) para obtener la máxima información posible. Con las respuestas obtenidas, se aplica lógica de eliminación para aislar el código correcto, lo cual garantiza la victoria matemática en 5 intentos o menos.

## Por qué funciona
Es un reto introductorio que fomenta la eliminación sistemática en lugar del azar. Para blindar el juego contra la suerte, cuenta con un mecanismo programado: si el jugador adivina la primera fase en su primer intento, el sistema lo detecta como "suerte de principiante" y genera un nuevo código, obligando al usuario a realizar al menos 2 o 3 intentos de verdadera deducción.
