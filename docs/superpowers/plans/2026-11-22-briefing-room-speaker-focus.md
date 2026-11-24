# Briefing-Room Speaker Focus Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Keep the briefing-room cast in place while a fitted cyan contour identifies the current speaker and the complete Dr. Chandry artwork remains visible.

**Architecture:** Store the approved composition frame and portrait-key-to-outline-resource mapping in a Godot-free layout class that the logic tests can exercise. Render speaker contours through one reusable Godot `Control` with two full-frame overlay layers for crossfades; both briefing cinematic classes consume that component, while Ompok's separate hologram receives a transparent-texture outline shader.

**Tech Stack:** Godot 4.7.2 Mono, C#/.NET 10, Godot `Control`/`TextureRect`/`ShaderMaterial`, SVG overlay assets, existing console logic-test harness.

**Spec:** `docs/superpowers/specs/2026-11-22-briefing-room-speaker-focus-design.md`

## Global Constraints

- Keep every character in the standing position shown in `Assets/Cinematic/aurora-debriefing-room.png`.
- Do not circle characters, add a `SPEAKING` label, redraw the room, or change cinematic dialogue.
- Use soft cyan contours aligned to the flattened group composition.
- Apply the same behavior to the Mission 2 introduction, Mission 2 wrap-up, and Mission 3 introduction.
- Keep the complete Dr. Chandry figure inside the approved 1400 × 1070 cinematic layout.
- Preserve all existing uncommitted Mission 2/3 cinematic work; do not discard or overwrite it while implementing this plan.

## Review Focus

- An unknown or future portrait key must clear the prior outline instead of leaving the wrong speaker highlighted; Task 1 tests `OutlineFor("unknown") == null`.
- Consecutive lines by the same speaker must not accumulate duplicate overlays; Task 2 verifies the reusable view reuses two layers and resets their visibility before each crossfade.
- Transition and finish screens must clear both group outlines and Ompok's hologram effect; Task 2 adds explicit `Clear()` calls to both cinematic flows.
- Ompok dialogue must never show a crew outline behind his hologram; Task 1 tests that `ompok` is classified as a hologram and has no group outline resource.
- The background must preserve its wide aspect ratio at the safe inset rather than stretching or cropping Chandry; Task 1 tests the exact frame bounds and aspect ratio.

---

### Task 1: Speaker-Focus Layout Contract and Contour Assets

**Files:**
- Create: `Scripts/UI/BriefingSpeakerFocusLayout.cs`
- Create: `Assets/Cinematic/SpeakerFocus/issac-outline.svg`
- Create: `Assets/Cinematic/SpeakerFocus/jeane-outline.svg`
- Create: `Assets/Cinematic/SpeakerFocus/nance-outline.svg`
- Create: `Assets/Cinematic/SpeakerFocus/xander-outline.svg`
- Modify: `Tests/FailedOverride.LogicTests.csproj`
- Modify: `Tests/Program.cs`

**Interfaces:**
- Consumes: `MissionTwoIntroBeat.Portrait` keys from all three briefing-room scripts.
- Produces: `BriefingSpeakerFocusLayout.BackgroundFrame`, `BriefingSpeakerFocusLayout.OutlineFor(string portraitKey) -> string?`, and `BriefingSpeakerFocusLayout.IsHologram(string portraitKey) -> bool`.

- [ ] **Step 1: Add the new pure layout file to the logic-test project and write failing tests**

Add `briefing_speaker_focus_maps_every_scripted_group_speaker` and `briefing_speaker_focus_preserves_safe_wide_frame` to `Tests/Program.cs`. Assert that every distinct portrait key used by the three scripts either resolves through `OutlineFor` or is `ompok`; `ompok` is a hologram with no group outline; an unknown key returns `null`; and the frame equals `(24, 82, 1352, 761)`, stays inside 1400 pixels, and has an aspect ratio within `0.01` of `1672f / 941f`.

