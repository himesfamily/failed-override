# Mission 3 Crew Selection and Captain Gershon Design

## Intent

Starting with Mission 3, choosing the squad should become an explicit and understandable part of mission preparation. The player chooses exactly four named crew members from the characters currently available, then reviews their Training Paths and unresolved Class Progressions before the mission begins.

Mission 3 permits any four available characters. Later missions may declare one or more required story characters while allowing the player to fill the remaining slots.

Captain Jeane Gershon becomes a playable, deliberately powerful command character. `Gershon` is the canonical player-facing spelling and replaces the older `Gerson` spelling throughout the game. Dr. Chandry Perra is also selectable alongside Xander, Nance, Akiko, and Issac.

## Goals

- Insert a dedicated **SELECT YOUR CREW** screen before the existing preparation screen for Mission 3 and later missions.
- Present all available named crew as clear portrait cards with class, level, core stats, and selection state.
- Require exactly four selected characters for Mission 3.
- Support future mission-specific required characters without redesigning the screen.
- Make Captain Gershon a complete playable character with weapons, a command action, progression, campaign persistence, board artwork, and combat presentation.
- Preserve existing saves and Dr. Chandry's existing assets/progression by retaining her internal `chrandy` identifier while using the requested display spelling.

## Non-goals

- Missions 1 and 2 retain their fixed story rosters and go directly to the existing preparation screen.
- This feature does not rebalance the existing five playable characters.
- Machine-specific command abilities for Akiko and Issac are deferred.
- Captain Gershon's command ability cannot target summoned or reprogrammed robots in this version.
- The roster size remains four; no formation or deployment-position selection is added.

## Approaches Considered

### Dedicated roster screen — selected

A separate portrait-card screen makes squad selection a distinct decision, scales to a larger cast, and keeps it from being confused with Training Paths and Class Progressions.

### Extend Crew Preparation

This would require less UI code, but the existing character tabs and deployment switches make roster state easy to miss and mix two different decisions together.

### Compact selection dialog

This would be fast, but it would not show enough character information to support an informed choice.

## Player Flow

1. The player selects Mission 3 from the campaign flow or mission menu.
2. The new **SELECT YOUR CREW** screen opens.
3. Six cards are displayed: Xander, Nance, Akiko, Issac, Dr. Chandry, and Captain Gershon.
4. Clicking a card toggles it between selected and unselected. Selected cards use a bright cyan/gold border, a visible checkmark, and `SELECTED` text rather than relying on a dark tint.
5. A persistent counter reads `CREW SELECTED 0 / 4` through `CREW SELECTED 4 / 4`.
6. Once four valid characters are selected, **Continue to Preparation** becomes enabled.
7. The existing Crew Preparation screen opens for only the selected four characters, where the player may change Training Paths, review abilities, and resolve pending Class Progressions.
8. **Begin Mission** starts Mission 3 with that roster, after its existing cinematic.
9. Returning from preparation to roster selection preserves the current draft choices. Campaign data is not mutated until existing confirmation rules apply.

For a future mission with required characters, required cards start selected, display `REQUIRED`, and cannot be deselected. They count toward the four-character limit.

## Roster Screen Layout

- Full-screen panel using the established navy, cyan, and gold visual language.
- Header: `SELECT YOUR CREW` and the mission number/name.
- Short instruction: `Choose 4 crew members for this mission.`
- A responsive two-row grid of character cards, designed for the game's supported window sizes.
- Each card contains portrait, full name, class, level, Health, Armor, Movement, attack summary, and a short role description.
- Footer contains the selection counter, Back when a valid prior destination exists, and **Continue to Preparation**.
- Selection must be readable through border, checkmark, and text so it is not dependent on color alone.
- When four are selected, unselected cards remain readable but cannot be selected until another card is removed; a short message explains the four-character maximum.

## Available Crew and Mission Rules

- Mission 1: fixed Xander, Nance, and Akiko.
- Mission 2: fixed Xander, Nance, Akiko, and Issac.
- Mission 3: choose exactly four of Xander, Nance, Akiko, Issac, Dr. Chandry, and Captain Gershon; none are required.
- Campaign roster validation owns availability, exact roster size, uniqueness, and required-character rules. UI state uses the same validation rather than duplicating rules.
- Mission 3 receives four neutral player spawn cells. Selected characters are assigned to those cells in roster order; the map no longer depends on a fixed token for every possible crew member.

## Captain Jeane Gershon

### Base profile

- Internal ID: `jeane`
- Canonical display name: `Captain Jeane Gershon`; existing `jeane` IDs and `jeane-gerson-*` asset filenames remain unchanged for compatibility.
- Class/role: Captain
- Health: 24
- Armor: 2
- Movement: 6
- Pulse rifle: power 8, range 4
- Officer's blade: power 10, range 1

