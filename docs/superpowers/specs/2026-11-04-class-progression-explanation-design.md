# Class Progression Explanation and Selection Design

## Intent

Make Class Progression rewards as easy to understand and identify as Training Paths. Players should immediately know what the reward does, why it is available, and which choice is currently selected.

## Approved approach

Use the existing Training Path visual language. When a character has a pending Class Progression, show an always-visible bordered explanation above the choices. A selected progression displays `✓ SELECTED`, a bright teal fill, cyan border, white text, and the existing gold keyboard-focus outline. Each button continues to show the progression name and its exact gameplay effect.

The preparation and reward content scrolls vertically when both explanation panels, acquired abilities, choices, and confirmation controls exceed the available height.

## Copy requirements

The explanation states that Class Progressions are permanent rewards for completing a mission with a character for the first time, that one pending reward must be selected and confirmed, and that a tier/version II choice requires the matching tier/version I choice. It distinguishes these rewards from automatic Level Ups.

## Progression data finding

No progression-data change is required. `CampaignState.AwardXp` updates persistent level and stat bonuses immediately; `BattleState.Award` mirrors those changes onto the active unit; battle and preparation cards read those updated values. Defeat rolls the campaign progression back, and veteran mission replays restore permanent progression to the pre-mission snapshot.

## Alternatives considered

- Always-visible panel and matching selected styling — chosen for consistency and readability.
- Short inline helper text — rejected because it would be easier to overlook.
- Modal explanation — rejected because it would interrupt reward selection.

