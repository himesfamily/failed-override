# Briefing Speaker Portraits Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace resolution-sensitive briefing-room outline markers with alternating full speaker portraits over a slightly darkened group background.

**Architecture:** Keep `DebriefingCinematic` responsible for scene/background/dialogue orchestration. Replace the marker-specific focus layout/view with a catalog-driven portrait stage: a pure layout class maps every scripted portrait key to a resource and left/right frame, a pure transition state controls two-layer crossfades, and the Godot view renders the selected portrait or Ompok hologram.

**Tech Stack:** Godot 4.7.2 .NET, C#, existing portrait PNG resources, existing logic-test console project.

**Spec:** `docs/superpowers/specs/2026-11-23-briefing-speaker-portraits-design.md`

## Global Constraints

- Preserve the existing briefing-room background, scripts, dialogue order, scene title, dialogue panel, click-to-continue behavior, transitions, and skip button.
- Darken the group background slightly while spoken dialogue is visible.
- Display one full portrait cutout at a time, alternating left/right by a stable per-character map.
- Use frame-relative layout that remains correct at all supported window sizes.
- Ompok retains the hologram presentation.
- Remove circles, arrows, and SVG outline markers from later briefing cinematics.
- Do not change the early Mission 1 cinematic treatment.

## Review Focus

- A scripted portrait key missing from the catalog must fail a test instead of silently showing no speaker.
- Repeated lines by one speaker must not flash or restart the crossfade.
- Rapid speaker changes must leave exactly one fully visible portrait after the tween completes.
- Small-window scaling must keep the entire portrait above the dialogue panel and inside the background frame.
- Ompok transitions to and from human speakers must clear stale portrait and hologram layers.

---

### Task 1: Speaker Portrait Catalog and Responsive Layout

**Files:**
- Create: `Scripts/UI/BriefingSpeakerPortraitLayout.cs`
- Modify: `Tests/FailedOverride.LogicTests.csproj`
- Test: `Tests/Program.cs`

**Interfaces:**
- Produces: `SpeakerPortraitSide`, `BriefingSpeakerPortrait` record, `BriefingSpeakerPortraitLayout.For(string)`, `DisplayFrameFor(string)`, and `AllHumanKeys`.

- [ ] **Step 1: Add failing catalog tests**

Add `briefing_portrait_catalog_covers_every_scripted_human_speaker` and `briefing_portrait_frames_stay_inside_scene_above_dialogue`. Assert every non-`ompok` portrait key across Mission 2 intro/wrap-up and Mission 3 intro resolves to an existing `res://Assets/Portraits/...` resource, each human is assigned Left or Right, and every 1400×1070 display frame stays within `BackgroundFrame` and ends above dialogue Y `730`.

- [ ] **Step 2: Run tests and verify RED**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj`

Expected: compile failure because `BriefingSpeakerPortraitLayout` does not exist.

- [ ] **Step 3: Implement the catalog and frames**

Map `jeane`, `issac`, `xander`, `nance`, `akiko`, and `chrandy` to their existing transparent/full portrait resources and stable alternating sides. Expose portrait frames derived from the existing 1400×1070 cinematic canvas and `BackgroundFrame`, using one left safe frame and one right safe frame above Y `730`.

- [ ] **Step 4: Run tests and verify GREEN**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj`

Expected: catalog and frame tests pass.

- [ ] **Step 5: Commit**

```bash
git add Scripts/UI/BriefingSpeakerPortraitLayout.cs Tests
git commit -m "feat: add briefing speaker portrait layout"
```

### Task 2: Portrait Crossfade State and Godot Stage

**Files:**
- Create: `Scripts/UI/BriefingSpeakerPortraitTransitionState.cs`
- Modify: `Scripts/UI/BriefingSpeakerFocusView.cs`
- Modify: `Tests/FailedOverride.LogicTests.csproj`
- Test: `Tests/Program.cs`

**Interfaces:**
- Consumes: Task 1 catalog/layout.
- Produces: `BriefingSpeakerPortraitTransitionState.Select(string)`, `Complete(...)`, `Clear()`, and a repurposed `BriefingSpeakerFocusView.ShowSpeaker(string, TextureRect?)` that renders portrait cutouts instead of SVG markers.

- [ ] **Step 1: Add failing transition tests**

