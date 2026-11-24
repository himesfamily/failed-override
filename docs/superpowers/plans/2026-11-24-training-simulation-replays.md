# Training Simulation Replays Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add permanent-progression virtual replays of completed missions with any four canonically unlocked crew, while making participation the only source of progression in every run mode.

**Architecture:** Carry an explicit `MissionRunMode` from launch UI into `BattleState`, and isolate run-specific completion behavior in a pure campaign transaction service. Add a dedicated training mission selector that feeds the existing crew-selection and preparation flow, while the battle/result UI uses the run mode to skip story presentation and expose simulation navigation.

**Tech Stack:** Godot 4 C#, .NET 8, existing console test harness in `Tests/FailedOverride.Tests.csproj`.

**Spec:** `docs/superpowers/specs/2026-11-24-training-simulation-replays-design.md`

## Global Constraints

- Deploy exactly four distinct named crew in a training simulation.
- Only canonically completed missions are selectable for simulation.
- Only crew unlocked by canonical campaign progress are selectable.
- Only deployed characters progress in campaign missions and simulations; reserve catch-up XP is removed.
- Simulation wins never mutate `CompletedMissions` or unlock story content.
- Failed runs restore XP, Level Ups, pending Class Progressions, acquired Class Progressions, and bonds.
- Simulation battles skip intros, wrap-ups, and the Mission 1 tutorial.
- Existing save files remain loadable without destructive migration.
- Summoned and reprogrammed robots continue crediting XP to their named controller.

## Review Focus

- A simulation launched with a mission that is not canonically complete must be rejected rather than silently treated as campaign play (Task 1 test).
- Repeated victories after a character exhausts all catalog progressions must not accumulate unusable pending rewards (Task 2 test).
- Defeat after XP and bond changes must restore both, including characters whose progression record did not exist at launch (Task 2 test).
- Mission 1 simulation with playable Issac must retain a separate captive objective and avoid duplicate unit IDs (Task 2 test).
- Returning from training after Mission 3 must route to the main menu when no next campaign mission exists (Task 4 test).

---

### Task 1: Run Mode, Canonical Unlocks, and Roster Policy

**Files:**
- Create: `Scripts/Campaign/MissionRunMode.cs`
- Create: `Scripts/Campaign/TrainingSimulationPolicy.cs`
- Modify: `Scripts/Campaign/CampaignState.cs`
- Modify: `Scripts/UI/MissionLaunchFlowState.cs`
- Test: `Tests/Program.cs`

**Interfaces:**
- Produces: `enum MissionRunMode { Campaign, TrainingSimulation }`.
- Produces: `TrainingSimulationPolicy.CompletedMissionNumbers(CampaignState)`, `UnlockedCrewIds(CampaignState)`, `ValidateMission(CampaignState, int)`, and `ValidateRoster(CampaignState, int, IEnumerable<string>)`.
- Produces: `MissionLaunchFlowState(int mission, MissionRunMode mode, IEnumerable<string> initial)` with `Mode` and roster selection required for all simulations.

- [ ] **Step 1: Write failing policy tests**

Add tests proving: no completed mission yields no simulator missions; Mission 1 completion yields mission 1 and unlocks Issac; Mission 2 completion yields missions 1–2 and unlocks Chandry and Gershon; an incomplete mission is rejected; simulator rosters require four distinct unlocked crew; and campaign roster validation remains compatible.

- [ ] **Step 2: Run the focused tests and verify RED**

Run: `dotnet run --project Tests/FailedOverride.Tests.csproj`

Expected: FAIL because `MissionRunMode` and `TrainingSimulationPolicy` do not exist.

- [ ] **Step 3: Implement the pure policy and launch context**

Keep canonical `CampaignState.AvailableCrew(int)` for story missions. Derive simulator unlocks from `CompletedMissions`, and make `MissionLaunchFlowState` require roster selection whenever `Mode == TrainingSimulation` or `Mission >= 3`.

- [ ] **Step 4: Run the full test harness and verify GREEN**

Run: `dotnet run --project Tests/FailedOverride.Tests.csproj`

Expected: all tests pass.

- [ ] **Step 5: Commit**

```bash
git add Scripts/Campaign/MissionRunMode.cs Scripts/Campaign/TrainingSimulationPolicy.cs Scripts/Campaign/CampaignState.cs Scripts/UI/MissionLaunchFlowState.cs Tests/Program.cs
git commit -m "feat: define training simulation launch policy"
```

### Task 2: Transactional Progression and Capped Replay Rewards

