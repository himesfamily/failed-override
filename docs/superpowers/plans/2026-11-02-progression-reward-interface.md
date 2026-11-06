# Progression and Mission Reward Interface Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the legacy post-mission upgrade flow with visible battle XP feedback, immediate path-based Level Ups, per-character mission credit, and an integrated crew preparation/reward interface.

**Architecture:** Keep persistent progression and per-character mission credit in campaign-owned records. Add engine-independent presentation state for XP animation and queued Level Up notices, then move the large reward/preparation UI out of `BattleView` into a focused Godot control that consumes campaign APIs. Battle actions continue reporting awards through `BattleState`; the UI observes structured award results rather than parsing the combat log.

**Tech Stack:** Godot 4.7.2 Mono, C#, .NET 8 game project, .NET 10 logic-test project

**Spec:** `docs/superpowers/specs/2026-11-02-progression-reward-interface-design.md`

## Global Constraints

- Show Health, Armor, Movement, and Attack in full throughout progression UI.
- Apply Level Ups immediately in battle without asking for a choice.
- Keep Training Path and Class Progression as separate decisions.
- Require an assigned Training Path before confirming a reward card.
- Previously selected Class Progressions are permanent; only pending choices may be resolved.
- Award permanent mission XP and one Class Progression only once per character per mission.
- Preserve existing saves and grandfathered legacy upgrades.
- Remove the old fixed post-Mission-1 upgrade-button flow.

## Review Focus

- One action crossing 100 XP must animate the old value, Level Up, carryover value, and stat award in order.
- A mixed replay roster must commit new characters while rolling veteran characters back without corrupting shared campaign state.
- Closing and reopening preparation must preserve confirmed permanent abilities but not silently lose unresolved rewards.
- Four-character validation must remain correct when required characters consume roster slots.
- Full stat labels and controls must remain usable at every supported window size.

---

### Task 1: Per-Character Mission Credit and Replay Transactions

**Files:**
- Modify: `Scripts/Campaign/CharacterProgression.cs`
- Modify: `Scripts/Campaign/CampaignState.cs`
- Modify: `Scripts/Battle/BattleState.cs`
- Modify: `Tests/Program.cs`

**Interfaces:**
- Produces: `CharacterProgression.CompletedMissionIds`, `CampaignState.HasMissionCredit(string,string)`, and replay-safe battle commit behavior.

- [ ] Add failing tests for first completion, same-roster replay rollback, mixed veteran/new roster commit, eligible reserve catch-up, and save migration with no credit collection.
- [ ] Run `dotnet run --project Tests/FailedOverride.LogicTests.csproj` and confirm the new tests fail.
- [ ] Persist completed mission IDs per character and snapshot mission-start XP/stat totals independently for each deployed character.
- [ ] On victory, commit XP and grant a pending Class Progression only to characters receiving first-time mission credit; restore veteran progression to its pre-mission snapshot.
- [ ] Restrict replay reserve catch-up to characters without mission credit and preserve failure rollback for everyone.
- [ ] Run the logic tests and commit `feat: track progression credit per character`.

### Task 2: Structured XP and Level Up Presentation State

**Files:**
- Create: `Scripts/UI/ProgressionPresentationState.cs`
- Modify: `Scripts/Battle/BattleState.cs`
- Modify: `Tests/FailedOverride.LogicTests.csproj`
- Modify: `Tests/Program.cs`

**Interfaces:**
- Produces: `XpAwardResult`, `BattleState.LastXpResult`, and `ProgressionPresentationState` with `Begin`, `Advance`, `DismissLevelUp`, and queued presentation properties.

- [ ] Add failing tests for +20/+25/+50 labels, animation interpolation, carryover across 100 XP, multiple Level Ups, full stat-name notices, and large frame deltas.
- [ ] Run the targeted logic tests and confirm RED.
- [ ] Return structured old/new XP, reason, recipient, and `LevelUpAward` values from the single battle XP award path.
- [ ] Implement engine-independent animation and click-to-dismiss Level Up queue state.
- [ ] Run all logic tests and commit `feat: present battle experience gains`.

### Task 3: Integrated Battle Character Card

**Files:**
- Create: `Scripts/UI/CharacterProgressCard.cs`
- Modify: `Scripts/UI/BattleView.cs`

