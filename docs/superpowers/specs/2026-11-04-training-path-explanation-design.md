# Training Path Explanation Design

## Intent

Players must choose Training Paths before Mission 1, so the preparation screen must explain why the choice matters and make the current choice unmistakable.

## Behavior

- The pre-mission screen always explains how XP, Level Ups, and Training Paths work.
- The explanation states that 100 XP causes an immediate Level Up, every Level Up including Level 2 automatically uses the selected path, paths can change between missions, and previous Level Ups do not change retroactively.
- The explanation warns that an unassigned reserve character can receive a random bonus from catch-up XP.
- A short instruction makes clear that every deployed character needs a selection before the mission can begin.
- The active Training Path uses a bright cyan background and border plus an explicit `✓ SELECTED` label. Unselected paths remain visibly separate from the dark-blue screen.
- The clearer selected state also appears when paths are reviewed between later missions.

## Approach

Keep the explanatory copy and path labels in a small presentation helper that can be logic-tested, while `CrewPreparationView` supplies the native Godot panel and button styling. This avoids adding another dismissible tutorial window.
