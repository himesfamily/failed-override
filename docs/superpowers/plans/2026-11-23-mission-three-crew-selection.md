# Mission 3 Crew Selection Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a dedicated four-person roster screen for Mission 3+, make Captain Jeane Gershon and Dr. Chandry selectable, and implement Gershon as a complete command-class combatant.

**Architecture:** Put roster rules in a Godot-independent `CrewSelectionState`, keep campaign availability/required-character policy in `CampaignState`, and let `BattleView` coordinate the roster → preparation → cinematic → battle flow. Create named crew from a shared catalog and deploy them into mission-defined player spawn cells; Captain combat rules remain in `BattleState` and progression rules in the existing catalog.

**Tech Stack:** Godot 4.7.2 C#, .NET 8 game project, .NET 10 logic test harness, native Godot Controls, existing PNG portrait assets.

**Spec:** `docs/superpowers/specs/2026-11-23-mission-three-crew-selection-design.md`

## Global Constraints

- Mission 1 and Mission 2 keep their fixed story rosters and do not show the new roster screen.
- Mission 3 requires exactly four unique characters chosen from Xander, Nance, Akiko, Issac, Dr. Chandry, and Captain Gershon; none is required.
- Future required crew are selected, locked, and counted toward the four-person limit.
- Player-facing spellings are `Captain Jeane Gershon` and `Dr. Chandry Perra`; internal IDs and existing asset filenames remain `jeane`, `chrandy`, and `jeane-gerson-*` for compatibility.
- Issue Order targets only living, uncaptured named crew, never Gershon or machines, and restores both movement and action.
- Base Issue Order has range 2 and one use per mission.
- Command II permits a second use: if the first is used in round 1, it is unavailable in rounds 2–3 and available in round 4.
- No new third-party dependencies.
- Preserve existing campaign saves and progression behavior.

## Review Focus

- A roster draft containing unavailable or duplicate IDs must not enable Continue; covered by Task 2 state tests.
- A future mission whose required crew already fill all four slots must lock every other card without corrupting selection order; covered by Task 2 state tests.
- Reopening roster selection after preparation must preserve the four-character draft without prematurely mutating campaign progression; covered by Task 6 flow tests.
- Issue Order after an ally moved must remove that ally's stale undo route so the ally cannot teleport back; covered by Task 5 battle tests.
- Mission 3 must remain playable when any specialist is absent and must not dereference Issac, Chandry, or Xander; covered by Task 4 roster-construction tests.

---

### Task 1: Shared Named-Crew Catalog and Canonical Names

**Files:**
- Create: `Scripts/Campaign/CrewCatalog.cs`
- Modify: `Scripts/Units/Unit.cs`
- Modify: `Scripts/Campaign/CampaignState.cs`
- Modify: `Scripts/UI/CrewPreparationView.cs`
- Modify: `Scripts/UI/BattleView.cs`
- Modify: `Scripts/UI/OpeningCinematic.cs`
- Modify: `Scripts/UI/MissionOneWrapUp.cs`
- Modify: `Scripts/UI/MissionTwoIntroScript.cs`
- Modify: `Scripts/UI/MissionThreeIntroScript.cs`
- Modify: `Tests/FailedOverride.LogicTests.csproj`
- Test: `Tests/Program.cs`

**Interfaces:**
- Produces: `CrewProfile` record and `CrewCatalog.Profile(string id)`, `CrewCatalog.AvailableIds(int mission)`, and `Unit.NamedCrew`.
- Produces: canonical display names and base combat stats used by Tasks 2–7.

- [ ] **Step 1: Write failing catalog and naming tests**

Add `crew_catalog_has_all_playable_profiles_and_canonical_names` asserting six Mission 3 IDs, Gershon `(24,2,6)`, Chandry/Gershon spellings, and distinct IDs. Add `named_crew_excludes_controlled_machines` asserting the six roles/IDs are named crew while summoned/reprogrammed machines are not. Update dialogue tests to require `Gershon` and reject player-facing `Gerson`/`Chrandy`.