**Interfaces:**
- Consumes: `BattleState.LastXpResult` and `ProgressionPresentationState`.
- Produces: reusable compact/expanded character-card rendering methods.

- [ ] Build the compact card with portrait, class, Level, full Health/Armor/Movement/Attack labels, Training Path, numeric XP, and a progress bar.
- [ ] Animate the XP bar only after the current combat/action presentation completes and show the amount plus plain-language reason.
- [ ] Present queued LEVEL UP notices above the card; click anywhere dismisses the current notice and advances the queue.
- [ ] Replace every `Level - 1` weapon-power display with the stored Attack bonus and update support-action text for unlocked class effects.
- [ ] Build the Godot C# project and manually verify card layout at supported window sizes.
- [ ] Commit `feat: add integrated battle progression card`.

### Task 4: Crew Preparation and Mission Reward Screen

**Files:**
- Create: `Scripts/UI/CrewPreparationView.cs`
- Create: `Scripts/UI/CrewPreparationState.cs`
- Modify: `Scripts/UI/BattleView.cs`
- Modify: `Tests/FailedOverride.LogicTests.csproj`
- Modify: `Tests/Program.cs`

**Interfaces:**
- Consumes: campaign roster, Training Path, pending progression, available progression, and per-character mission-credit APIs.
- Produces: `CrewPreparationState` selection/validation and a reusable pre-mission/post-victory Godot screen.

- [ ] Add failing state tests for required Training Path, four-person roster constraints, unresolved deployed rewards, permanent acquired abilities, and review-only veteran cards.
- [ ] Implement state validation and explicit draft/confirm behavior so changes persist only when confirmed.
- [ ] Build the expanded card with portrait, XP bar, full stats, four Training Path choices, acquired abilities, eligible pending Class Progression choices, exact effect preview, and character-specific confirmation text.
- [ ] Add the victory team XP summary and one-card-at-a-time deployed reward flow, distinguishing newly rewarded characters from veterans.
- [ ] Use the same screen before every mission for roster selection, path changes, ability review, and pending-choice resolution.
- [ ] Remove the legacy `upX`, `upN`, `upA`, `ChooseUpgrade`, `upgradeStep`, and `rosterReady` flow.
- [ ] Run tests, build Godot, and commit `feat: add crew preparation and reward flow`.

### Task 5: Persistence, Migration, and End-to-End Flow

**Files:**
- Modify: `Scripts/Campaign/CampaignStorage.cs`
- Modify: `Scripts/Campaign/CampaignState.cs`
- Modify: `Scripts/UI/BattleView.cs`
- Modify: `Tests/Program.cs`

**Interfaces:**
- Consumes: Tasks 1–4.
- Produces: complete new-campaign, continue, victory, replay, and next-mission progression flow.

- [ ] Add migration tests for legacy saves, partially resolved rewards, completed missions without character credit, and round-trip save/load.
- [ ] Migrate old campaign-level completions to safe per-character credit without removing legacy power.
- [ ] Save after confirmed reward/preparation changes and prevent mission start until deployed cards are valid.
- [ ] Verify Mission 1 victory no longer displays legacy upgrade buttons and Mission 3 permits four-of-five crew selection.
- [ ] Run all logic tests, clean C# restore/build, and `git diff --check`.
- [ ] Commit `feat: complete persistent progression reward flow`.

### Task 6: Review and Native macOS Test Build

**Files:**
- Modify only as required by review.
- Use: `build/macos-test-entitlements.plist`

**Interfaces:**
- Produces: reviewed Native macOS DMG in the top-level `outputs` folder.

- [ ] Run a whole-branch review against both progression specifications and fix Critical/Important findings once.
- [ ] Run all logic tests, isolated C# build, asset import, and a headless launch smoke test.
- [ ] Manually smoke-test new campaign, Mission 1 reward flow, Training Path change, replay with a mixed-credit roster, and continue from an existing save.
- [ ] Export using Godot 4.7.2 Mono, re-sign ad hoc without hardened runtime using the C# entitlements, and verify signature plus headless exit code 0.
- [ ] Package a UDF-only compressed DMG, calculate SHA-256, and place it in the top-level `outputs` folder.
