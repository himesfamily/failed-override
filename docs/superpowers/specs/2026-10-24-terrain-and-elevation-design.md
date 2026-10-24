# Terrain Clarity and Elevation Design

## Intent

Redesign all three tactical battlefields so players can immediately understand why a unit can or cannot move or shoot through a space. The maps should become easier to read before they become more decorative. The current painted backgrounds will be removed from active battle rendering for now; polished environmental artwork can return later, after the terrain rules and layouts have been playtested.

The new maps should create useful tactical choices through solid structures, low cover, and climbable high ground while preserving each mission's story, objectives, characters, and approximate difficulty.

## Goals

- Make walkable, blocked, climbable, and protective terrain visually distinct without requiring the movement overlay.
- Use the chosen **height-and-shadows** visual direction: raised edges and cast shadows communicate physical height.
- Give all three missions deliberately designed terrain rather than decorative backgrounds that do not match their collision maps.
- Add simple, predictable elevation and cover rules that work for players and enemy AI.
- Keep click-to-move, attack selection, undo movement, combat presentation, tutorials, and mission progression working.
- Make terrain definitions extensible so later environment artwork can replace the clean shapes without changing gameplay rules.

## Non-goals

- Creating final polished environment art or a complete environment tile atlas.
- Adding destructible terrain, pushing, falling damage, ladders, ramps, jumping, or multiple elevation levels.
- Adding hit percentages or random cover misses; combat remains deterministic.
- Changing character abilities, mission objectives, campaign progression, or the general difficulty curve except where terrain requires small enemy-placement adjustments.
- Reworking unit portraits, tokens, music, cinematics, menus, or feedback features.

## Anti-goals

- A map where artwork suggests a barrier but the rules allow passage, or where an apparently open tile is secretly blocked.
- Terrain that can be understood only after selecting a character.
- Busy tactical markings that compete with character art.
- Elevation rules that require players to calculate several conditional bonuses.

## Chosen Approach

Use a clean, generated terrain renderer now, with gameplay data as the single source of truth. Each tile's terrain type controls both its appearance and rules. Existing full-board environment paintings will not render beneath the tactical grid during this phase.

The selected visual direction is **Option C: height and shadows**. Walls, pillars, crates, and elevated containers receive distinct silhouettes, top faces, raised edges, and shadows. Mission-specific palettes retain a sense of place without sacrificing clarity.

Later, final environmental tiles can be drawn to the same terrain categories and replace the generated shapes without changing pathfinding or combat.

## Approaches Considered

### 1. Clean generated terrain — selected

Render terrain from consistent shapes, colors, edges, and shadows. This is quickest to revise during playtesting and gives the strongest correspondence between appearance and rules.

### 2. Existing artwork with stronger overlays

Keep the painted backgrounds and place bold collision shapes over them. This preserves atmosphere, but decorative features can still conflict with the tactical layout and obscure why a tile behaves as it does.

### 3. Complete environmental tile-art set

Create polished floor, wall, cargo, pillar, and elevation tiles for every setting immediately. This should eventually produce the best-looking result, but it would lock in visual work before the new terrain rules and maps have been tested.

## Terrain Model

Replace character-based `#` checks with an explicit terrain definition per cell. Mission maps may remain compact text grids, but each terrain symbol must map to a named terrain type and its rules.

| Terrain | Movement | Gunfire | Defensive effect | Elevation |
|---|---|---|---|---|
| Floor | Normal, cost 1 | Passes through | None | 0 |
| Solid wall | Blocked | Blocks line of sight | Complete obstruction | — |
| Pillar | Blocked | Blocks line of sight | Complete obstruction | — |
| Low crate | Blocked | Passes over | Reduces ranged damage by 1 when the shot path crosses it | 0 |
| Elevated container | Enterable | Passes over | High-ground rules | 1 |
| Objective floor | Normal, cost 1 | Passes through | None | 0 |

Walls and pillars have the same collision and line-of-sight rules but different shapes. Walls form continuous boundaries; pillars are isolated structural blockers.

