# Training Path Explanation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Explain Training Paths before Mission 1 and make the selected path visually unmistakable.

**Architecture:** Add a Godot-independent presentation helper for exact copy and labels, then render it through native Godot controls and styles in `CrewPreparationView`.

**Tech Stack:** Godot 4 C#, .NET logic-test executable

**Spec:** `docs/superpowers/specs/2026-11-04-training-path-explanation-design.md`

## Global Constraints

- Keep all four choices visible without a dismissible popup.
- Preserve the existing Training Path behavior and confirmation requirements.
- Do not imply that changing paths alters previous Level Ups.

## Review Focus

- Explanation remains readable at the smallest supported window size.
- Selected and unselected buttons are distinguishable without relying only on color.
- All four selected-state labels retain their numerical bonus.
- Reward mode does not become unnecessarily crowded.
- Existing automatic next-character navigation remains unchanged.

---

### Task 1: Copy and selected-state labels

**Files:** `Scripts/UI/CrewPreparationPresentation.cs`, `Tests/Program.cs`, `Tests/FailedOverride.LogicTests.csproj`

- [ ] Add failing tests for the explanation's five promises and selected/unselected labels.
- [ ] Implement the presentation helper.
- [ ] Run the complete logic suite.

### Task 2: Native preparation-screen presentation

**Files:** `Scripts/UI/CrewPreparationView.cs`

- [ ] Add the always-visible explanation panel in preparation mode.
- [ ] Add high-contrast selected and unselected button styles.
- [ ] Build the complete Godot C# project and verify the diff.
