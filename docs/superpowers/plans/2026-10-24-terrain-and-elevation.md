# Terrain and Elevation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace unclear painted battle underlays with readable generated terrain, weighted climbing, low cover, high-ground combat modifiers, and redesigned 12×12 layouts for all three missions.

**Architecture:** Add explicit terrain and mission-definition classes to the battle domain, then make `BattleState` the single authority for weighted movement, paths, line of sight, cover, elevation, and damage breakdowns. `BattleView` will render those terrain values with the approved height-and-shadows language and consume model-provided paths and combat explanations. A dependency-free console test project will exercise battle logic without requiring a Godot window.

**Tech Stack:** Godot 4.7.2 .NET, C# / .NET 8, Godot `Node2D` drawing APIs, custom .NET console logic-test runner, Git.

**Spec:** `docs/superpowers/specs/2026-10-24-terrain-and-elevation-design.md`

## Global Constraints

- Keep the board at exactly 12×12 cells with 64-pixel tiles.
- Preserve deterministic combat, abilities, saves, upgrades, bonds, objectives, cinematics, sound, settings, and navigation.
- Use elevation 0 and 1 only. Floor-to-high costs 3; high-to-high and descending cost 1.
- Ranged high ground deals +2 downward and reduces incoming ranged damage from below by 1.
- Crossed low cover reduces ranged damage by 1 and never stacks with itself.
- Walls and pillars block movement and sight; low crates block movement but not sight; high containers are walkable.
- Do not render full-board environment paintings during battles in this version.
- Add no external test or runtime dependencies.

## Review Focus

- Exactly 2 movement points cannot climb, while exactly 3 can; test in Task 3.
- Shallow/diagonal traces and multiple crates apply cover at most once; test in Task 4.
- Melee across one height level stays legal and unmodified; test in Task 4.
- Player and enemy animations never visually cross blockers; model-test paths in Task 3 and playtest in Task 6.
- Every spawn/objective is valid and reachable, with two viable Mission 3 bridge routes; test in Task 2.

---

### Task 1: Establish Version Control and a Logic-Test Harness

**Files:**
- Create: `.gitignore`
- Create: `Tests/FailedOverride.LogicTests.csproj`
- Create: `Tests/Program.cs`
- Modify: `README.md`

**Interfaces:**
- Consumes: existing pure C# battle, campaign, and unit files.
- Produces: `dotnet run --project Tests/FailedOverride.LogicTests.csproj`; `Test.Run(string, Action)` and `Test.Equal<T>(T, T)`.

- [ ] **Step 1: Create `.gitignore`**

Ignore `.godot/`, `.superpowers/`, `bin/`, `obj/`, test build folders, `.DS_Store`, generated export folders, `*.pck`, and local save/feedback data. Keep source art and shipped `.import` metadata.

- [ ] **Step 2: Initialize Git and commit the baseline**

Run `git init`, `git add .`, and `git commit -m "chore: preserve pre-terrain prototype"`.

Expected: one root commit and a clean working tree containing the playable project, approved design, and ignore rules.

- [ ] **Step 3: Create the dependency-free console test project**

Target `net8.0`, disable default compile items, and explicitly compile `Program.cs` plus pure logic files under `Scripts/Battle`, `Scripts/Campaign/CampaignState.cs`, and `Scripts/Units/Unit.cs`. Do not compile Godot-dependent UI or storage files.

- [ ] **Step 4: Add and run a smoke test**

Add `battle_constructs_all_three_missions`, asserting `new BattleState(1..3)` each has a 12×12 map and a player unit.

Run: `dotnet run --project Tests/FailedOverride.LogicTests.csproj`

Expected: three `PASS` results and exit code 0.

- [ ] **Step 5: Document development commands**

Add a README section with the logic-test command and explain that Git is local with no remote configured.

- [ ] **Step 6: Commit**

Commit `.gitignore`, `Tests`, and `README.md` as `test: add battle logic harness`.

### Task 2: Model Terrain and Mission Definitions

**Files:**
- Create: `Scripts/Battle/Terrain.cs`
- Create: `Scripts/Battle/MissionDefinition.cs`
- Modify: `Scripts/Battle/BattleState.cs`
- Modify: `Tests/Program.cs`

**Interfaces:**
- Consumes: `Cell`, `Unit`, `Role`, and `Team`.
- Produces: `TerrainKind`, `TerrainCell`, `UnitSpawn`, `MissionDefinition.For(int)`, `BattleState.Definition`, `BattleState.TerrainAt(Cell)`, and definition-backed objective cells.

- [ ] **Step 1: Write failing terrain-definition tests**

Add `terrain_kinds_expose_correct_rules`, `all_missions_are_12_by_12`, `all_spawns_and_objectives_use_valid_terrain`, `mission_one_objective_is_reachable`, `mission_two_shuttle_is_reachable`, and `mission_three_has_two_distinct_routes_to_bridge`. Assert the exact spec table; for Mission 3, remove either approach cell and confirm the other route works.