- [ ] **Step 2: Run the focused tests and verify RED**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj`

Expected: compile failure because `CrewCatalog` and `Unit.NamedCrew` do not exist, plus old spelling assertions fail.

- [ ] **Step 3: Implement the shared catalog and canonical text**

Create `CrewProfile(string Id, string Name, string ClassName, Role Role, string Token, int Health, int Armor, int Movement, string RoleSummary)` and `CrewCatalog` entries for all six characters. Add `Role.Captain`, `Unit.NamedCrew`, and replace duplicated preparation profile data with catalog reads. Change all player-facing spellings in battle help/objectives and cinematics; keep IDs, code-only compatibility names, and resource paths unchanged. Captain weapon behavior is deferred to Task 5.

- [ ] **Step 4: Run the logic tests and verify GREEN**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj`

Expected: all tests pass.

- [ ] **Step 5: Commit**

```bash
git add Scripts/Campaign/CrewCatalog.cs Scripts/Units/Unit.cs Scripts/Campaign/CampaignState.cs Scripts/UI/CrewPreparationView.cs Scripts/UI/BattleView.cs Scripts/UI/OpeningCinematic.cs Scripts/UI/MissionOneWrapUp.cs Scripts/UI/MissionTwoIntroScript.cs Scripts/UI/MissionThreeIntroScript.cs Tests
git commit -m "feat: add canonical playable crew catalog"
```

### Task 2: Testable Crew Selection Rules

**Files:**
- Create: `Scripts/UI/CrewSelectionState.cs`
- Create: `Scripts/UI/CrewSelectionPresentation.cs`
- Modify: `Tests/FailedOverride.LogicTests.csproj`
- Test: `Tests/Program.cs`

**Interfaces:**
- Consumes: `CampaignState.AvailableCrew(int)`, `CampaignState.RequiredCrew(int)`, `CampaignState.ValidateRoster(int, IEnumerable<string>)`, and Task 1 catalog.
- Produces: `CrewSelectionState(CampaignState campaign, int mission, IEnumerable<string> selected)`, `SelectedIds`, `Toggle(string id)`, `IsSelected`, `IsRequired`, `CanSelect`, `CanContinue`, `CountLabel`, and presentation labels for Task 3.

- [ ] **Step 1: Write failing selection-state tests**

Add `mission_three_roster_selects_exactly_four_of_six`, `selection_rejects_duplicates_and_unavailable_ids`, `required_crew_are_locked_and_count_toward_limit`, `four_required_crew_lock_optional_cards`, and `selection_order_survives_toggle_and_reopen`. Assert Continue is false at 0–3 and true only for a campaign-valid four.

- [ ] **Step 2: Run the focused tests and verify RED**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj`

Expected: compile failure because `CrewSelectionState` does not exist.

- [ ] **Step 3: Implement state and presentation**

Keep selection order in a list plus membership set. Seed required IDs first, discard unavailable/duplicate seed IDs, cap at four, and expose exact strings `CREW SELECTED {count} / 4`, `SELECTED`, and `REQUIRED`. Do not mutate `CampaignState`.

- [ ] **Step 4: Run logic tests and verify GREEN**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj`

Expected: all tests pass.

- [ ] **Step 5: Commit**

```bash
git add Scripts/UI/CrewSelectionState.cs Scripts/UI/CrewSelectionPresentation.cs Tests
git commit -m "feat: add crew selection rules"
```

### Task 3: Dedicated Select Your Crew Screen

**Files:**
- Create: `Scripts/UI/CrewSelectionView.cs`
- Modify: `Scripts/UI/BattleView.cs`

**Interfaces:**
- Consumes: Task 2 state/presentation and Task 1 profiles.
- Produces: `Open(CampaignState campaign, int mission, IEnumerable<string> selected, IReadOnlyDictionary<string, Texture2D> portraits, Action<IReadOnlyList<string>> onContinue, Action? onBack)` and `Visible` UI blocker integration.

