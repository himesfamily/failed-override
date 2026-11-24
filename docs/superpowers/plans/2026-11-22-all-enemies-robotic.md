# All Enemies Robotic Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Let Issac reprogram every damaged enemy type in missions 1–3 while preserving each enemy's existing identity.

**Architecture:** Add a single role-level machine classification and make BattleState and BattleView consume it for reprogram, repair, healing exclusion, and action availability. Preserve all existing roles and assets.

**Tech Stack:** Godot 4.7.2 Mono, C#/.NET, existing console logic tests.

**Spec:** `docs/superpowers/specs/2026-11-22-all-enemies-robotic-design.md`

## Global Constraints

- Preserve existing art, role, stats, attacks, AI, and special behavior.
- Reprogram and repair eligibility must use the same machine classification.
- Reprogram health thresholds and action costs do not change.

## Review Focus

- Specialized mission 3 enemies remain visually and tactically unchanged after classification.
- Ompok's proxy can be reprogrammed only when adjacent and sufficiently damaged.
- Converted specialized machines become repairable.
- Organic healing rejects converted machines.
- UI eligibility matches BattleState validation.

---

### Task 1: Shared Machine Classification

**Files:**
- Modify: `Scripts/Units/Unit.cs`
- Modify: `Scripts/Battle/BattleState.cs`
- Modify: `Scripts/UI/BattleView.cs`
- Test: `Tests/Program.cs`

**Interfaces:**
- Produces: `Unit.IsMachineRole(Role role)` and `Unit.Machine`
- Consumes: existing role values and reprogram/repair/heal flows

- [ ] **Step 1: Write failing tests for reprogramming and repairing every enemy role.**
- [ ] **Step 2: Run the logic suite and verify the new assertions fail.**
- [ ] **Step 3: Add the shared predicate and replace hard-coded role lists.**
- [ ] **Step 4: Run the full logic suite and C# build.**
- [ ] **Step 5: Commit the tested change.**

### Task 2: Native Test Package

**Files:**
- Create: `Assets/Audio/SFX/issac_scream_wilhelm.mp3`
- Create: `Scripts/UI/OpeningCinematicAudio.cs`
- Modify: `Scripts/UI/OpeningCinematic.cs`
- Modify: Native export staging tree
- Create: `outputs/FailedOverride-macOS-v3.23.0-speaker-focus-native.zip`

- [ ] **Step 1: Add the supplied scream and verify only Issac's attack narration triggers it.**
- [ ] **Step 2: Run Godot headless import.**
- [ ] **Step 3: Export the corrected PCK and rebuild the signed native app.**
- [ ] **Step 4: Extract the ZIP, verify signature and launch, and record SHA-256.**
