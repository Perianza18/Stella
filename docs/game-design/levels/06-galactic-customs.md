# Level 6 — Galactic Customs

## Narrative Role

The Guardian gives Stella exact coordinates, but the route crosses a restricted border sector. Agent Glip grants passage only after Stella identifies a stowaway hidden among meteor eggs. Beyond customs, Stella finally finds her rock where the anomaly left it.

## Player-Facing Objective

Identify the heavier stowaway among 27 meteor eggs using no more than three scale readings.

## Interaction

For each scale use, the player assigns equal-sized groups of current candidates to the left and right pans, leaves any others aside, and observes whether the scale balances or one side is heavier. After the available readings, the player selects one egg.

## Visual and UI Direction

- Use a close-up customs-station interface with Agent Glip and blue-purple alien technology framing the task, while keeping the weighing surface dominant.
- Place the alien balance on the left and the meteor-egg candidate grid on the right. Use enclosed alien baskets or boxes instead of open scale plates; front-facing LED counters show how many eggs each side contains without requiring every assigned egg to be drawn inside.
- Assign eggs through clicks or taps rather than mandatory dragging. The player selects candidates for the left container, advances with a clear next/confirm control, assigns the right container, then confirms the weighing.
- Dim eggs that have been assigned and mark their left/right group clearly. Prevent weighing until the two containers hold equal numbers, but leave unassigned eggs visibly available as the third group.
- Show the three battery uses as persistent pips. After the third weighing—or once only one candidate remains—the balance powers down and the interface changes to a single-egg selection state.
- Character cards may be omitted if they crowd the 27 candidates. Scale animation, LED counts, and the candidate grid take priority, and no feedback should reveal how the adversarial outcome was chosen.

## Rules

- There are 27 visually identical meteor eggs.
- Exactly one candidate must be identified as the heavier stowaway.
- Each weighing compares equal-sized groups.
- The scale may be used at most three times.
- The stowaway is not assigned to a fixed egg before play.
- After the readings, the player must identify a single egg with certainty.

## Win Condition

Reduce the consistent candidate set to one egg and select it.

## Loss Condition

Use the available readings without isolating one candidate, or select an egg that has not been established uniquely.

## Developer Design

### Hidden Mathematical Idea

Each weighing has three outcomes: left heavier, right heavier, or balanced. Equal three-way partitioning uses all three outcomes efficiently, producing the candidate sequence `27 → 9 → 3 → 1`.

### Optimal Strategy

At each step, divide the current candidates into three equal groups. Compare two groups and retain the heavier pan's group, or retain the unweighed group if the pans balance.

### Bot / System Behaviour

After every weighing, the system compares the three consistent candidate subgroups and returns an outcome retaining the largest one. The candidate set is updated to that subgroup.

### Anti-Luck / Anti-Bruteforce Behaviour

The heavy egg is not predetermined. Adversarial outcome selection prevents a lucky guess or an inefficient split from receiving a favorable hidden placement. Only a strategy that reduces the worst case to one candidate can guarantee success.

### What the Player Is Expected to Discover

The player should discover that every scale reading has three possible outcomes and that balancing those outcomes minimizes the largest remaining candidate group. The player-facing experience should not name the technique.

## Implementation Status

### Python Prototype

Implemented in [`prototypes/python/level_06_galactic_customs/aduana_galactica.py`](../../../prototypes/python/level_06_galactic_customs/aduana_galactica.py), including adversarial candidate retention.

### Unity

Not started. The Unity project has not been created.