Low cover applies only when at least one low-crate cell lies on the traced shot path between attacker and defender. It does not reduce melee damage. Multiple crossed crates do not stack; the total low-cover reduction is 1.

## Elevation Rules

- Elevated containers have one raised level; no terrain is higher than level 1.
- A unit may climb onto an elevated container from any orthogonally adjacent side.
- Entering elevated terrain from floor costs 3 movement points total: 1 normal step plus a 2-point climbing cost.
- Moving between adjacent elevated tiles costs 1.
- Descending from elevated terrain to floor costs 1.
- Elevated tiles are valid destinations and can hold one unit normally.
- A ranged attack from elevation 1 against a target at elevation 0 deals **+2 damage**.
- A ranged attack from elevation 0 against a target at elevation 1 deals **1 less damage**.
- These high-ground modifiers apply after the normal weapon/level/bond/armor calculation, while preserving the game's minimum-damage behavior.
- Low cover and high-ground protection can both apply to the same ranged attack and stack, for at most 2 points of defensive reduction.
- Melee attacks may target an orthogonally adjacent enemy across a one-level elevation change. Melee receives no elevation bonus or penalty.
- Summoned robots may occupy and climb elevated containers under the same rules as other units.

## Movement, Range, and Undo

Uniform breadth-first movement must become weighted pathfinding so climbing consumes additional movement. The reachable-tile result continues to expose the least movement cost for every reachable cell. It must still respect occupied cells, special passage restrictions, and map bounds.

Movement animation should follow the selected least-cost path rather than interpolate directly through blocked terrain. The existing move-undo feature returns the unit to its original tile and restores its unused movement/action state exactly as before.

Attack range continues to use the existing grid-distance rules. Line of sight stops at walls and pillars but passes over low crates and elevated containers. Units do not block line of sight.

## Combat and Feedback

- Damage preview must include low cover and elevation modifiers and show the actual deterministic result.
- When a modifier applies, the side-panel hint and combat result should name it in plain language, for example `High ground +2` or `Low cover -1`.
- Selecting a unit still shows cyan reachable cells, but the underlying terrain remains legible through a translucent highlight.
- Attack overlays remain red and interaction/summon overlays remain gold.
- Hover help should explain each terrain type. A compact always-visible legend should identify wall/pillar, low cover, and high ground.
- The tutorial in Mission 1 should introduce low cover the first time a relevant shot is available and high ground the first time a player can climb or make an elevation-modified attack. Tutorial prompts must not prevent free play after dismissal.

## Enemy AI

Enemy movement uses the same weighted reachability and terrain costs as player movement. When comparing legal move-and-attack choices, the AI evaluates the actual predicted damage, including cover and elevation, instead of only weapon power and armor.

The AI should prefer higher predicted damage but does not need a separate long-term strategy for claiming high ground. This keeps the initial change understandable and avoids turning terrain work into a full AI redesign. Ompok's bridge behavior remains objective-first and uses weighted movement to approach the bridge.

## Visual Language

The board will use clean generated shapes:

- **Floor:** flat, lightly varied panels with a subtle grid.
- **Wall:** near-black raised mass with a clear top edge and deep cast shadow; continuous wall cells visually join.
- **Pillar:** isolated raised structural column with a strong circular or squared silhouette and shadow.
- **Low crate:** waist-high cargo shape with a visible top face and short shadow; lower-looking than a unit and elevated container.
- **Elevated container:** broad raised platform with a brighter top face, deeper shadow, and a small upward-chevron marker until players learn the convention.
- **Objective floor:** ordinary walkable floor plus the existing console, shuttle, or bridge art/icon so objective rules remain recognizable.

Units always render above terrain. Highlights render above the floor/top surface but below units and important objective icons. Shadows may not extend far enough to cover neighboring units or disguise tile boundaries.

Each mission gets a restrained palette while retaining the same shapes and height conventions:

