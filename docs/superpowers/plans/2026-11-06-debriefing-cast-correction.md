# Mission 2 Debriefing Cast Correction Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Restore male Chrandy and make the far-left debriefing-room officer unmistakably Issac Wynter.

**Architecture:** Restore Chrandy's two canonical standalone assets from the branch baseline, remove the superseded female-presentation support, and make one targeted raster edit to the room illustration using Issac's established portrait. Keep full-composition cinematic framing so both edge characters remain visible.

**Tech Stack:** Built-in image editing, PNG assets, Godot 4.7.2 Mono, C#

**Spec:** `docs/superpowers/specs/2026-11-06-debriefing-cast-correction.md`

## Global Constraints

- Chrandy remains the established male medic.
- Issac is a Black man with short dark hair, trimmed beard, and a white-and-blue science-officer uniform.
- Do not change Captain Gershon, Xander, Nance, Akiko, dialogue, gameplay, or the room composition.
- Preserve full-composition cinematic framing.

## Review Focus

- Restored portrait and token must exactly match the pre-redesign male assets.
- Far-left Issac must clearly match `Assets/Portraits/issac-wynter.png` at cinematic display size.
- Center Xander and far-left Issac must no longer read as duplicate characters.
- Far-right male Chrandy must remain visible and distinct from Xander.
- Godot resource paths and imports must remain valid.

---

### Task 1: Restore Male Chrandy

**Files:**
- Restore: `Assets/Portraits/dr-chrandy-perra.png`
- Restore: `Assets/BoardTokens/chrandy-medic.png`
- Modify: `Scripts/UI/BattleView.cs`
- Remove: `Scripts/UI/MedicPresentation.cs`
- Modify: `Tests/FailedOverride.LogicTests.csproj`
- Modify: `Tests/Program.cs`

**Interfaces:**
- Consumes: branch-baseline male Chrandy assets and copy
- Produces: original male Chrandy portrait, token, and pronouns with no female-redesign support

- [ ] Restore both canonical PNGs from the branch baseline and verify their checksums against the baseline versions.
- [ ] Remove the superseded female-presentation class and regression test include.
- [ ] Restore Chrandy's masculine self-treatment copy while preserving unrelated Mission 2 code.

### Task 2: Correct Issac in the Debriefing Room

**Files:**
- Modify: `Assets/Cinematic/aurora-debriefing-room.png`
- Refresh: `Assets/Cinematic/aurora-debriefing-room.png.import`

**Interfaces:**
- Consumes: the original debriefing room, `Assets/Portraits/issac-wynter.png`, and the original male Chrandy reference
- Produces: corrected room art with recognizable Issac at far left and male Chrandy at far right

- [ ] Restore the pre-female-edit room as the editing base.
- [ ] Edit only the far-left officer's identity and uniform to match Issac's portrait.
- [ ] Compare before/after and verify all other people, props, lighting, and composition are materially preserved.

### Task 3: Preserve Framing and Verify

**Files:**
- Modify: `Scripts/UI/MissionTwoIntroScript.cs`
- Modify: `Scripts/UI/MissionTwoIntro.cs`
- Modify: `Tests/Program.cs`

**Interfaces:**
- Consumes: corrected wide cinematic
- Produces: full-composition presentation with both Issac and Chrandy visible

- [ ] Add a presentation constant and regression test requiring full-composition framing.
- [ ] Use centered aspect-preserving display over a dark backdrop in the Mission 2 intro.
- [ ] Reimport the revised assets and verify all relevant resources load.
- [ ] Run the full logic suite, C# build, headless Godot launch, and `git diff --check`.
