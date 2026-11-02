# Ranged Attack Projectiles Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Animate distinct hero plasma bolts and enemy laser beams on the board before the existing click-to-dismiss combat portrait appears.

**Architecture:** Add a small, deterministic `RangedAttackPresentationState` that classifies ranged attacks by allegiance and advances normalized effect timing without depending on Godot drawing APIs. `BattleView` will start that state after attack resolution, draw the appropriate bolt/beam and impact flash, preserve the target's pre-hit health display until impact, then open the existing combat portrait panel.

**Tech Stack:** Godot 4 C#, .NET, the existing executable logic-test harness, Godot `Node2D` drawing APIs.

**Spec:** `docs/superpowers/specs/2026-11-01-ranged-attack-projectiles-design.md`

## Global Constraints

- Hero ranged attacks use a fast cyan-blue plasma bolt with a short glowing trail and a small impact flash.
- Enemy ranged attacks use a red beam that briefly connects attacker and target, followed by a red impact flash.
- Hero timing is approximately 0.25 seconds; enemy timing is approximately 0.4 seconds.
- The existing portrait panel appears only after the board effect and remains until clicked.
- Melee attacks, combat rules, AI, sounds, and turn flow retain their current behavior.
- Unsupported attacks safely fall back to the current portrait presentation.

## Review Focus

- Melee attacks must skip projectile presentation and show the portrait normally.
- Allied summoned robots and reprogrammed droids must use the hero effect.
- Enemy units of every ranged role must use the enemy effect based on team rather than role.
- A large frame-time jump must complete the effect exactly once without leaving input blocked.
- A defeated target must remain drawable through the impact before its portrait appears.

### Task 1: Deterministic projectile presentation state

- Add `RangedAttackEffect` (`None`, `HeroBolt`, `EnemyBeam`) and `RangedAttackPresentationState` with `Start(Team, int)`, `Advance(double)`, `Active`, `Effect`, `Progress`, `ImpactProgress`, and `CompletedThisFrame`.
- Test melee classification, team classification, 0.25-second hero timing, 0.4-second enemy timing, and one-shot completion on a large frame delta.
- Run all logic tests and commit.

### Task 2: Board rendering and combat-presentation integration

- Route successful ranged player and enemy attacks through the presentation state; leave melee and unsupported actions on the current path.
- Synchronize ranged sound, impact, and portrait timing; keep the portrait click-to-dismiss.
- Draw a cyan-white moving hero bolt with trail and impact flash.
- Draw a red enemy beam with outer glow, bright core, and impact flash.
- Show pre-impact HP until impact and keep defeated targets drawable through the effect.
- Run logic tests, build the solution, smoke-launch Godot, and commit.

### Task 3: Whole-change verification and test build

- Review every ranged attack path and team-allegiance case.
- Run the clean logic test suite and solution build.
- Export, sign, package, verify, and smoke-launch a macOS UDF DMG.

