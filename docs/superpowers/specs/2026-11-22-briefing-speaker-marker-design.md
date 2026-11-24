# Briefing Speaker Marker — Design

## Intent

Make the active speaker immediately recognizable in briefing-room cinematics at every supported window size without moving characters out of the group composition.

## Design

- Replace the inaccurate full-body SVG contours with a compact cyan halo around the speaker's head and shoulders.
- Add a downward cyan pointer above the halo.
- Pulse the marker gently while the speaker is active and crossfade when speakers change.
- Anchor marker regions to the original 1672×941 briefing-room artwork, then transform them into the fixed letterboxed display frame.
- Keep Ompok's hologram treatment unchanged.

## Non-goals

- Do not redraw or crop the briefing-room artwork.
- Do not move speakers out of their group positions.
- Do not change dialogue, timing, or cinematic navigation.

## Approach

Use native Godot drawing for the halo and pointer. This is window-independent and avoids maintaining hand-traced silhouette assets that cannot reliably match the overlapping painted figures.
