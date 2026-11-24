# All Enemies Robotic — Design

## Intent

Make Issac's reprogramming ability useful against every enemy in the first three missions without reducing the visual or tactical variety already present.

## Behavior

- Every enemy role used in missions 1–3 is mechanically a machine.
- Existing enemy names, artwork, statistics, weapons, AI, and role-specific behavior remain unchanged.
- Issac may reprogram any adjacent damaged enemy that meets his current health threshold.
- A converted enemy remains its original role and becomes a player-controlled machine.
- Akiko and Issac may repair any converted enemy machine.
- Organic healing remains unavailable to machines.

## Approach

Use one shared machine-role predicate for reprogramming, repair, and related UI eligibility. Do not convert specialized enemies into the generic Droid role, because that would discard their tactical identity.

## Non-goals

- No artwork, animation, AI, balance, or mission-layout changes.
- No changes to the reprogramming health threshold or action cost.

## Added Mission 1 Audio

Use the supplied Wilhelm scream file for Issac's attack beat at the end of the mission 1 opening. The cue plays only on the SYSTEM narration that says Issac screams.