- [ ] **Step 1: Add failing presentation assertions**

In `Tests/Program.cs`, add `crew_selection_copy_is_explicit_and_accessible` asserting the title, instruction, count label, selection label, required label, maximum message, and Continue label.

- [ ] **Step 2: Run logic tests and verify RED**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj`

Expected: failure on missing presentation strings.

- [ ] **Step 3: Implement the Godot screen**

Build a full-screen `PanelContainer` with a responsive 3×2 card grid, portrait, name/class/level/stats/role summary, cyan/gold selected border, checkmark plus text, locked required treatment, persistent count, Back, and disabled Continue until `CanContinue`. At four selected, keep unselected cards readable and disabled with the maximum message.

- [ ] **Step 4: Wire screen blocking without changing mission flow yet**

Instantiate the view in `BattleView._Ready()` and include it in `BattleUiBlocked()`. Load Gershon's existing portrait under `jeane` and retain Chandry's `chrandy` resource path.

- [ ] **Step 5: Build the Godot project**

Run: `dotnet build FailedOverride.csproj --no-restore`

Expected: 0 errors.

- [ ] **Step 6: Commit**

```bash
git add Scripts/UI/CrewSelectionView.cs Scripts/UI/BattleView.cs Tests/Program.cs
git commit -m "feat: add select your crew screen"
```

### Task 4: Dynamic Mission 3 Deployment

**Files:**
- Modify: `Scripts/Battle/MissionDefinition.cs`
- Modify: `Scripts/Battle/BattleState.cs`
- Modify: `Scripts/Campaign/CampaignState.cs`
- Test: `Tests/Program.cs`

**Interfaces:**
- Consumes: Task 1 `CrewCatalog.Profile` and Task 2 campaign roster validation.
- Produces: `MissionDefinition.PlayerSpawns`, catalog-driven named-unit construction, and valid Mission 3 battles for any four-person roster.

- [ ] **Step 1: Write failing dynamic-deployment tests**

Add `mission_three_accepts_any_four_available_named_crew`, exercising at least three rosters including one without Issac and one without Xander. Assert exactly four named player units, four unique `PlayerSpawns`, roster-order placement, and no omitted specialist dereference. Add validation assertions for six available IDs, exactly four, duplicates, unavailable IDs, and Mission 1/2 fixed rosters.

- [ ] **Step 2: Run logic tests and verify RED**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj`

Expected: failures because Gershon is unavailable and Mission 3 is tied to fixed map tokens.

- [ ] **Step 3: Add mission player spawn cells**

Add `IReadOnlyList<Cell> PlayerSpawns` to `MissionDefinition`; define four walkable, non-overlapping Mission 3 cells matching the existing starting area, and remove fixed player tokens from Mission 3 `UnitRows`.

- [ ] **Step 4: Construct selected named crew from the catalog**

In `BattleState`, create selected player units in roster order at `PlayerSpawns`; continue parsing enemy tokens from `UnitRows`. Use `Unit.NamedCrew` in campaign apply/record, XP ownership, defeat, rescue/interact, and mission completion paths where the prior Hero/Officer split would exclude Gershon.

