# Level 1 — Double Lock (Mastermind)

## Narrative hook
Stella traces the route her ship followed before the anomaly. The records point to an abandoned space station where an old information broker named Zyx-7 archived data on similar anomalies. If she can open the sealed archive door, she'll know where to keep searching.

## Level context
Stella arrives at an abandoned space station looking for an alien trader who has information about the Perfect Rock. To reach him, she must open a door sealed by an alien double-lock security system (Zyx-7).

## Overview
An introductory logic-deduction puzzle based on the classic game of Mastermind, where the player must decipher hidden combinations by interpreting visual feedback.

## Rules
- The player sees a control panel with 4 slots and 6 gem-colored buttons.
- The goal is to guess a secret 4-gem code, choosing from the 6 available colors.
- After each guess, the system responds only with visual indicators: a bright green dot means correct color and position; yellow means correct color but wrong position; gray means the color isn't in the code.
- The player has 6 attempts per lock and must clear two consecutive phases. Failing either one means restarting from the first.

## Strategy
The goal isn't to guess, but to eliminate options systematically. The ideal first guess should use distinct colors (e.g. Red-Blue-Green-Yellow) to get the maximum amount of information. Using the responses, elimination logic is applied to isolate the correct code, guaranteeing a mathematical win in 5 attempts or fewer.

## Why it works
It's an introductory challenge that encourages systematic elimination over guesswork. To shield the game against luck, it has a built-in safeguard: if the player guesses phase 1 on their very first attempt, the system detects it as "beginner's luck" and generates a new code, forcing the user to make at least 2-3 attempts of real deduction.