**Files:**
- Create: `Scripts/Campaign/MissionProgressionTransaction.cs`
- Modify: `Scripts/Campaign/CampaignState.cs`
- Modify: `Scripts/Battle/BattleState.cs`
- Modify: `Scripts/Units/Unit.cs` if a unique captive identity field is required
- Test: `Tests/Program.cs`

**Interfaces:**
- Consumes: `MissionRunMode` and `TrainingSimulationPolicy` from Task 1.
- Produces: `MissionProgressionTransaction(CampaignState campaign, MissionRunMode mode, int mission, IReadOnlyList<string> deployedIds)` with `CommitVictory(IReadOnlyDictionary<string,int> xpGains)` and `Rollback()`.
- Produces: `CampaignState.TryGrantPendingClassProgression(string id) : bool`, capped by remaining catalog rewards.
- Produces: `BattleState(int mission, CampaignState? campaign, IEnumerable<string>? roster, MissionRunMode mode = MissionRunMode.Campaign)` and `RunMode`.

- [ ] **Step 1: Replace replay/catch-up tests with failing transaction tests**

Test that deployed veterans keep full replay XP; undeployed characters receive no campaign or simulation XP; every victorious deployed character receives at most one pending reward when choices remain; exhausted catalogs receive none; simulation wins do not mutate `CompletedMissions`; campaign wins still do; bond growth applies only to deployed pairs; and defeat restores progressions and bonds exactly.

Add a Mission 1 simulation test with Issac in the deployed roster that asserts one playable unit with ID `issac`, one distinct captive objective, and successful rescue.

- [ ] **Step 2: Run the focused tests and verify RED**

Run: `dotnet run --project Tests/FailedOverride.Tests.csproj`

Expected: the old veteran rollback and reserve catch-up behavior fails the new assertions.

- [ ] **Step 3: Implement the transaction and integrate `BattleState`**

Snapshot progressions and bonds at construction. On campaign victory, add the canonical mission completion; on simulation victory, do not. In both modes retain deployed XP, grant one capped pending progression per deployed named character, grow only deployed bonds, and never grant reserve XP. Roll back both snapshots on defeat.

Represent Mission 1's captive with a non-roster identity (for example `simulated-issac-captive`) while preserving its officer role, display name, objective access, and defeat rules.

- [ ] **Step 4: Run the full test harness and verify GREEN**

Run: `dotnet run --project Tests/FailedOverride.Tests.csproj`

Expected: all tests pass.

- [ ] **Step 5: Commit**

```bash
git add Scripts/Campaign/MissionProgressionTransaction.cs Scripts/Campaign/CampaignState.cs Scripts/Battle/BattleState.cs Scripts/Units/Unit.cs Tests/Program.cs
git commit -m "feat: commit simulation progression transactionally"
```

### Task 3: Training Mission Selection Screen

**Files:**
- Create: `Scripts/UI/TrainingMissionSelectionState.cs`
- Create: `Scripts/UI/TrainingMissionSelectionPresentation.cs`
- Create: `Scripts/UI/TrainingMissionSelectionView.cs`
- Modify: `FailedOverride.csproj`
- Test: `Tests/Program.cs`

**Interfaces:**
- Consumes: `TrainingSimulationPolicy.CompletedMissionNumbers` from Task 1.
- Produces: `TrainingMissionSelectionState(CampaignState campaign)` with `Missions`, `CanSelect(int)`, and `Select(int)`.
- Produces: `TrainingMissionSelectionView.Open(CampaignState campaign, Action<int> selected, Action cancelled)`.

- [ ] **Step 1: Write failing state and copy tests**

Test ordered completed-mission cards, rejection of locked/unknown missions, exact heading `TRAINING SIMULATION`, the permanent-reward explanation, objective summaries for Missions 1–3, and empty-state behavior.

- [ ] **Step 2: Run the focused tests and verify RED**

Run: `dotnet run --project Tests/FailedOverride.Tests.csproj`

Expected: FAIL because the training selector types do not exist.

- [ ] **Step 3: Implement the selector state, presentation, and Godot view**

Use the existing cyan/gold panel styling. Each selectable card shows mission number, title, and objective summary; Back returns through the supplied callback.

- [ ] **Step 4: Run tests and compile the game project**

Run: `dotnet run --project Tests/FailedOverride.Tests.csproj && dotnet build FailedOverride.csproj`

Expected: all tests pass and build succeeds with zero errors.

- [ ] **Step 5: Commit**

```bash
git add Scripts/UI/TrainingMissionSelectionState.cs Scripts/UI/TrainingMissionSelectionPresentation.cs Scripts/UI/TrainingMissionSelectionView.cs FailedOverride.csproj Tests/Program.cs
git commit -m "feat: add training mission selector"
```

### Task 4: Simulation-Aware Crew, Battle, and Result Flow