- [ ] **Step 2: Run tests to verify failure**

Run the logic tests. Expected: compilation fails because the terrain types do not exist.

- [ ] **Step 3: Implement terrain types**

Create `TerrainKind { Floor, Wall, Pillar, LowCover, Elevated, Objective }` and `readonly record struct TerrainCell(TerrainKind Kind)` with `Walkable`, `BlocksSight`, and `Elevation`. Wall/pillar/cover are not walkable; only wall/pillar block sight; only elevated has elevation 1.

- [ ] **Step 4: Implement mission definitions and redesigned maps**

Define `UnitSpawn(char Token, Cell Position)` and `MissionDefinition` with `Number`, `TerrainCell[,] Terrain`, `IReadOnlyList<UnitSpawn> Spawns`, nullable `Console`, `Shuttle`, `Bridge`, `For(int)`, and `At(Cell)`. Use separate 12×12 terrain and spawn data. Mission 1 teaches cover with optional elevation; Mission 2 has cargo routes and Issac with the crew; Mission 3 has two bridge routes. Outer boundaries stay solid.

- [ ] **Step 5: Make `BattleState` consume definitions**

Construct units from `Definition.Spawns`; source objective cells from the definition; add `TerrainAt`. Retain a compatibility `Map` only if legacy behavior needs it, but all terrain rules use `TerrainAt`.

- [ ] **Step 6: Verify and commit**

Run logic tests and `dotnet build FailedOverride.csproj`; expect all tests passing and 0 errors. Commit as `feat: define tactical terrain and mission maps`.

### Task 3: Add Weighted Movement and Legal Paths

**Files:**
- Modify: `Scripts/Battle/BattleState.cs`
- Modify: `Tests/Program.cs`

**Interfaces:**
- Consumes: terrain walkability/elevation.
- Produces: `MovementCost(Cell from, Cell to)`, weighted `Reach(Unit)`, and `Path(Unit, Cell)` returning origin through destination.

- [ ] **Step 1: Write failing movement tests**

Add `floor_step_costs_one`, `climb_costs_three`, `high_to_high_costs_one`, `descent_costs_one`, `two_points_cannot_climb_but_three_can`, `blockers_cannot_be_entered`, `least_cost_path_routes_around_blockers`, `path_never_crosses_low_cover`, and `occupied_cells_are_unreachable`. Use a small test definition rather than relying only on production maps.

- [ ] **Step 2: Run tests to verify failure**

Expected: new tests fail because movement is uniform and no path API exists.

- [ ] **Step 3: Implement weighted reachability**

Use `PriorityQueue<Cell,int>` relaxation, stop beyond `u.Move`, and preserve bounds, occupation, and origin handling. `MovementCost` returns 3 for elevation 0→1 and 1 otherwise.

- [ ] **Step 4: Implement path reconstruction**

`Path(Unit, Cell)` uses the same legal/cost rules as `Reach`, returns empty for illegal destinations, otherwise returns the least-cost origin-to-destination cells.

- [ ] **Step 5: Verify and commit**

Run logic tests and the project build; expect all tests passing and 0 errors. Commit as `feat: add weighted climbing and legal movement paths`.

### Task 4: Add Cover, Elevation, and Terrain-aware Enemy Choices

**Files:**
- Create: `Scripts/Battle/DamageBreakdown.cs`
- Modify: `Scripts/Battle/BattleState.cs`
- Modify: `Tests/Program.cs`

**Interfaces:**
- Consumes: terrain height and sight rules.
- Produces: `DamageBreakdown`, `Trace(Cell, Cell)`, `HasLowCover(Cell, Cell)`, `PreviewDamageBreakdown(Unit, Unit, Weapon)`, and compatible `PreviewDamage(Unit, Unit, Weapon)`.

- [ ] **Step 1: Write failing sight/combat tests**

Add tests for wall/pillar sight blocking; cover/elevation not blocking sight; one cover reduction through one or several crates; no duplicate shallow-angle cells; no melee cover modifier; +2 shooting down; -1 shooting up; stacked cover/low-ground defense; legal unmodified cross-height melee; zero stun-only damage; existing minimum damage; and enemy preference for greater actual terrain-adjusted damage.

- [ ] **Step 2: Run tests to verify failure**

Expected: terrain modifier cases fail under the current damage formula.

- [ ] **Step 3: Add structured damage results**

Define `DamageBreakdown(int FinalDamage, int HighGroundBonus, int HighGroundProtection, int LowCoverProtection)` plus `ModifierSummary`, returning only active clauses such as `High ground +2` and `Low cover -1`.

- [ ] **Step 4: Centralize tracing, sight, cover, and damage**

