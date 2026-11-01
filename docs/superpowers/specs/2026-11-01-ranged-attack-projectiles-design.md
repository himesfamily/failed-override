# Ranged Attack Projectile Design

## Intent

Make ranged combat readable directly on the board. Players should immediately see who fired, where the shot traveled, and who was hit before the existing combat portrait appears.

## Goals

- Give hero and enemy ranged attacks visibly different effects.
- Play the board effect before the existing click-to-dismiss combat portrait.
- Preserve the existing damage, sounds, attack rules, and turn flow.
- Keep the effect brief enough that repeated attacks do not make combat feel slow.

## Non-goals

- Melee attacks will not gain projectile effects.
- This change will not alter accuracy, damage, weapon ranges, or AI decisions.
- No new artwork files are required; the effects will be drawn in Godot.

## Chosen approach

Hero ranged attacks use a fast cyan-blue plasma bolt with a short glowing trail and a small impact flash. Enemy ranged attacks use a red beam that briefly connects attacker and target, followed by a red impact flash. This gives the player two clearly different treatments to compare while playing.

Other approaches considered were using the same projectile with team-colored tinting, which would be consistent but less distinctive, and using full sprite-based projectile artwork, which would add asset work without improving this first test enough to justify it.

## Presentation sequence

1. The acting unit remains highlighted on the board.
2. The ranged effect travels or appears from the attacker’s cell center to the target’s cell center.
3. An impact flash appears on the target.
4. The existing attack sound and damage presentation occur in sync with the impact.
5. The existing attacker-versus-target portrait panel appears and remains until the player clicks.

The hero bolt should take roughly 0.25 seconds. The enemy beam should build and hold for roughly 0.4 seconds. Exact timing may be adjusted slightly during testing for clarity.

## Constraints and safeguards

- The effect must use the displayed/animated board positions rather than changing unit positions.
- Input remains blocked during the effect, matching the current combat presentation behavior.
- A missing or unsupported attack type must safely fall back to the current portrait presentation.
- Summoned robots and reprogrammed droids use the hero effect when allied and the enemy effect when hostile.

