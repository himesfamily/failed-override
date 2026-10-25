# Automatic Unit Turn Completion Design

## Intent

When a player-controlled unit has both moved and taken an action, its turn is complete. The player should not need to press **Wait** afterward.

## Goals

- Support both move-then-act and act-then-move.
- Immediately hand control to the next ready crew member after both allowances are used.
- Automatically begin the enemy phase when no ready crew members remain.
- Keep **Wait** available only for voluntarily ending a partially used turn.

## Non-goals

- Do not remove action-first play.
- Do not remove the ability to undo a move before acting.
- Do not change combat, ability, or movement costs.

## Approach

The approved approach is one shared turn-completion policy used after every successful player command. The battle state exposes whether a unit is complete and whether all player units are complete; the UI uses those results for selection, **Wait** visibility, and enemy-phase transition. This replaces scattered combinations of direct `Moved` and `Acted` checks with one consistent rule.

Alternatives considered were patching individual buttons or automatically consuming unused movement after every action. Button-by-button patches would remain fragile, while consuming movement would break action-first-then-move play.

## Constraints

- A unit is complete only when both `Moved` and `Acted` are true.
- Captured or defeated units are not considered ready.
- Existing attack presentation remains visible until the player clicks; turn handoff occurs after dismissal.