- [ ] **Step 2: Run the logic tests and verify RED**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj`

Expected: build failure because `BriefingSpeakerFocusLayout` does not exist.

- [ ] **Step 3: Implement the Godot-free layout contract**

Create `BriefingSpeakerFocusLayout` with the exact frame and resource mapping from Step 1. Normalize portrait keys with ordinal case-sensitive matching, return `null` for unknown keys, and classify only `ompok` as a hologram.

- [ ] **Step 4: Create full-canvas SVG contour overlays**

Each SVG uses `viewBox="0 0 1672 941"`, a transparent background, and a fitted cyan path around the corresponding figure in the approved room image. Use `#67E7F4` for the core stroke plus a low-opacity wider cyan stroke for the glow. Do not fill, circle, label, or reposition the person.

- [ ] **Step 5: Run the tests and validate every mapped asset**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj && for f in Assets/Cinematic/SpeakerFocus/*-outline.svg; do test -s "$f"; done`

Expected: all tests print `PASS`, no `FAIL` lines appear, and all four SVG files are non-empty.

- [ ] **Step 6: Commit the contract and contour assets**

Stage only the new layout file, contour assets, test-project include, test additions, approved design, and this plan. If the test files also contain the already-delivered Mission 2/3 script tests, include those existing changes in a separate baseline commit first rather than dropping them.

Commit message: `feat: define briefing speaker focus layout`

### Task 2: Reusable Crossfading Focus View and Cinematic Integration

**Files:**
- Create: `Scripts/UI/BriefingSpeakerFocusTransitionState.cs`
- Create: `Scripts/UI/BriefingSpeakerFocusView.cs`
- Modify: `Scripts/UI/MissionTwoIntro.cs`
- Modify: `Scripts/UI/MissionTwoIntroScript.cs`
- Modify: `Scripts/UI/DebriefingCinematic.cs`
- Test: `Tests/Program.cs`

**Interfaces:**
- Consumes: `BriefingSpeakerFocusLayout.BackgroundFrame`, `OutlineFor(string)`, and `IsHologram(string)` from Task 1.
- Produces: `BriefingSpeakerFocusTransitionState.Select(string portraitKey) -> BriefingSpeakerFocusTransition` and `BriefingSpeakerFocusTransitionState.Clear()` for deterministic current/previous focus ownership.
- Produces: `BriefingSpeakerFocusView.ShowSpeaker(string portraitKey, TextureRect? hologram)` and `BriefingSpeakerFocusView.Clear(TextureRect? hologram = null)`.

- [ ] **Step 1: Write failing transition-state tests**

Add `briefing_speaker_focus_switches_without_stale_or_duplicate_layers` to `Tests/Program.cs` and compile `BriefingSpeakerFocusTransitionState.cs` in the test project. Assert that selecting `jeane` starts with no previous focus; selecting `jeane` again keeps one current focus and no previous focus; selecting `issac` returns `jeane` as previous and `issac` as current; selecting `ompok` clears the group current focus and sets hologram focus; selecting `unknown` and calling `Clear()` leave neither a group focus nor hologram focus. Retain the Task 1 assertion that every scripted non-hologram speaker has one mapping.

- [ ] **Step 2: Run the logic tests and verify RED**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj`

Expected: build failure because `BriefingSpeakerFocusTransitionState` does not exist.

- [ ] **Step 3: Implement the transition state**

Create immutable `BriefingSpeakerFocusTransition` output with previous group key, current group key, and hologram-focus flag. `Select` uses Task 1's layout classification, suppresses duplicate previous/current values for repeated speakers, and clears all focus for unknown keys. `Clear` resets stored state.

- [ ] **Step 4: Run the focused tests and verify GREEN**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj`

Expected: every test prints `PASS` and no `FAIL` lines appear.

- [ ] **Step 5: Implement `BriefingSpeakerFocusView`**

Create a `Control` containing two `TextureRect` layers at `BackgroundFrame` and one `BriefingSpeakerFocusTransitionState`. `ShowSpeaker` consumes the state transition, loads the mapped full-canvas overlay, fades the previous layer from alpha 1 to 0 and the next from 0 to 1 over `0.18` seconds, and reuses the two layers for repeated calls. For Ompok, apply one reusable canvas-item alpha-outline `ShaderMaterial` to the supplied hologram instead of showing a group overlay. `Clear` resets the state, hides both layers, kills active focus tweens, and restores the hologram's normal material/visibility state.

- [ ] **Step 6: Integrate the component into the Mission 2 introduction**

Use `BriefingSpeakerFocusLayout.BackgroundFrame` for the room image with `TextureRect.StretchModeEnum.Scale`. Add the focus view above the existing dim layer and below input/dialogue controls. Call `ShowSpeaker(beat.Portrait, null)` from `ShowBeat`, and call `Clear()` before the final transition and when finishing. Do not restore the old foreground portrait.

- [ ] **Step 7: Integrate the component into the shared wrap-up/intro cinematic**

Use the same safe room frame in `DebriefingCinematic`. Call `ShowSpeaker(beat.Portrait, hologram)` after setting hologram visibility. Clear focus during the cargo-bay transition and on finish. Ensure a group-speaker beat hides Ompok and a hologram beat clears every group contour.

- [ ] **Step 8: Run the complete logic suite and C# build**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj && dotnet build FailedOverride.sln`

Expected: every test prints `PASS`, no `FAIL` lines appear, and the solution build ends with `0 Error(s)`.

- [ ] **Step 9: Run Godot's parser/import check**

Run: `'/Applications/Godot_mono.app/Contents/MacOS/Godot' --headless --editor --path . --quit`

Expected: exit code 0 with no C# parse errors, missing-resource errors, or invalid shader errors.

- [ ] **Step 10: Commit the reusable view and integrations**

Commit message: `feat: highlight briefing room speakers in place`

### Task 3: Visual and Packaged-App Verification

**Files:**
- Modify only if verification reveals a defect: the smallest Task 1 or Task 2 file responsible for that defect.
- Create deliverable outside the repository: `outputs/FailedOverride-macOS-v3.23.0-speaker-focus-native.zip`

**Interfaces:**
- Consumes: the completed briefing-room cinematics and existing Native macOS packaging process.
- Produces: a signed ARM64 application ZIP that launches after fresh extraction.

- [ ] **Step 1: Exercise all briefing speakers in the editor build**

Play or invoke each briefing flow and verify Issac, Jeane, Nance, and Xander receive in-place contours; repeated Ompok lines show only the outlined hologram; transitions clear the effect; and no contour persists onto a different speaker.

- [ ] **Step 2: Verify the composition at the reference window size**

At 1400 × 1070, confirm the dialogue remains readable, no contour is a circle or label, and Chandry's complete right edge stays inside the background frame in all three scenes.

- [ ] **Step 3: Re-run the complete automated verification immediately before packaging**

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj && dotnet build FailedOverride.sln`

Expected: all tests pass and the build ends with `0 Error(s)`.

- [ ] **Step 4: Create the Native ARM64 package**

Follow the previously verified Native process: export a fresh PCK from a clean project copy, publish the self-contained `osx-arm64` runtime into `data_FailedOverride_macos_arm64`, update the bundle version to `3.23.0`, remove quarantine attributes, ad-hoc sign with `build/macos-test-entitlements.plist`, and ZIP the app with `ditto`.

- [ ] **Step 5: Verify a fresh extraction**

Extract the ZIP into a new temporary directory; run `codesign --verify --deep --strict --verbose=2`; verify the main executable is ARM64 and the .NET data directory exists; then run `Failed Override.app/Contents/MacOS/Failed Override --headless --quit-after 2`.

Expected: signature valid, required runtime files present, and launch exit code 0.

- [ ] **Step 6: Record the deliverable checksum**

Run: `shasum -a 256 outputs/FailedOverride-macOS-v3.23.0-speaker-focus-native.zip`

Expected: one SHA-256 digest for the verified ZIP.
