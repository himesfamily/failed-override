# Turn Pacing and Hero Commitment Design

## Intent

Make enemy turns easier to follow and make acting before moving feel like a deliberate commitment to the currently selected hero.

## Behavior

- After one enemy finishes its full presentation, the game waits one second before introducing the next enemy.
- The next enemy is then selected and pulse-highlighted for about 0.6 seconds while remaining stationary.
- Only after that preview does the enemy begin moving and acting. The existing movement, projectile, impact, and click-to-dismiss combat presentations remain unchanged.
- During the player phase, if the selected hero has acted but has not moved, clicking a different allied character automatically applies **Wait** to the previous hero. Their unused movement is forfeited and their turn becomes complete before the new character is selected.
- Switching away from a hero who has not acted remains free. Switching from a hero whose turn is already complete also remains unchanged.
- Automatic Wait uses the existing battle rule and log feedback so the turn state remains consistent and visible.

## Approaches Considered

- **Automatic Wait on character switch — selected.** Fast, predictable, and matches the player’s deliberate choice to move on.
- **Block character switching.** Clear but creates friction and requires the player to return to the action list.
- **Ask for confirmation.** Safest against accidental clicks but would interrupt the tactical flow too often.

For enemy pacing, a fixed pause plus a separate preview highlight was selected over merely slowing movement. It makes the identity of the next actor clear without making every movement step sluggish.

## Constraints and Non-goals

- Do not change combat rules, enemy AI decisions, attack timing, or movement speed.
- Do not add a confirmation dialog for automatic Wait.
- Do not auto-Wait merely because a hero acts; it occurs only when the player deliberately selects another allied character.
- Preserve automatic end-of-player-phase behavior after all playable units finish.

## Verification

- Logic tests cover automatic Wait only for an acted, unmoved selected hero when switching allies.
- Presentation-state tests cover the one-second inter-enemy delay and 0.6-second preview window.
- Manual verification confirms the preview highlight is visible before enemy movement and that the action log reports the automatic Wait.
