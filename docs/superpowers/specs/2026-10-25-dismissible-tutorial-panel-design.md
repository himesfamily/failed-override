# Dismissible Tutorial Panel

## Intent

Let players temporarily clear Captain Jeane Gershon's instructional panel when it blocks a useful part of the map, without allowing an accidental click to permanently skip an important lesson.

## Behavior

- While the tutorial panel is visible, clicking anywhere on the panel dismisses it.
- Dismissal hides only the current presentation of the lesson. It does not mark the lesson complete.
- The lesson may appear again on a later tutorial refresh until the player performs the action that completes it.
- Completing the requested action continues to advance the tutorial exactly as it does now.
- Clicking the tutorial panel must not also click or move on the map underneath it.
- The panel should visually indicate that it is clickable with a short “Click to dismiss” prompt and a pointing-hand cursor.

## Approaches Considered

1. Temporarily hide the panel while retaining the active lesson. Chosen because it clears the board without permanently losing guidance.
2. Mark the lesson complete when dismissed. Rejected because a quick accidental click could skip important instructions.

## Scope

This is limited to Captain Gershon's Mission 1 tutorial panel. It does not change tutorial triggers, lesson completion rules, other dialogue windows, or campaign progression.

## Verification

- A focused test will confirm that dismissal hides the presentation but preserves the active lesson.
- Existing gameplay tests and a production C# build must continue to pass.
