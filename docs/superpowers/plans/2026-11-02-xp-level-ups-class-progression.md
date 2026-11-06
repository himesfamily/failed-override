# XP, Level Ups, Roster Selection, and Class Progression Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Deliver a playable progression loop with visible battle XP, automatic path-based Level Ups, four-character deployment, reserve catch-up, rollback on failure, and prerequisite-based Class Progressions.

**Architecture:** Add persistent per-character progression records to `CampaignState`, while `BattleState` owns mission-local XP, machine ownership, and victory/rollback transactions. Keep Class Progression definitions declarative and engine-independent, then add a Godot preparation screen and level-up presentation that consume the campaign interfaces.

**Tech Stack:** Godot 4.7.2, C#, .NET 8 game project, .NET 10 console logic tests

**Spec:** `docs/superpowers/specs/2026-11-02-xp-level-ups-class-progression-design.md`

## Global Constraints

- Preserve 25 XP for a damaging non-kill attack and 50 XP total for a killing attack.
- Award 20 XP only for successful useful actions named in the spec; Transform, Move, and Wait award none.
- Level 2 grants +4 maximum/current HP; later Level Ups follow the selected path.
- No choice dialog occurs during battle.
- Four player characters deploy; mission-required characters occupy slots; machines do not.
- Victory commits progression and awards reserve catch-up; defeat/restart restores the pre-mission snapshot.
- Tier II Class Progressions require their matching Tier I.
- Existing saves load without losing legacy power.

## Review Focus

- XP crossing 100 exactly or with carryover must apply one or more Level Ups without losing XP.
- Machine XP must still reach its owner if the owner is defeated later.
- Stun-only attacks that deal zero damage must not earn attack XP.
- A failed mission after an in-battle Level Up must restore every ordinary stat bonus.
- Roster validation must reject duplicates, unavailable IDs, missing required characters, and more than four selections.

---

### Task 1: Persistent Character Progression

**Files:**
- Create: `Scripts/Campaign/CharacterProgression.cs`
- Modify: `Scripts/Campaign/CampaignState.cs`
- Modify: `Tests/FailedOverride.LogicTests.csproj`
- Test: `Tests/Program.cs`

**Interfaces:**
- Produces: `TrainingPath`, `CharacterProgression`, `CampaignState.Progress(string)`, `CampaignState.AwardXp(string,int)`, `CampaignState.SetTrainingPath(string,TrainingPath)`, and save-data migration.

- [ ] Add failing tests for Level 2 health, each later path, carryover, random reserve growth, and legacy save migration.
- [ ] Run tests and confirm RED.
- [ ] Implement persistent level, XP, path, four stat bonuses, progression IDs, and pending Class Progression count.
- [ ] Replace universal `Level - 1` damage growth with stored attack bonus while migrating old levels to equivalent legacy attack power.
- [ ] Run tests and confirm GREEN.
- [ ] Commit `feat: model persistent character progression`.

### Task 2: Battle XP, Ownership, and Mission Transactions

**Files:**
- Modify: `Scripts/Units/Unit.cs`
- Modify: `Scripts/Battle/BattleState.cs`
- Test: `Tests/Program.cs`

**Interfaces:**
- Consumes: Task 1 progression APIs.
- Produces: machine `ControllerId`, `BattleState.LastXpAward`, victory commit, reserve catch-up, and rollback-safe provisional progression.

- [ ] Add failing tests for 25/50 attack XP, every 20 XP utility action, excluded actions, and stun-only attacks.
- [ ] Add failing tests that robot and reprogrammed-droid attacks credit Akiko and Issac.
- [ ] Add failing tests for reserve average XP, victory commit, and defeat/restart rollback.
- [ ] Implement one XP award helper and route successful actions through it.
- [ ] Snapshot progression at mission construction; commit catch-up and pending Class Progressions only on victory; restore on defeat/restart.
- [ ] Run tests and confirm GREEN.
- [ ] Commit `feat: award and transact mission experience`.

