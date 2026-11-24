# Briefing Speaker Marker Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace inaccurate full-body briefing outlines with window-stable speaker markers.

**Architecture:** Store head-and-shoulder regions in source-art coordinates, transform them through the existing letterboxed background frame, and render native pulsing marker controls. Reuse the tested speaker transition state for crossfades and replay safety.

**Tech Stack:** Godot 4.7.2 Mono, C#/.NET, existing console logic tests.

**Spec:** `docs/superpowers/specs/2026-11-22-briefing-speaker-marker-design.md`

## Global Constraints

- Keep characters in the group artwork.
- Marker positions must scale with the artwork at every window size.
- Preserve Ompok's hologram focus.

## Review Focus

- Every scripted crew speaker has one distinct, non-overlapping marker region.
- Source-to-display coordinate conversion preserves aspect ratio and frame offset.
- Rapid speaker changes and replay do not show a stale marker.
- Marker controls cannot intercept cinematic clicks.
- Ompok still clears crew markers synchronously.

---

### Task 1: Native Speaker Marker

**Files:**
- Modify: `Scripts/UI/BriefingSpeakerFocusLayout.cs`
- Create: `Scripts/UI/BriefingSpeakerMarker.cs`
- Modify: `Scripts/UI/BriefingSpeakerFocusTransitionState.cs`
- Modify: `Scripts/UI/BriefingSpeakerFocusView.cs`
- Modify: `Tests/Program.cs`

- [ ] **Step 1: Write failing tests for marker mappings, coordinate conversion, and replay transitions.**
- [ ] **Step 2: Run tests and verify the marker API is missing.**
- [ ] **Step 3: Implement native markers and remove SVG texture dependence.**
- [ ] **Step 4: Run logic tests, C# build, Godot import, and rendered composition preview.**
- [ ] **Step 5: Commit and create a signed Native test ZIP.**
