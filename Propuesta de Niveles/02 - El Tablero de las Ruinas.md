# Nivel 2 — El Tablero de las Ruinas

## Gancho narrativo
Los archivos la llevan a un planeta cubierto de ruinas de una civilización extinta. Un nómada alienígena — último guardián de esa memoria — dice haber visto objetos aparecer y desaparecer en anomalías como la de Stella. Conoce a alguien que podría saber más. Pero primero, exige que ella juegue el juego sagrado de los antiguos constructores.

## Contexto del nivel
Stella aterriza en un planeta cubierto por ruinas de una civilización extinta. Allí, un alienígena nómada la desafía al juego sagrado de los antiguos constructores. Se juega sobre un tablero ceremonial de piedra, cuya única regla inquebrantable es respetar la piedra inamovible ubicada en su centro.

## Overview
Un duelo de Teoría de Juegos Combinatoria en una cuadrícula. El jugador se enfrenta a un problema de posiciones simétricas donde la estrategia visual y geométrica es la clave para ganar.

## Reglas
- El tablero es una cuadrícula de 7x7 con la casilla central bloqueada (48 casillas libres en total).
- El alienígena y Stella toman turnos para colocar una pieza de piedra en forma de "L" (que ocupa 4 casillas, similar al movimiento del caballo de ajedrez).
- Las piezas se pueden rotar y reflejar, pero no pueden superponerse ni salirse de los límites del tablero.
- El alienígena siempre coloca la primera pieza. Pierde el jugador que ya no tenga espacio para colocar su pieza.

## Estrategia
La victoria depende de la "Estrategia del Espejo". Como la casilla central está bloqueada, el tablero tiene una simetría perfecta. Al ser el segundo jugador, Stella solo debe responder a cada movimiento del alienígena colocando su pieza en el reflejo exacto rotado a 180° respecto al centro. Si el rival encuentra un espacio válido, la posición simétrica opuesta siempre estará libre, asegurando que Stella nunca se quede sin jugadas.

## Por qué funciona
La estrategia es visual y sumamente elegante, alejándose de los cálculos numéricos. El "aha moment" ocurre cuando el jugador comprende el diseño del nivel: la piedra central no está ahí por accidente; evita que el oponente cruce el centro y rompa el espejo. Una vez asimilada la simetría, la estrategia es infalible y elimina cualquier variable de suerte.
