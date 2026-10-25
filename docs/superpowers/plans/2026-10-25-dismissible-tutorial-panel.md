# Dismissible Tutorial Panel Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Let a player click Captain Gershon's Mission 1 tutorial panel to hide it temporarily without completing or skipping the active lesson.

**Architecture:** Add a small Godot-independent presentation-state class that distinguishes the active lesson from whether its panel is currently visible. `BattleView` will use that state and make the panel consume mouse clicks, dismiss on press, re-present an incomplete lesson on a later tutorial refresh, and display a dismiss hint.

**Tech Stack:** Godot 4.7.2, C#, existing console logic-test harness

**Spec:** `docs/superpowers/specs/2026-10-25-dismissible-tutorial-panel-design.md`

## Global Constraints

- Clicking anywhere on the visible Captain Gershon panel dismisses only its current presentation.
- Dismissal must not complete or skip the active lesson.
- The incomplete lesson may reappear on a later tutorial refresh.
- The click must not pass through to the tactical map.
- Show “Click to dismiss” and a pointing-hand cursor.
- Do not change other dialogue windows, tutorial triggers, completion rules, or campaign progression.

## Review Focus

- A dismissal must preserve the active lesson ID; test it directly in the presentation-state unit test.
- Re-presenting the same incomplete lesson must make it visible again; test a dismiss-then-show sequence.
- Completing a lesson must clear both the lesson ID and visibility; test both outputs.
- Resetting for a new campaign must clear presentation state; test `Reset()` independently.
- Panel clicks must not reach map movement; verify the Godot control uses `MouseFilterEnum.Stop` and accepts the GUI event during integration review.

---

### Task 1: Tutorial Presentation State and Clickable Panel

**Files:**
- Create: `Scripts/UI/TutorialPresentationState.cs`
- Modify: `Tests/FailedOverride.LogicTests.csproj`
- Modify: `Tests/Program.cs`
- Modify: `Scripts/UI/BattleView.cs`

**Interfaces:**
- Produces: `TutorialPresentationState.LessonId`, `TutorialPresentationState.IsVisible`, `Show(string lessonId)`, `Dismiss()`, `Complete()`, and `Reset()`.
- Consumes: Existing `BattleView` tutorial lesson triggers and completion checks.

- [ ] **Step 1: Write the failing presentation-state tests**

Add `Scripts/UI/TutorialPresentationState.cs` to the logic-test project and add tests named `tutorial_dismissal_preserves_active_lesson`, `tutorial_incomplete_lesson_can_be_presented_again`, `tutorial_completion_clears_lesson`, and `tutorial_reset_clears_presentation`. Assert the exact lesson ID and visibility after each operation.

- [ ] **Step 2: Run the logic-test project and verify RED**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj --no-restore`

Expected: compilation fails because `TutorialPresentationState` does not exist.

- [ ] **Step 3: Implement the minimal presentation state**

Create `FailedOverride.UI.TutorialPresentationState` with the interface above. `Dismiss()` changes only `IsVisible`; `Show()` retains/sets the lesson and makes it visible; `Complete()` and `Reset()` clear the lesson and visibility.

- [ ] **Step 4: Run the logic-test project and verify GREEN**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj --no-restore`

Expected: all existing and four new tests pass.

- [ ] **Step 5: Integrate the state into `BattleView`**

Replace the standalone tutorial lesson string with `TutorialPresentationState`. Configure `tutorialPanel.MouseFilter` as `Stop`, use a pointing-hand cursor, handle a pressed left mouse button through `GuiInput`, call `Dismiss()`, hide the panel, and accept the event. Keep child controls set to `Ignore` so the parent receives clicks. Append a clearly styled `Click to dismiss` line to the tutorial text. In `UpdateTutorial()`, call `Show()` for an existing incomplete lesson so it can return after a later refresh.

- [ ] **Step 6: Build the Godot C# project**

Run: `dotnet build FailedOverride.csproj --no-restore -p:RuntimeIdentifiers=`

Expected: build succeeds with zero errors and zero warnings.

- [ ] **Step 7: Re-run the complete logic suite**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj --no-restore`

Expected: all tests pass.

- [ ] **Step 8: Review the diff against the approved spec**

Confirm the panel consumes its click, the lesson ID survives dismissal, the same lesson returns on refresh, completion behavior is unchanged, and no unrelated UI or gameplay behavior changed.

- [ ] **Step 9: Commit the feature**

```bash
git add Scripts/UI/TutorialPresentationState.cs Scripts/UI/BattleView.cs Tests/FailedOverride.LogicTests.csproj Tests/Program.cs docs/superpowers/specs/2026-10-25-dismissible-tutorial-panel-design.md docs/superpowers/plans/2026-10-25-dismissible-tutorial-panel.md
git commit -m "feat: make tutorial panel temporarily dismissible"
```