**Files:**
- Modify: `Scripts/UI/CrewSelectionState.cs`
- Modify: `Scripts/UI/CrewSelectionPresentation.cs`
- Modify: `Scripts/UI/CrewSelectionView.cs`
- Modify: `Scripts/UI/CrewPreparationView.cs`
- Modify: `Scripts/UI/BattleView.cs`
- Create: `Scripts/UI/PostMissionNavigationState.cs`
- Test: `Tests/Program.cs`

**Interfaces:**
- Consumes: the run mode, simulation policy, progression transaction, mission selector, and launch state from Tasks 1–3.
- Produces: simulation-aware crew-selection/preparation copy and roster validation.
- Produces: `PostMissionNavigationState(CampaignState campaign, int mission, MissionRunMode mode)` with `CanAdvanceCampaign`, `NextCampaignMission`, `CanOpenTraining`, and button labels.

- [ ] **Step 1: Write failing navigation and presentation tests**

Test that campaign victory offers Advance plus Training; Mission 3 omits Advance; training victory offers Replay Another plus Return to Campaign; training after Mission 3 routes safely to main menu; the landing-screen training action appears only after a canonical completion; crew-selection copy says `SELECT YOUR SIMULATION CREW`; and battle presentation exposes a persistent virtual label.

- [ ] **Step 2: Write failing cinematic/tutorial suppression tests**

Add pure launch-presentation assertions that simulation mode returns false for Mission 1 tutorial, Mission 1/2 wrap-up, and Mission 2/3 intro, while campaign mode preserves existing behavior.

- [ ] **Step 3: Run the focused tests and verify RED**

Run: `dotnet run --project Tests/FailedOverride.Tests.csproj`

Expected: FAIL because navigation and simulation presentation do not exist.

- [ ] **Step 4: Integrate the full UI flow**

Add the persistent landing button and post-victory actions, open the training selector, pass `TrainingSimulation` through crew selection/preparation into `BattleState`, display the simulation HUD label, skip cinematic/tutorial triggers by mode, and route result buttons through `PostMissionNavigationState`.

- [ ] **Step 5: Run tests and compile the game project**

Run: `dotnet run --project Tests/FailedOverride.Tests.csproj && dotnet build FailedOverride.csproj`

Expected: all tests pass and build succeeds with zero errors.

- [ ] **Step 6: Commit**

```bash
git add Scripts/UI/CrewSelectionState.cs Scripts/UI/CrewSelectionPresentation.cs Scripts/UI/CrewSelectionView.cs Scripts/UI/CrewPreparationView.cs Scripts/UI/BattleView.cs Scripts/UI/PostMissionNavigationState.cs Tests/Program.cs
git commit -m "feat: integrate training simulation flow"
```

### Task 5: End-to-End Regression and Native Test Package

**Files:**
- Modify: `README.md`
- Modify: `Tests/Program.cs`
- Output: `outputs/FailedOverride-macOS-v3.26.0-training-simulation.zip`

**Interfaces:**
- Consumes: all prior tasks.
- Produces: documented simulator behavior and a verified macOS package.

- [ ] **Step 1: Add an end-to-end state regression test**

Exercise: canonical Mission 1 completion; simulator availability; Mission 1 simulation with Issac deployed; participant XP retained; undeployed progress unchanged; one capped pending progression; no canonical mission mutation; and defeat rollback in a second run.

- [ ] **Step 2: Run the test first and verify it catches any missing integration**

Run: `dotnet run --project Tests/FailedOverride.Tests.csproj`

Expected: FAIL if any integration contract is incomplete; otherwise temporarily revert one required integration line to demonstrate RED, restore it, and continue.

- [ ] **Step 3: Update README player instructions**

Document the Training Simulation button, full participant rewards, no reserve progression, four-character selection, skipped story scenes, and defeat rollback.

- [ ] **Step 4: Run fresh verification**

Run: `dotnet run --project Tests/FailedOverride.Tests.csproj && dotnet build FailedOverride.csproj`

Expected: all tests pass and build succeeds with zero errors.

- [ ] **Step 5: Export and verify the Native macOS package**

Export a fresh PCK with Godot Mono, combine it with the verified arm64 app bundle and current managed assembly, ad-hoc sign it, archive with `ditto`, extract to a clean temporary directory, run `codesign --verify --deep --strict`, confirm the managed assemblies directory exists, and smoke-launch the extracted executable headlessly.

Expected: all verification commands exit 0 and the ZIP exists at `outputs/FailedOverride-macOS-v3.26.0-training-simulation.zip`.

- [ ] **Step 6: Commit**

```bash
git add README.md Tests/Program.cs
git commit -m "docs: explain training simulation progression"
```
