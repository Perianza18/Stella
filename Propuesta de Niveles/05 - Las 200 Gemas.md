# Nivel 5 — Las 200 Gemas

## Gancho narrativo
Los Krik confirman que objetos de esa anomalía han sido vistos en el sector de Vrex. En ese planeta hay un Guardián anciano que registra todo lo que pasa por esa región del espacio. Si alguien vio la roca de Stella, fue él. Pero el Guardián solo comparte información con quien le gane el juego de las 200 gemas.

## Contexto del nivel
Stella llega al planeta Vrex, donde una antigua civilización de coleccionistas resguarda 200 gemas como ofrenda sagrada. El Guardián del Templo, una entidad anciana, le ofrece un trato: si logra vencerlo en el Juego de las Gemas, le entregará un mapa estelar que revela la ubicación de la Piedra Perfecta.

## Overview
Un desafío matemático de deducción numérica en el que el jugador compite en un juego de sustracción. El objetivo es descubrir de forma empírica una secuencia oculta para acorralar a un bot que juega de manera óptima.

## Reglas
- Sobre el altar hay 200 gemas.
- En cada turno, se puede tomar una cantidad de gemas que va desde 1 hasta la mitad del total restante (redondeando hacia abajo; por ejemplo, si quedan 7 gemas, se pueden tomar hasta 3).
- El jugador toma el primer turno.
- Gana quien logre llevarse la última gema.
- El Guardián juega sin cometer errores matemáticos.

## Estrategia (la lógica detrás para ganar)
La victoria se asegura obligando al oponente a quedarse con una secuencia de "números malditos": 2, 5, 11, 23, 47, 95 y 191. El patrón matemático dicta que cada número es el doble del anterior más uno. Partiendo de 200 gemas, el jugador debe tomar 9 en su primer turno para dejarle 191 al Guardián. Sin importar la cantidad que el bot decida tomar, siempre quedará el margen exacto para que el jugador vuelva a dejarlo en el siguiente número de la secuencia descendente, atrapándolo por completo.

## Por qué funciona
Este nivel eleva la dificultad al requerir experimentación y registro analítico. No existe un patrón visual obvio; el jugador se verá obligado a probar, fallar y anotar en qué números pierde irremediablemente para deducir la fórmula oculta. Empezar en 200 (que no es un número maldito) garantiza que el jugador tenga la ventaja matemática de inicio, pero penaliza cualquier movimiento azaroso cediendo el control definitivo a la IA.
