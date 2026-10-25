# Automatic Unit Turn Completion Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Automatically finish and hand off a player unit after it has both moved and acted, without requiring **Wait**.

**Architecture:** `BattleState` will own the definitions of unit completion and whether all controllable player units are finished. `BattleView` will use those shared queries after every command, while preserving delayed handoff for click-dismissed combat presentations.

**Tech Stack:** Godot 4.7.2, C#, .NET logic test executable

**Spec:** `docs/superpowers/specs/2026-10-25-automatic-unit-turn-completion-design.md`

## Global Constraints

- Preserve both move-then-act and act-then-move.
- Preserve move undo before an action.
- Preserve click-to-dismiss attack presentations.
- Keep **Wait** for partially used turns only.

## Review Focus

- Move then act completes the unit.
- Act then move completes the unit.
- A unit that has only moved remains ready and may undo.
- Captured and defeated units do not prevent phase completion.
- The final completed crew member triggers the existing automatic enemy-phase path.

---

### Task 1: Centralize turn-completion rules

**Files:**
- Modify: `Scripts/Battle/BattleState.cs`
- Test: `Tests/Program.cs`

**Interfaces:**
- Produces: `bool TurnComplete(Unit unit)` and `bool PlayerTurnComplete()`

- [ ] **Step 1: Write failing logic tests**

Add tests proving that both action orders produce a completed unit, a move-only unit remains incomplete, and inactive units do not block full player-turn completion.

- [ ] **Step 2: Run the logic test executable and verify RED**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj --no-restore`

Expected: build failure because the new completion methods do not exist.

- [ ] **Step 3: Implement the minimal shared completion queries**

Add `TurnComplete(Unit)` and `PlayerTurnComplete()` to `BattleState`, using the existing `Moved`, `Acted`, alive, team, and captured state.

- [ ] **Step 4: Run the logic tests and verify GREEN**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj --no-restore`

Expected: all tests pass.

### Task 2: Route the battle interface through the shared policy

**Files:**
- Modify: `Scripts/UI/BattleView.cs`

**Interfaces:**
- Consumes: `BattleState.TurnComplete(Unit)` and `BattleState.PlayerTurnComplete()`

- [ ] **Step 1: Replace direct completion checks**

Use the shared policy for crew-ready counts, status copy, **Wait** visibility, next-unit selection, automatic enemy-phase transition, readiness highlighting, and completed-token appearance.

- [ ] **Step 2: Build and run the full suite**

Run: `dotnet build FailedOverride.csproj --no-restore -p:RuntimeIdentifiers=`

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj --no-restore`

Expected: zero build errors and all tests pass.

- [ ] **Step 3: Review the diff**

Confirm that no action, movement, combat-presentation, or undo behavior changed beyond shared completion detection and automatic handoff.
