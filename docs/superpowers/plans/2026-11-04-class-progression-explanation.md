# Class Progression Explanation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Explain Class Progression rewards and make the currently selected reward unmistakable.

**Architecture:** Put stable player-facing copy and label formatting in `CrewPreparationPresentation`, where it can be tested without Godot. Render that content and reuse the existing path-button styling from `CrewPreparationView`.

**Tech Stack:** Godot 4.7.2, C#, existing console logic tests

**Spec:** `docs/superpowers/specs/2026-11-04-class-progression-explanation-design.md`

## Global Constraints

- Do not change progression rules or save data.
- Show the panel only when Class Progression choices are available.
- Preserve progression name and exact effect on every choice.
- Selected choices must say `✓ SELECTED` and use the same high-contrast styling as Training Paths.
- Preparation and reward content must scroll vertically rather than place confirmation controls outside the viewport.

## Review Focus

- Long ability descriptions must remain readable and wrapped.
- Tier II wording must not imply it can be selected without Tier I.
- Rebuilding the screen after selection must retain the selected state.
- The added panel must not obscure the confirmation controls.
- Characters without a pending reward must not see misleading reward instructions.

---

### Task 1: Presentation copy and selection labels

**Files:**
- Modify: `Scripts/UI/CrewPreparationPresentation.cs`
- Modify: `Tests/Program.cs`

**Interfaces:**
- Produces: `ClassProgressionExplanation`, `ClassProgressionInstruction`, and `ClassProgressionLabel(ClassProgressionDefinition progression, bool selected)`.

- [ ] Add a failing logic test covering permanent reward wording, Level Up distinction, Tier I prerequisite wording, instruction text, and selected/unselected labels.
- [ ] Run the logic tests and confirm the new assertions fail because the presentation members do not exist.
- [ ] Implement the presentation members with the approved copy and label format.
- [ ] Run the full logic suite and confirm it passes.

### Task 2: Reward-screen rendering and styling

**Files:**
- Modify: `Scripts/UI/CrewPreparationView.cs`

**Interfaces:**
- Consumes: Task 1 presentation members.
- Produces: an explanation panel and styled progression choice buttons.

- [ ] Add the explanation panel immediately above available Class Progression choices.
- [ ] Render buttons with `ClassProgressionLabel`, a taller wrapped layout, and `ApplyPathStyle` using the drafted selection state.
- [ ] Build the full solution and run all logic tests.
- [ ] Run `git diff --check`, review the screen-state code, and commit the feature.