Gershon uses her existing portrait and receives a distinct board token adapted from that portrait, with command-class framing that matches the established token style. Her profile, board token, combat portrait, hover text, and action descriptions must all be wired into the same asset registries used by the other named crew.

### Issue Order

- Once per mission at base level.
- Costs Gershon's action.
- Targets a living, uncaptured, named allied crew member within two grid spaces.
- Cannot target Gershon herself.
- Cannot target summoned robots, reprogrammed robots, or any other non-named unit.
- The target must already have moved or acted during the current player phase.
- Restores both movement and action, giving that character a complete additional turn.
- Clears any stored movement-undo origin/path for the refreshed character so an old move cannot be undone after the order.
- Produces a visible command effect, sound/feedback cue if an appropriate existing cue is available, and a clear combat-log message.
- The UI disables the action after its mission use and explains why no target is currently legal.
- Captain progression usage resets when a mission begins and rolls back with other mission state after defeat.

### Captain Class Progressions

Three paths follow the existing tier/prerequisite rules:

#### Command

- **Command I:** Issue Order range increases from 2 to 3.
- **Command II:** Issue Order gains a second use per mission. After the first use, two complete subsequent player rounds must pass before the second use becomes available. For example, an order used in round 1 is unavailable in rounds 2 and 3 and becomes available in round 4.

#### Sharpshooter

- **Sharpshooter I:** Pulse rifle range increases from 4 to 5.
- **Sharpshooter II:** Pulse-rifle attacks ignore 1 point of target armor.

#### Duelist

- **Duelist I:** Officer's blade power increases from 10 to 12.
- **Duelist II:** Defeating an adjacent enemy with the officer's blade restores Gershon's movement once for that action, following the existing melee-mobility pattern.

## Dr. Chandry Compatibility

- Player-facing text uses `Dr. Chandry Perra`.
- The existing internal ID `chrandy`, progression IDs, save fields, portrait paths, and token paths remain unchanged to protect existing campaigns.
- This is a display-name correction, not a save migration.
- Her existing portrait is adapted into a refreshed board token with medic-class framing, using the same treatment as Gershon rather than generating a new interpretation of the character.

## Captain Name Compatibility

- All player-facing `Gerson` text becomes `Gershon`, including cinematics, tutorial dialogue, roster/profile UI, and test expectations.
- The internal character ID remains `jeane` and existing resource filenames are not renamed, avoiding save or import churn.

## Campaign and Progression Integration

- Gershon is added to Mission 3+ available crew and the progression catalog.
- Her Training Path, XP, levels, mission credit, Class Progressions, and bonds use the same generic campaign structures as other named crew.
- Campaign application/recording treats the Captain as a named playable character; this should be expressed through a shared named-crew rule rather than a growing list of role exceptions.
- First-time Mission 3 completion with Gershon grants the normal pending Class Progression. Replaying Mission 3 with the same character grants no duplicate permanent reward, consistent with existing rules.
- Reserve catch-up XP includes Gershon once she is available, following the existing campaign rules.

## Battle and Mission Integration

- Mission 3 defeat remains based on all deployed combat-capable named crew being defeated, not on the absence of any particular character.
- Objectives and tutorial/help text must not assume that Issac, Akiko, Nance, or Chandry is present.
- Actions that require an absent specialist simply do not appear.
- Enemy targeting, bonds, XP attribution, turn completion, automatic wait behavior, and combat presentation treat Gershon like other named player characters.
- Mission 3's cinematic may show the wider cast for story purposes; cinematic attendance does not force roster deployment.

## Testing and Acceptance

- Roster-state tests cover selecting, deselecting, exactly-four enforcement, required cards, duplicate rejection, unavailable IDs, and draft preservation.
- Campaign tests cover Mission 3's six available characters, any valid four, and future required-character rules.
- Battle tests construct Mission 3 with multiple different valid rosters and confirm four distinct spawn cells.
- Captain tests cover stats, both weapons, legal and illegal Issue Order targets, once-per-mission use, complete turn refresh, undo-state clearing, range, cooldown/second use progression, and defeat rollback.
- Progression tests cover all six Captain progression choices and prerequisites.
- UI/presentation tests cover clear selection labels, four-character counter, disabled Continue, required labels, and displayed Chandry spelling.
- Existing logic tests remain green, the Godot C# project builds, and a packaged macOS app is smoke-tested after extraction.

## Builder Decisions

- Exact card dimensions and responsive breakpoints may be adjusted to fit the existing 1400×1050 base layout as long as all six cards and the footer remain comfortably usable.
- Reuse an existing non-conflicting command/UI sound if suitable; otherwise ship the visual/log feedback without introducing a temporary low-quality sound.
- Implement the roster screen as separate state and view classes, with BattleView coordinating transitions, so selection rules remain testable without Godot rendering.