### Task 3: Roster Selection and Deployment

**Files:**
- Modify: `Scripts/Campaign/CampaignState.cs`
- Modify: `Scripts/Battle/BattleState.cs`
- Test: `Tests/Program.cs`

**Interfaces:**
- Produces: `CampaignState.AvailableCrew(int)`, `CampaignState.RequiredCrew(int)`, `CampaignState.ValidateRoster(int,IEnumerable<string>)`, and `BattleState(int,CampaignState?,IEnumerable<string>?)`.

- [ ] Add failing tests for Mission 1/2 fixed rosters, Mission 3 four-of-five selection, required crew, duplicates, and machines excluded from roster size.
- [ ] Implement roster validation and omit unselected player characters from mission construction.
- [ ] Ensure objectives and defeat rules remain valid for every legal Mission 3 roster.
- [ ] Run tests and confirm GREEN.
- [ ] Commit `feat: select four-character mission rosters`.

### Task 4: Class Progression Definitions and Effects

**Files:**
- Create: `Scripts/Campaign/ClassProgressionCatalog.cs`
- Modify: `Scripts/Campaign/CampaignState.cs`
- Modify: `Scripts/Battle/BattleState.cs`
- Modify: `Scripts/Units/Unit.cs`
- Test: `Tests/Program.cs`

**Interfaces:**
- Produces: `ClassProgressionDefinition`, `ClassProgressionCatalog.For(string)`, `CampaignState.AvailableClassProgressions(string)`, and `CampaignState.ChooseClassProgression(string,string)`.

- [ ] Add failing tests for all Tier II prerequisites, one-time selection, pending-choice consumption, and branch mixing.
- [ ] Add focused combat tests for every approved Class Progression effect.
- [ ] Implement declarative catalog and apply effects at unit construction or the relevant battle rule.
- [ ] Preserve legacy upgrade fields and effects for old saves.
- [ ] Run tests and confirm GREEN.
- [ ] Commit `feat: add branching class progressions`.

### Task 5: Preparation and Battle Progression UI

**Files:**
- Create: `Scripts/UI/LevelUpPresentationState.cs`
- Modify: `Scripts/UI/BattleView.cs`
- Modify: `Tests/FailedOverride.LogicTests.csproj`
- Test: `Tests/Program.cs`

**Interfaces:**
- Consumes: Tasks 1–4 campaign, roster, and progression APIs.
- Produces: pre-mission crew preparation screen, XP bar, XP feedback, and click-to-dismiss Level Up presentation.

- [ ] Add engine-independent tests for queuing and dismissing multiple Level Up notices after an action presentation.
- [ ] Add a preparation panel with character cards, deployment toggles, Training Path controls, pending Class Progression choices, and a validated Begin Mission button.
- [ ] Replace the fixed Mission 1 upgrade flow with the preparation screen.
- [ ] Add selected-character XP bar/path text and `+XP` feedback.
- [ ] Show Level Up notices only after the current combat/action presentation finishes.
- [ ] Run logic tests and a clean isolated C# build.
- [ ] Commit `feat: add crew preparation and progression UI`.

### Task 6: Review, Migration Smoke Test, and Native macOS Build

**Files:**
- Modify only as required by review.
- Use: `build/macos-test-entitlements.plist`

**Interfaces:**
- Produces: verified Native macOS DMG in the top-level `outputs` folder.

- [ ] Run whole-branch review against the approved spec and fix Critical/Important findings once.
- [ ] Run all logic tests, clean isolated restore/build, and `git diff --check`.
- [ ] Import assets and smoke-test a new campaign plus an existing-save migration.
- [ ] Export with Godot 4.7.2, re-sign ad hoc without hardened runtime using the four C# entitlements, and verify headless launch exits 0.
- [ ] Package a UDF-only DMG, verify its checksum, and copy it to the top-level `outputs` folder.
