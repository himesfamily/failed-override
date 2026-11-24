# Briefing-Room Speaker Portraits

## Intent

Make speakers in the later briefing-room cinematics as easy to identify as speakers in the early story scenes. Preserve the existing briefing-room group artwork and dialogue presentation while eliminating resolution-sensitive outline markers.

## Goals

- Darken the group background slightly during spoken dialogue.
- Draw the active speaker as a separate, full portrait cutout above the background.
- Alternate the portrait between left and right according to a stable per-character placement map, choosing the side that obscures the fewest important background faces.
- Crossfade cleanly when the speaker changes.
- Keep the speaker name, dialogue box, scene title, click-to-continue behavior, transitions, and skip button unchanged.
- Support Captain Jeane Gershon, Issac Wynter, Xander McRaven, Nance Parker, Akiko Ikeda, Dr. Chandry Perra, and Emperor Ompok.
- Scale correctly at every supported window size.

## Non-goals

- Do not regenerate or replace the briefing-room background.
- Do not move characters inside the group artwork.
- Do not alter scripts, dialogue, timing, or cinematic order.
- Do not change the early Mission 1 cinematic treatment.

## Approaches Considered

1. **Fixed left portrait:** closest to a traditional visual novel, but repeatedly hides the same portion of the group.
2. **Alternating portrait sides — selected:** places each character on the clearer side of the composition. This preserves more of the room and gives dialogue a natural back-and-forth rhythm.
3. **Centered portrait:** unmistakable speaker focus, but covers too much of the group scene.

## Presentation

- The briefing-room background receives a consistent dark translucent overlay while dialogue is visible.
- One portrait cutout appears at a time, anchored to the cinematic frame rather than raw screen coordinates.
- Portraits use `KeepAspectCentered` and a safe frame above the dialogue panel, preventing cropping at smaller window sizes.
- A speaker-side map keeps placement predictable. Conversation partners should generally face into the frame when their portrait artwork permits it.
- Speaker changes use a short crossfade; repeated lines from the same speaker reuse the current portrait without flashing.
- Ompok continues to use the hologram presentation, with the background dimming applied consistently.
- The old circles, arrows, and SVG outline markers are removed from these cinematics.

## Verification

- Add layout/state tests for portrait resource selection, left/right placement, repeated-speaker reuse, crossfade state, and Ompok hologram handling.
- Verify Mission 2 introduction, Mission 2 wrap-up, and Mission 3 introduction at small, default, and fullscreen sizes.
- Confirm every scripted speaker has a portrait mapping and no portrait overlaps the dialogue box or extends beyond the cinematic frame.