- [ ] **Step 5: Run logic tests and verify GREEN**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj`

Expected: all tests pass.

- [ ] **Step 6: Commit**

```bash
git add Scripts/Battle/MissionDefinition.cs Scripts/Battle/BattleState.cs Scripts/Campaign/CampaignState.cs Tests/Program.cs
git commit -m "feat: deploy selected crew dynamically"
```

### Task 5: Captain Combat Actions and Progressions

**Files:**
- Modify: `Scripts/Units/Unit.cs`
- Modify: `Scripts/Battle/BattleState.cs`
- Modify: `Scripts/Campaign/ClassProgressionCatalog.cs`
- Modify: `Scripts/UI/BattleView.cs`
- Test: `Tests/Program.cs`

**Interfaces:**
- Produces: `BattleState.CanIssueOrder(Unit captain, Unit target)`, `BattleState.IssueOrder(Unit captain, Unit target)`, `IssueOrderUsesRemaining`, and `IssueOrderCooldownRounds` for UI and tests.

- [ ] **Step 1: Write failing Captain profile/weapon tests**

Assert Gershon is Captain with 24 Health, 2 Armor, 6 Movement, Pulse rifle `(8,4)`, and Officer's blade `(10,1)` before progression.

- [ ] **Step 2: Write failing base Issue Order tests**

Assert a range-2 named ally who moved or acted becomes `Moved=false, Acted=false`, Gershon spends her action, one use is consumed, the target's prior `UndoMove` returns false, and a second base use fails. Reject self, fresh ally, range-3 ally, dead/captured ally, summoned robot, and reprogrammed robot.

- [ ] **Step 3: Write failing progression tests**

Assert six catalog entries and prerequisites: Command I range 3; Command II two total uses with rounds 2–3 blocked after a round-1 use and round 4 enabled; Sharpshooter I range 5; Sharpshooter II ignores exactly 1 Armor for Pulse rifle only; Duelist I blade power 12; Duelist II melee defeat restores movement.

- [ ] **Step 4: Run logic tests and verify RED**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj`

Expected: failures because Captain actions/progressions do not exist.

- [ ] **Step 5: Implement Captain state and battle rules**

Track per-mission uses and last-use round in `BattleState`, clear the ordered target's `moveOrigins`/`movePaths`, award 20 XP for a useful Issue Order, and apply progression effects only to the specified weapon/action.

- [ ] **Step 6: Add battle action UI and feedback**

Add `Issue Order` to Gershon's action list, an order-target mode, valid target highlighting, hover help, disabled/unavailable explanation, log text, and an obvious native pulse/line effect between Captain and ally. Reuse an existing suitable UI cue only if it fits.

- [ ] **Step 7: Run logic tests and project build**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj && dotnet build FailedOverride.csproj --no-restore`

Expected: all tests pass; build has 0 errors.

- [ ] **Step 8: Commit**

```bash
git add Scripts/Units/Unit.cs Scripts/Battle/BattleState.cs Scripts/Campaign/ClassProgressionCatalog.cs Scripts/UI/BattleView.cs Tests/Program.cs
git commit -m "feat: make captain gershon playable"
```

### Task 6: Roster → Preparation → Cinematic Flow

**Files:**
- Create: `Scripts/UI/MissionLaunchFlowState.cs`
- Modify: `Scripts/UI/BattleView.cs`
- Modify: `Scripts/UI/CrewPreparationView.cs`
- Modify: `Tests/FailedOverride.LogicTests.csproj`
- Test: `Tests/Program.cs`

**Interfaces:**
- Consumes: Task 2 selection state, Task 3 view, Task 4 dynamic battle construction.
- Produces: a state-tested Mission 3 launch sequence and Back transition preserving the roster draft.

- [ ] **Step 1: Write failing launch-flow tests**

Add `mission_one_and_two_skip_roster_selection`, `mission_three_opens_selection_before_preparation`, `preparation_back_preserves_roster_draft`, and `mission_three_intro_receives_confirmed_four`. Assert campaign Training Paths/Class Progressions are unchanged by roster toggles.

- [ ] **Step 2: Run logic tests and verify RED**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj`

Expected: compile failure because `MissionLaunchFlowState` does not exist.

- [ ] **Step 3: Implement transition state**

Define stages `RosterSelection`, `CrewPreparation`, `Cinematic`, and `Battle`; expose mission/draft roster and guarded transition methods so only a valid four advances for Mission 3.

- [ ] **Step 4: Wire BattleView flow**