`Trace` returns unique intermediate cells in deterministic order. `Los` consults `BlocksSight`; `HasLowCover` checks traced intermediate cells. Apply ranged-only modifiers after the existing base formula, keep stun-only at 0 and non-stun minimum at 1. Keep `PreviewDamage` as the final integer wrapper.

- [ ] **Step 5: Make enemy scoring use the shared breakdown**

Evaluate candidate attacks from candidate cells without mutating the unit merely to preview. Rank by actual final damage while preserving current target priorities and Ompok behavior.

- [ ] **Step 6: Verify and commit**

Run logic tests and build; expect all passing and 0 errors. Commit as `feat: apply cover and elevation to combat`.

### Task 5: Render Height-and-Shadows Terrain and Feedback

**Files:**
- Create: `Scripts/UI/TerrainRenderer.cs`
- Modify: `Scripts/UI/BattleView.cs`
- Modify: `README.md`

**Interfaces:**
- Consumes: `TerrainAt`, `TerrainKind`, mission definitions, origin, and tile size.
- Produces: `TerrainRenderer.Draw(CanvasItem, BattleState, Vector2 origin, int tileSize)` and terrain labels/descriptions.

- [ ] **Step 1: Implement the generated renderer**

Give each mission a restrained palette. Draw floors, joined raised walls, isolated pillars, low crates with short shadows/top faces, and elevated containers with deeper shadows/top faces and a small upward chevron.

- [ ] **Step 2: Replace painted underlays**

Remove the full-board environment-atlas draw. Keep the atlas only for console, shuttle, and bridge icons. Render translucent cyan/red/gold overlays above terrain and below units.

- [ ] **Step 3: Add legend and hover help**

Show: `Solid — blocks movement + shots`, `Low cover — blocks movement, -1 ranged damage`, and `High ground — climb +2 move, ranged advantage`. Hovered terrain uses the existing bottom help console without replacing active ability help.

- [ ] **Step 4: Add Mission 1 teaching moments**

Teach low cover when a relevant shot appears and high ground when climbing or an elevation-modified shot becomes relevant. Prompts disappear after the lesson action/dismissal and never block input.

- [ ] **Step 5: Show terrain modifiers in combat feedback**

Use `PreviewDamageBreakdown` in attack hints and combat panels; append active modifier clauses, leaving ordinary attacks concise.

- [ ] **Step 6: Update README and verify**

Document generated terrain/cover/high ground and temporary removal of painted underlays. Run the build, launch Godot 4.7.2 .NET, open Mission 1, and verify every terrain type is distinct before unit selection.

- [ ] **Step 7: Commit**

Commit as `feat: render readable raised battlefield terrain`.

### Task 6: Use Legal Paths in Animation and Playtest All Missions

**Files:**
- Modify: `Scripts/UI/BattleView.cs`
- Modify: `Tests/Program.cs`
- Modify: `README.md`

**Interfaces:**
- Consumes: model paths, weighted reach, damage breakdowns, and final mission definitions.
- Produces: player/enemy animations that follow model-approved paths and a verified terrain release.

- [ ] **Step 1: Add final map regression tests**

For each mission, assert no unit/objective starts on blocked terrain, every player unit has a legal move, and objective routes remain available after excluding occupied starting cells. Retain the Mission 3 two-route assertion.

- [ ] **Step 2: Run tests before presentation changes**

Expected: map tests pass, establishing that later movement problems are presentation bugs.

- [ ] **Step 3: Replace visual path guessing**

Capture `Path(actor, destination)` before player movement and animate it. Have enemy actions expose or retain their chosen model-approved path; remove `EnemyVisualPath` guessing. Animate undo by reversing the stored player route when valid, otherwise use the current legal return path.

- [ ] **Step 4: Run complete automated verification**

Run logic tests and `dotnet build FailedOverride.csproj`. Expect every test `PASS`, 0 errors, and no new warnings.

- [ ] **Step 5: Playtest Mission 1**

Verify terrain clarity, lessons, cover/high ground, console feedback, legal animations, rescue, wrap-up, and upgrades.

- [ ] **Step 6: Playtest Mission 2**

Verify Issac starts with the crew, cannot evacuate in one turn, ground/climbing routes are useful, heavy droid stays distinctive, repair/reprogram work, and evacuation completes.

- [ ] **Step 7: Playtest Mission 3**

Verify two Ompok bridge approaches, no single-tile trivial blockade, hidden enemies, healing, terrain effects for both teams, and correct victory/defeat.

- [ ] **Step 8: Inspect and commit**

Run `git status --short` and `git diff --check`; ensure no caches or exports are tracked. Commit as `fix: animate units along legal terrain paths`.

- [ ] **Step 9: Record the release version**

Update the README to the next version in the existing history and commit the version-only change if it was not part of Step 8.
