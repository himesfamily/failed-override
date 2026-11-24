# Briefing-Room Speaker Focus

## Intent

Make later briefing-room cinematics as easy to follow as the early narrative scenes without making anyone appear to leave the group. The player should immediately recognize the current speaker while still seeing the whole crew together.

## Goals

- Keep every character in the standing position shown in `aurora-debriefing-room.png`.
- Fade in a soft cyan contour around the current speaker's hair, body, and clothing.
- Crossfade the contour between speakers when dialogue advances.
- Keep the group image slightly inset so Dr. Chandry's complete figure remains visible on the right.
- Give Emperor Ompok's hologram the same cyan speaker-focus treatment.
- Apply the treatment consistently to the Mission 2 introduction, Mission 2 wrap-up, and Mission 3 introduction.

## Non-Goals

- Do not move characters out of the group composition.
- Do not circle characters or place a `SPEAKING` label over them.
- Do not redraw or replace the approved briefing-room artwork.
- Do not change dialogue, progression, or cinematic timing beyond the short focus crossfade.

## Constraints

- The group artwork is a flattened image, so each speaking character needs a fitted contour mask aligned to their position in that image.
- The dialogue panel must remain readable and must not cover the visible portion of Chandry.
- The effect must scale with the existing 1400 × 1070 cinematic layout.

## Approaches Considered

1. **Foreground portrait:** clearest match to the early cinematics, but makes the speaker look as though they stepped away from the group.
2. **Camera pan/zoom:** preserves the group pose, but risks cropping characters and reducing image quality.
3. **In-place speaker focus:** keeps the full composition and uses a fitted contour around the speaker. This is the selected approach because it preserves the group scene while making dialogue ownership clear.

## Selected Behavior

The briefing-room image remains visible at a safe inset. On every dialogue beat, the scene dims slightly and a cyan contour fades around the speaking character in their existing position. When the speaker changes, the previous contour fades out as the next contour fades in. Ompok remains a separate hologram because he is not physically present, but his hologram receives the same cyan contour and glow language.

## Verification

- Each scripted speaker maps to a valid focus mask or the Ompok hologram.
- Every focus mask stays inside the safe background frame.
- Chandry's right edge stays within the frame at the game's reference resolution.
- Existing cinematic script tests and the complete logic test suite remain green.
- A clean Godot C# build succeeds, followed by a packaged-app launch check before delivery.
