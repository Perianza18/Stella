# Nivel 4 — El Saludo de los Krik (Teoría de Juegos — Nim)

## Gancho narrativo
El Coleccionista no tiene la roca, pero tiene una pista: la anomalía que vivió Stella tiene firma conocida — ocurre cerca del corredor exterior, territorio Krik. Para cruzarlo, Stella debe enfrentar el ritual de saludo obligatorio. No es negociable. Los Krik no hacen excepciones.

## Contexto del nivel
Stella aterriza en el místico planeta de los Krik. Para obtener direcciones, debe superar el saludo ritual frente a un guardia alto y enigmático. Si pierde, sufre un castigo cómico: un monólogo incomprensible de símbolos.

## Overview
Un duelo de Teoría de Juegos Combinatoria basado en la variante clásica de Nim, donde el jugador usa aritmética modular para vencer a un bot perfecto.

## Reglas
- Hay 20 cristales de energía en un altar.
- Stella decide si inicia o cede el turno.
- Por turnos, apagan 1, 2 o 3 cristales.
- Quien apaga el último cristal gana el derecho a hablar.
- El bot juega de manera óptima y penaliza cualquier error.

## Estrategia
Forzar múltiplos de 4. Al iniciar con 20 cristales, Stella debe ceder el primer turno al Krik. Después, aplica el "complemento a 4": si el bot apaga 1, ella apaga 3; si el bot apaga 2, ella 2; si el bot 3, ella 1. Así garantiza reducir el total de 4 en 4 hasta ganar.

## Por qué funciona
Elimina la suerte al 100%. El sistema penaliza el azar usando lógica modular, forzando al jugador a deducir la heurística por sí mismo.

## Nota de implementación
Ya existe un prototipo de este mecanismo (juego de sustracción por turnos) en `Levels/Level 4/copy_lab3.py`.
