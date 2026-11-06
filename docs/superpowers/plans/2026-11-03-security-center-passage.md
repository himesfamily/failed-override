# Mission 2 Security-Center Passage Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace Mission 2's Issac-only shuttle interaction with a walk-on security-center passage usable by any deployed crew member.

**Architecture:** Store the renamed objective in `MissionDefinition`, resolve victory in `BattleState.Move`, and keep `BattleView` responsible only for wording and the map marker.

**Tech Stack:** Godot 4 C#, .NET logic-test executable

**Spec:** `docs/superpowers/specs/2026-11-03-security-center-passage-design.md`

## Global Constraints

- Preserve the 20 XP useful-objective award.
- Preserve the existing Mission 2 map coordinate and campaign mission key.
- Do not require a second action after entering the passage.

## Review Focus

- Every deployed hero and Issac can complete the objective.
- Reprogrammed and summoned allied machines cannot complete the objective alone.
- Mission 2 has no invisible console interaction.
- Mission 1 console behavior remains available.
- Movement animation and reward flow still receive the normal move path and victory state.

---

### Task 1: Objective rule and regression coverage

**Files:** `Scripts/Battle/MissionDefinition.cs`, `Scripts/Battle/BattleState.cs`, `Tests/Program.cs`

- [ ] Add failing tests for every Mission 2 crew member and an allied machine entering the objective.
- [ ] Rename the objective and complete victory from normal movement.
- [ ] Verify the full logic suite.

### Task 2: Player-facing mission identity

**Files:** `Scripts/UI/BattleView.cs`, `README.md`

- [ ] Replace evacuation wording and the Issac-only action.
- [ ] Draw a labeled security-center passage marker.
- [ ] Build the full Godot C# solution and check for stale shuttle references.