Add tests for first selection, repeated-speaker reuse, left-to-right crossfade, rapid replacement before completion, human-to-Ompok, Ompok-to-human, and `Clear()`. Assert two-layer state never finishes with more than one visible human layer and hologram focus never leaves a human layer visible.

- [ ] **Step 2: Run tests and verify RED**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj`

Expected: compile failure because the portrait transition state does not exist.

- [ ] **Step 3: Implement portrait transition state**

Use two alternating layer indices with a `.18f` view fade, reusing the current layer for repeated keys and clearing human visibility for `ompok`.

- [ ] **Step 4: Replace marker rendering with portrait rendering**

Change `BriefingSpeakerFocusView` layers from `BriefingSpeakerMarker` to `TextureRect`. Load the Task 1 resource and frame for the active speaker, use `KeepAspectCentered`, crossfade old/new layers, and retain the existing hologram outline behavior. Remove marker halo/circle/arrow rendering from this path.

- [ ] **Step 5: Run logic tests and project build**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj && dotnet build FailedOverride.csproj --no-restore`

Expected: all tests pass; build has 0 errors.

- [ ] **Step 6: Commit**

```bash
git add Scripts/UI/BriefingSpeakerPortraitTransitionState.cs Scripts/UI/BriefingSpeakerFocusView.cs Tests
git commit -m "feat: draw briefing speakers as portraits"
```

### Task 3: Background Dimming, Cinematic Integration, and Verification

**Files:**
- Modify: `Scripts/UI/DebriefingCinematic.cs`
- Modify: `Scripts/UI/BriefingSpeakerFocusLayout.cs` or remove it if no longer referenced
- Modify/Delete if unused: `Scripts/UI/BriefingSpeakerMarker.cs`
- Test: `Tests/Program.cs`

**Interfaces:**
- Consumes: Task 2 `BriefingSpeakerFocusView` behavior.
- Produces: complete portrait treatment in Mission 2 intro/wrap-up and Mission 3 intro.

- [ ] **Step 1: Add failing integration assertions**

Add `later_briefing_scripts_use_supported_portrait_or_hologram_keys` and assert all beats resolve through the human catalog or `IsHologram`, including repeated speakers and every supported cast member.

- [ ] **Step 2: Add the background dimmer**

In `DebriefingCinematic`, place a single dark translucent `ColorRect` after the group background and before the portrait stage. Keep it visible for dialogue, clear portrait/hologram layers during transitions, and preserve existing panel/button Z-order.

- [ ] **Step 3: Remove obsolete marker dependencies**

Delete or disconnect marker-specific layout values, SVG loading, circles, arrows, and `BriefingSpeakerMarker` only after `rg` confirms no remaining runtime/test references.

- [ ] **Step 4: Run complete verification**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj && dotnet build FailedOverride.csproj --no-restore && git diff --check`

Expected: every test passes, build has 0 errors, and diff check is clean. The known environment-only NU1900 cache warning may remain.

- [ ] **Step 5: Run Godot visual smoke checks**

Launch Mission 2 intro, Mission 2 wrap-up, and Mission 3 intro at small, default, and fullscreen sizes. Confirm alternating sides, darkened group, no portrait/dialogue overlap, repeated-speaker stability, clean human/Ompok transitions, and no old marker artifacts.

- [ ] **Step 6: Commit**

```bash
git add Scripts/UI Tests
git commit -m "feat: integrate alternating briefing portraits"
```

### Task 4: macOS Test Package

**Files:**
- Modify only for defects found during packaging.

**Interfaces:**
- Produces: a signed, extracted, smoke-launched macOS test zip in `outputs/`.

- [ ] **Step 1: Request whole-branch review and fix Critical/Important findings**

Review against the approved spec with special attention to missing portrait mappings, resize behavior, tween cleanup, and hologram transitions.

- [ ] **Step 2: Export and package**

Export a fresh PCK, update the arm64 app bundle and managed assembly, ad-hoc sign it, zip with resource forks preserved, and extract into a fresh verification directory.

- [ ] **Step 3: Verify the extracted build**

Run `codesign --verify --deep --strict`, confirm managed assemblies exist, smoke-launch the extracted executable, record `shasum -a 256`, and report the versioned output path.

- [ ] **Step 4: Commit packaging fixes if needed**

```bash
git add <verified-fix-files>
git commit -m "fix: address briefing portrait verification"
```
