# Turn Flow Consistency Design

## Intent

Make preparation and battle turns behave consistently so the player always understands whose choices are complete and what an inactive enemy actually did.

## Behavior

- Confirming a character on either the pre-mission preparation screen or the post-mission Class Rewards screen selects the next unconfirmed deployed character.
- Switching away from a summoned or reprogrammed machine after it has moved or acted completes that machine's turn with Wait. An untouched machine remains available. Existing hero behavior remains unchanged: acting then switching waits, while moving alone can still be undone.
- An enemy that cannot move or attack logs and displays that it “waits” instead of claiming it “acts” or “advances.”

## Approach

Centralize preparation navigation in `CrewPreparationState`, extend the existing selection-change rule for player-controlled machine roles, and make enemy AI emit an explicit Wait log whenever its chosen movement leaves it in place. This keeps UI copy driven by battle-state facts instead of duplicating game rules in the view.

Alternatives considered were UI-only special cases and a larger enemy-action result object. UI-only checks would duplicate rules, while a new result model is disproportionate for these three targeted consistency fixes.

## Constraints

- Preserve the existing ability for a hero who only moved to undo that move.
- Do not grant XP or other rewards for automatic Wait.
- Keep deterministic combat and existing enemy pacing unchanged.
