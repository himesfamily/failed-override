# Turn Flow Consistency Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make character confirmation, controlled-machine switching, and inactive enemy messaging follow consistent turn-flow rules.

**Architecture:** Put navigation and battle rules in their existing state classes, leaving the Godot views to render the resulting selection and log text. Add focused logic tests before each production change.

**Tech Stack:** Godot 4 C#, .NET logic-test executable

**Spec:** `docs/superpowers/specs/2026-11-03-turn-flow-consistency-design.md`

## Global Constraints

- Preserve hero move undo behavior.
- Automatic Wait does not award XP.
- Preserve enemy pacing and deterministic combat.

## Review Focus

- The last confirmed character must not jump to an already confirmed card.
- Untouched machines must remain available when another unit is selected.
- A hero who moved but did not act must retain undo/action options.
- A blocked Ompok proxy must also use truthful Wait messaging.
- An enemy that attacks without moving must still report the attack, not Wait.

---

### Task 1: Preparation auto-advance

**Files:**
- Modify: `Scripts/UI/CrewPreparationState.cs`
- Modify: `Scripts/UI/CrewPreparationView.cs`
- Test: `Tests/Program.cs`

- [ ] Add a failing test for choosing the next unconfirmed deployed character in roster order.
- [ ] Add the state query and use it after confirmation in both preparation modes.
- [ ] Run the full logic-test suite.

### Task 2: Controlled-machine selection Wait

**Files:**
- Modify: `Scripts/Battle/BattleState.cs`
- Test: `Tests/Program.cs`

- [ ] Add failing tests for moved summoned robots and acted reprogrammed droids.
- [ ] Extend selection-change Wait for partial machine turns while preserving hero rules.
- [ ] Run the full logic-test suite.

### Task 3: Truthful enemy Wait messaging

**Files:**
- Modify: `Scripts/Battle/BattleState.cs`
- Modify: `Scripts/UI/BattleView.cs`
- Test: `Tests/Program.cs`

- [ ] Add failing tests for a blocked regular enemy and blocked Ompok proxy.
- [ ] Log Wait when movement makes no progress and replace the UI's generic “acts” fallback.
- [ ] Run the full logic-test suite and build the Godot C# project.