Change `PlayMission/OpenPreparation` so Mission 3+ opens roster selection first, Continue opens preparation with only those four, Back returns to selection, and Begin stores the roster before playing the existing Mission 3 intro. Mission 1/2 behavior remains unchanged.

- [ ] **Step 5: Run logic tests and project build**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj && dotnet build FailedOverride.csproj --no-restore`

Expected: all tests pass; 0 build errors.

- [ ] **Step 6: Commit**

```bash
git add Scripts/UI/MissionLaunchFlowState.cs Scripts/UI/BattleView.cs Scripts/UI/CrewPreparationView.cs Tests
git commit -m "feat: add mission roster launch flow"
```

### Task 7: Portrait-Derived Board Tokens and Combat Presentation

**Files:**
- Create: `Assets/BoardTokens/jeane-captain.png`
- Modify: `Assets/BoardTokens/chrandy-medic.png`
- Modify: `Scripts/UI/BattleView.cs`

**Interfaces:**
- Consumes: existing `Assets/Portraits/jeane-gerson-transparent.png` and `Assets/Portraits/dr-chrandy-perra.png`.
- Produces: distinct, portrait-faithful Captain and Medic board tokens registered under `jeane` and `chrandy`.

- [ ] **Step 1: Inspect the two source portraits and existing token style**

Use the local image viewer on both portraits and representative existing tokens before editing.

- [ ] **Step 2: Create portrait-faithful token assets**

Use the image-editing tool with each source portrait: preserve face, hair, skin tone, and costume identity; crop for circular board readability; add command-class framing for Gershon and medical framing for Chandry; keep transparent corners and match existing token dimensions.

- [ ] **Step 3: Inspect generated assets at native size and 54×52 board size**

Reject any token whose character is not recognizable or whose class cue disappears at board scale.

- [ ] **Step 4: Register assets and combat portraits**

Map `jeane` to the Captain token and existing portrait; keep `chrandy` internal mapping while displaying Chandry. Ensure attacks made/received use the correct portrait panel.

- [ ] **Step 5: Import and build**

Run the Godot 4.7.2 .NET editor/headless import, then `dotnet build FailedOverride.csproj --no-restore`.

Expected: both resources import and build has 0 errors.

- [ ] **Step 6: Commit**

```bash
git add Assets/BoardTokens Scripts/UI/BattleView.cs
git commit -m "feat: add captain and medic board tokens"
```

### Task 8: Full Verification and macOS Test Build

**Files:**
- Modify as needed only for defects found by verification.

**Interfaces:**
- Consumes: all prior tasks.
- Produces: a verified local macOS test package in `outputs/`.

- [ ] **Step 1: Run the complete logic suite**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj`

Expected: every test passes.

- [ ] **Step 2: Build the full Godot C# project**

Run: `dotnet build FailedOverride.csproj --no-restore`

Expected: 0 errors; document any environment-only NU1900 warning separately.

- [ ] **Step 3: Run Godot import and smoke checks**

Verify no missing scripts/resources and launch Mission 3 at supported small, default, and fullscreen window sizes. Check six readable cards, exactly-four gating, preparation Back, intro timing, four unique spawns, Gershon attacks/order, and a roster without Issac.

- [ ] **Step 4: Request whole-branch review and fix any Important findings**

Review against the approved spec, with special attention to save compatibility, order cooldown/undo state, absent specialists, and responsive selection cards.

- [ ] **Step 5: Create and verify a clean signed macOS package**

Export the PCK, publish the `osx-arm64` .NET runtime, update the app bundle, ad-hoc sign it, zip with resource forks preserved, extract into a fresh directory, verify `codesign`, verify the managed assemblies exist, and smoke-launch the extracted executable.

- [ ] **Step 6: Record checksums and output path**

Place the final zip under `outputs/` with a versioned crew-selection filename and run `shasum -a 256`.

- [ ] **Step 7: Commit verification fixes, if any**

```bash
git add <verified-fix-files>
git commit -m "fix: address crew selection verification"
```
