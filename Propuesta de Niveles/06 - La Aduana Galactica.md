# Nivel 6 — La Aduana Galáctica (Búsqueda Ternaria)

## Gancho narrativo
El Guardián tiene coordenadas exactas. La roca está en zona restringida — un sector fronterizo que requiere autorización para entrar. El satélite aduanero la detiene: el Agente Glip necesita que Stella resuelva un problema de contrabando antes de dejarla pasar. Stella lo resuelve. La aduana se abre.

Al otro lado: su roca. Exactamente donde la anomalía la dejó.

## Contexto del nivel
En un frío satélite burocrático, el Agente Glip detiene a Stella en su viaje. Entre 27 "huevos-meteorito" idénticos se esconde un polizón espacial ligeramente más pesado. Stella debe hallarlo usando un escáner de peso averiado con batería casi agotada.

## Overview
Un puzzle solitario de teoría de la información donde el jugador debe aislar un elemento más pesado entre 27 opciones usando una balanza un máximo de 3 veces.

## Reglas
- El jugador agrupa los huevos haciendo clic y los asigna a la balanza interactiva.
- Cada pesaje consume 1 de los 3 usos de batería.
- Tras agotar los usos, debe elegir el huevo infectado.
- Adversario Dinámico (Antisuerte): el polizón no está preasignado. El sistema evalúa las elecciones del jugador y sitúa el huevo pesado en el subgrupo más grande restante para evitar victorias por azar.

## Estrategia
Dividir siempre en tres partes iguales aprovechando que 3³ = 27.
- Pesaje 1: se pesan 9 contra 9, dejando 9 fuera para identificar qué grupo lo tiene.
- Pesaje 2: con esos 9 sospechosos, se pesan 3 contra 3, dejando 3 fuera.
- Pesaje 3: de los 3 restantes, se pesa 1 contra 1, dejando 1 fuera, revelando al polizón con un 100% de certeza.

## Por qué funciona
Traduce matemáticas discretas de manera intuitiva y anula intentos de ensayo y error. El sistema castiga activamente estrategias subóptimas como la búsqueda binaria (partir a la mitad), obligando al usuario a descubrir que dividir entre tres maximiza la información.
