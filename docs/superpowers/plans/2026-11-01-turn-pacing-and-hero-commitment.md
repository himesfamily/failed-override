# Turn Pacing and Hero Commitment Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a readable pause-and-preview sequence before each enemy acts and automatically Wait an acted-but-unmoved hero when the player selects another ally.

**Architecture:** Keep the hero commitment rule in `BattleState`, where turn mutations and combat logging already live. Add a small engine-independent `EnemyTurnPacingState` for deterministic one-second intermission and 0.6-second preview timing, then let `BattleView` bind that state to the existing active-unit pulse and enemy presentation pipeline.

**Tech Stack:** Godot 4.7.2, C#, .NET 8 game project, .NET 10 console logic tests

**Spec:** `docs/superpowers/specs/2026-11-01-turn-pacing-and-hero-commitment-design.md`

## Global Constraints

- Preserve current combat rules, AI decisions, movement speed, projectiles, impacts, and click-to-dismiss portraits.
- Inter-enemy pause is exactly 1.0 second; stationary preview highlight is approximately 0.6 seconds.
- Automatic Wait occurs only when selecting a different allied character after the current character acted without moving.
- Do not add a confirmation prompt.
- Preserve automatic end-of-player-phase behavior.

## Review Focus

- Clicking the already-selected hero must not consume movement; Task 1 pins this.
- Switching away before acting or after moving must not incorrectly Wait; Task 1 pins both cases.
- Switching after a non-attack action such as Transform or Heal must Wait identically; Task 1 tests the state flags rather than attack type.
- A large frame delta must still expose at least one preview frame instead of skipping directly to the action; Task 2 pins this.
- Defeated or removed enemies left in the queue must not receive a preview or action; Task 3 filters immediately before preview.

---

### Task 1: Commit the Current Hero When Selection Changes

**Files:**
- Modify: `Scripts/Battle/BattleState.cs`
- Modify: `Scripts/UI/BattleView.cs`
- Test: `Tests/Program.cs`

**Interfaces:**
- Consumes: existing `BattleState.Wait(Unit)` and `BattleState.Command(Unit)`.
- Produces: `public bool WaitForSelectionChange(Unit? current, Unit next)`, returning true only when it applies Wait to `current`.

- [ ] **Step 1: Write the failing selection-change tests**

Add `selection_change_waits_only_for_acted_unmoved_unit` with assertions that a different ally causes an acted/unmoved current unit to become moved, complete, and log `"Xander McRaven waits."`; the same unit, an untouched unit, and a moved-but-unacted unit remain unchanged.

- [ ] **Step 2: Run the logic tests to verify RED**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj`

Expected: compilation fails because `WaitForSelectionChange` does not exist.

- [ ] **Step 3: Implement `WaitForSelectionChange(Unit? current, Unit next)`**

In `BattleState`, require distinct units, a commandable current unit, `current.Acted`, and `!current.Moved`; call the existing `Wait(current)` and return true. Return false for every other state.

- [ ] **Step 4: Run the logic tests to verify GREEN**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj`

Expected: all tests pass.

- [ ] **Step 5: Integrate allied selection in `BattleView`**

Before assigning `selected=u` in the allied-unit click branch, call `b.WaitForSelectionChange(selected,u)`. Keep the normal selection, refresh, and automatic phase-end flow unchanged.

- [ ] **Step 6: Build and commit Task 1**

Run: `dotnet build FailedOverride.sln --no-restore`

Expected: build succeeds with zero errors.

Commit: `feat: commit acted hero when switching units`

### Task 2: Model Enemy Intermission and Preview Timing

**Files:**
- Create: `Scripts/UI/EnemyTurnPacingState.cs`
- Modify: `Tests/FailedOverride.LogicTests.csproj`
- Test: `Tests/Program.cs`

**Interfaces:**
- Consumes: frame delta as `double`.
- Produces: `EnemyTurnPacingPhase { Idle, Intermission, Preview }`; `BeginIntermission()`; `Advance(double delta)`; read-only `Phase`, `PreviewStartedThisFrame`, and `ActionReadyThisFrame`; and `public static Unit? TakeNextLiving(Queue<Unit> enemies)`.

- [ ] **Step 1: Write failing timing tests**

Add `enemy_turn_pacing_waits_then_previews` to assert no preview before 1.0 seconds, preview begins at 1.0 seconds, and action readiness begins after a further 0.6 seconds. Add `enemy_turn_pacing_large_delta_preserves_preview_frame` to assert that crossing the intermission boundary starts Preview but does not report action-ready in the same call.