- Mission 1, research outpost: cool blue-gray laboratory flooring, dark partitions, restrained amber cargo.
- Mission 2, docking bay: industrial gray flooring, many amber/brown crates and containers, strong dock structure and pillars.
- Mission 3, crew ship cargo/bridge route: darker navy ship flooring, bulkhead walls, freight containers, and a visually protected bridge area.

## Mission Redesign

All maps remain 12×12 so the existing board and side panel fit without changing the application window.

### Mission 1: Failed Override

Teach the terrain vocabulary gradually. Heroes begin together in a clear staging area. The central route contains low crates that demonstrate shooting over cover. One optional raised container offers a high-ground firing position without being required to rescue Issac. Walls and pillars divide the laboratory into readable rooms and lanes. The console remains reachable as an optional tactical aid. Issac remains far enough from the starting area that the player must engage with the battlefield.

### Mission 2: Evacuation / Docking Bay

Make this the strongest cargo-and-elevation map. Issac starts with the group, while the shuttle remains across the bay. Several freight stacks create alternate routes: a safer ground route with cover and a shorter route that spends movement climbing containers. Solid dock pillars and walls are visually unmistakable and block both travel and shots. Enemy placement should contest routes without allowing an immediate attack on the entire squad or an effortless one-turn evacuation.

### Mission 3: Unexpected Cargo

Use ship bulkheads and freight containers to create a defense-in-depth battle from the cargo area toward the bridge. Raised cargo gives defenders firing positions, but Ompok must retain at least two viable routes toward the bridge so one occupied choke point cannot trivialize the mission. Hidden enemies, Chrandy, the bridge takeover, and current victory/defeat rules remain intact. Hidden enemy starting cells must not overlap or become inaccessible because of terrain.

Exact cell layouts and modest enemy-position adjustments are left to the builder, provided they meet these mission constraints and pass playability checks.

## Data and Code Boundaries

- Add a terrain value/model in the battle domain rather than encoding all rules in the drawing code.
- `BattleState` owns terrain queries, movement cost, elevation, line of sight, cover detection, and damage modifiers.
- The UI asks the battle model for reachable cells, paths, terrain descriptions, and damage breakdowns; it does not independently reproduce game rules.
- `BattleView` owns generated terrain rendering, highlights, animation, hints, and legend presentation.
- Mission definitions should keep terrain, units, and objectives close together but distinguish them conceptually so future missions can be added without expanding a single unreadable constructor expression.
- Existing campaign and unit concepts remain separate from battle-map terrain.

## Compatibility and Save Data

No save-file migration is required because active battle maps are constructed at mission start and are not serialized. Existing campaign progress, upgrades, bond data, sound/window settings, and completed-mission records must continue to load.

## Verification

Automated or focused model-level checks should cover:

- Floor, wall, pillar, low-crate, and elevated-container movement rules.
- Climbing cost, descending cost, and least-cost reachability.
- Line of sight blocked by walls/pillars but not crates/containers.
- Low cover applying only to ranged shot paths and never stacking above 1.
- High-ground attack and defense modifiers, including their combination with cover and minimum damage.
- Player and enemy damage previews agreeing with resolved damage.
- AI selecting legal paths and using terrain-adjusted damage evaluation.
- Every hero, enemy, captive, objective, and shuttle/bridge cell spawning on valid terrain.
- Each mission having a valid route from player starting area to its objective.
- Mission 3 retaining at least two viable approaches to the bridge.

Manual play checks should verify that each terrain type is recognizable without selecting a character, overlays remain readable, movement animations follow valid paths, combat panels report terrain modifiers, and all three missions can be completed without regression.

## Builder Calls

The builder may decide:

- Exact colors, shadow offsets, and small decorative floor markings within the chosen height-and-shadows direction.
- Exact redesigned cell layouts and enemy positions, subject to the mission constraints above.
- Internal names and file organization for terrain definitions and path results.
- Whether the terrain legend is always shown in the side panel or compactly positioned near the board, provided it is visible during play.

