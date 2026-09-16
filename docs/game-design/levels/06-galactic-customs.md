# Level 6 — Galactic Customs (Ternary Search)

## Narrative hook
The Guardian has exact coordinates. The rock is in a restricted zone — a border sector that requires authorization to enter. The customs satellite stops her: Agent Glip needs Stella to solve a smuggling problem before letting her through. Stella solves it. Customs opens up.

On the other side: her rock. Exactly where the anomaly left it.

## Level context
On a cold, bureaucratic satellite, Agent Glip stops Stella on her journey. Among 27 identical "meteor eggs," a slightly heavier space stowaway is hiding. Stella must find it using a malfunctioning weight scanner running on a nearly dead battery.

## Overview
A solitaire information-theory puzzle where the player must isolate a heavier item among 27 options using a scale a maximum of 3 times.

## Rules
- The player groups the eggs by clicking and assigns them to the interactive scale.
- Each weighing consumes 1 of the 3 battery uses.
- Once the uses run out, the player must pick the infected egg.
- Dynamic Adversary (Anti-luck): the stowaway isn't preassigned. The system evaluates the player's choices and places the heavy egg in the largest remaining subgroup to prevent wins by chance.

## Strategy
Always split into three equal groups, taking advantage of the fact that 3³ = 27.
- Weighing 1: weigh 9 against 9, leaving 9 aside, to identify which group has it.
- Weighing 2: with those 9 suspects, weigh 3 against 3, leaving 3 aside.
- Weighing 3: of the remaining 3, weigh 1 against 1, leaving 1 aside, revealing the stowaway with 100% certainty.

## Why it works
It translates discrete mathematics in an intuitive way and shuts down trial-and-error attempts. The system actively punishes suboptimal strategies like binary search (splitting in half), forcing the user to discover that dividing into three maximizes information.