- [ ] **Step 2: Include the new source path and verify RED**

Add `../Scripts/UI/EnemyTurnPacingState.cs` to `Tests/FailedOverride.LogicTests.csproj`, then run `dotnet run --project Tests/FailedOverride.LogicTests.csproj`.

Expected: compilation fails because the pacing type does not exist.

- [ ] **Step 3: Implement `EnemyTurnPacingState`**

Use exact durations `1.0` and `0.6`. Reset transition flags on every `Advance`; when Intermission finishes, enter Preview and return without consuming remaining delta so at least one preview frame is visible; when Preview finishes, enter Idle and set `ActionReadyThisFrame`.

- [ ] **Step 4: Run tests and commit Task 2**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj`

Expected: all tests pass.

Commit: `feat: model enemy turn pacing`

### Task 3: Integrate Enemy Preview into the Battle Presentation

**Files:**
- Modify: `Scripts/UI/BattleView.cs`
- Test: `Tests/Program.cs`

**Interfaces:**
- Consumes: `EnemyTurnPacingState` from Task 2 and the existing enemy queue/pulse rendering.
- Produces: enemy presentation stage for intermission/preview and a pending enemy that is not passed to `BattleState.Enemy(Unit)` until preview completes.

- [ ] **Step 1: Add a controller-level queue eligibility test**

Add `enemy_turn_pacing_skips_dead_queued_enemies`, asserting `TakeNextLiving` removes a defeated first entry and returns the next living enemy, then returns null once the queue has no living enemies.

- [ ] **Step 2: Verify the new test fails**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj`

Expected: compilation fails because `TakeNextLiving` does not exist.

- [ ] **Step 3: Integrate pacing in `BattleView`**

When enemy phase has a living queued enemy, begin Intermission. At `PreviewStartedThisFrame`, use `TakeNextLiving` to select the next enemy, set it as `activeActingUnit`, keep it stationary, and redraw so the existing active ring pulses. At `ActionReadyThisFrame`, call the existing `BeginEnemyPresentation` for that pending enemy. If no living enemy remains, immediately start the next player phase.

- [ ] **Step 4: Preserve existing presentation boundaries**

After an enemy portrait is dismissed or a non-combat action presentation completes, return to the pacing entry point rather than starting the next enemy directly. Do not change movement segment duration, projectile duration, impact timing, or portrait dismissal.

- [ ] **Step 5: Run complete verification**

Run:

- `dotnet run --project Tests/FailedOverride.LogicTests.csproj`
- clean isolated `dotnet restore` and `dotnet build` with `NuGetAudit=false` if Godot holds its generated build folder
- `git diff --check`

Expected: all tests pass, build has zero errors, and diff check is clean.

- [ ] **Step 6: Manually smoke-test and commit Task 3**

Verify in Godot that each enemy pauses for one second, receives a stationary pulse for about 0.6 seconds, then moves; verify switching away from an acted/unmoved hero logs Wait and completes that hero.

Commit: `feat: pace enemy turns and commit hero selection`

### Task 4: Review and Produce a macOS 26 Test Build

**Files:**
- Modify only if needed by review: files from Tasks 1–3
- Use: `build/macos-test-entitlements.plist`

**Interfaces:**
- Consumes: verified project and the established macOS 26 ad-hoc signing fix.
- Produces: UDF DMG with a non-hardened ad-hoc signature and the four C# runtime entitlements.

- [ ] **Step 1: Run a whole-branch code review**

Review selection edge cases, pacing transitions, input locking, dead-enemy filtering, and interaction with existing projectile/portrait stages. Apply at most one focused fix pass and rerun verification.

- [ ] **Step 2: Export and re-sign a fresh macOS application**

Export with Godot 4.7.2, remove transfer metadata, then re-sign ad hoc without `--options runtime` while preserving `allow-jit`, `allow-unsigned-executable-memory`, `allow-dyld-environment-variables`, and `disable-library-validation`.

- [ ] **Step 3: Verify the launch fix before packaging**

Run the app executable with `--headless --verbose --quit`.

Expected: Godot and .NET initialize, the battle scene loads, and the process exits 0 rather than 137.

- [ ] **Step 4: Package and verify the disk image**

Create a UDF-only compressed DMG, run `hdiutil verify`, verify the app signature from the packaged bundle when mounting is available, and record the SHA-256 checksum.

- [ ] **Step 5: Deliver the test build**

Provide the DMG with instructions to replace the existing Applications copy.
