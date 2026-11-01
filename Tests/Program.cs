using FailedOverride.Battle;
using FailedOverride.UI;
using FailedOverride.Units;

static class Test
{
    static int failures;

    public static void Run(string name, Action test)
    {
        try { test(); Console.WriteLine($"PASS {name}"); }
        catch (Exception ex) { failures++; Console.WriteLine($"FAIL {name}: {ex.Message}"); }
    }

    public static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new Exception($"expected {expected}, got {actual}");
    }

    public static void True(bool value, string message)
    {
        if (!value) throw new Exception(message);
    }

    public static int ExitCode => failures == 0 ? 0 : 1;
}

static class Program
{
    public static int Main()
    {
        Test.Run("terrain_kinds_expose_correct_rules", () =>
        {
            Test.True(new TerrainCell(TerrainKind.Floor).Walkable, "floor walkable");
            Test.True(!new TerrainCell(TerrainKind.Wall).Walkable, "wall blocked");
            Test.True(new TerrainCell(TerrainKind.Wall).BlocksSight, "wall blocks sight");
            Test.True(new TerrainCell(TerrainKind.Pillar).BlocksSight, "pillar blocks sight");
            Test.True(!new TerrainCell(TerrainKind.LowCover).BlocksSight, "cover permits sight");
            Test.Equal(1, new TerrainCell(TerrainKind.Elevated).Elevation);
        });

        Test.Run("mission_definitions_are_valid", () =>
        {
            for (var mission = 1; mission <= 3; mission++)
            {
                var definition = MissionDefinition.For(mission);
                Test.Equal(12, definition.Terrain.GetLength(0));
                Test.Equal(12, definition.Terrain.GetLength(1));
                foreach (var spawn in definition.Spawns)
                    Test.True(definition.At(spawn.Position).Walkable, $"mission {mission} invalid spawn {spawn.Position}");
            }
            Test.True(MissionDefinition.For(1).Console.HasValue, "mission 1 console");
            Test.True(MissionDefinition.For(2).Shuttle.HasValue, "mission 2 shuttle");
            Test.True(MissionDefinition.For(3).Bridge.HasValue, "mission 3 bridge");
        });

        Test.Run("weighted_movement_costs_and_paths", () =>
        {
            var battle = new BattleState(1);
            Test.Equal(1, battle.MovementCost(new Cell(4, 7), new Cell(5, 7)));
            Test.Equal(3, battle.MovementCost(new Cell(5, 7), new Cell(5, 6)));
            Test.Equal(1, battle.MovementCost(new Cell(5, 6), new Cell(6, 6)));
            Test.Equal(1, battle.MovementCost(new Cell(5, 6), new Cell(5, 7)));
            var xander = battle.Units.Single(u => u.Id == "xander");
            var destination = new Cell(1, 9);
            var path = battle.Path(xander, destination);
            Test.True(path.Count >= 2, "path exists");
            Test.Equal(xander.Position, path[0]);
            Test.Equal(destination, path[^1]);
            Test.True(path.All(c => battle.TerrainAt(c).Walkable), "path stays walkable");
        });

        Test.Run("cover_and_elevation_modify_ranged_damage", () =>
        {
            var battle = new BattleState(1);
            var attacker = battle.Units.Single(u => u.Id == "xander");
            var target = battle.Units.First(u => u.Role == Role.Droid);
            attacker.Position = new Cell(1, 8); target.Position = new Cell(5, 8);
            var covered = battle.PreviewDamageBreakdown(attacker, target, new Weapon("Test rifle", 8, 6));
            Test.Equal(1, covered.LowCoverProtection);
            Test.True(battle.Los(attacker.Position, target.Position), "low cover permits sight");
            attacker.Position = new Cell(5, 6); target.Position = new Cell(5, 7);
            var downhill = battle.PreviewDamageBreakdown(attacker, target, new Weapon("Test rifle", 8, 3));
            Test.Equal(2, downhill.HighGroundBonus);
            attacker.Position = new Cell(5, 7); target.Position = new Cell(5, 6);
            var uphill = battle.PreviewDamageBreakdown(attacker, target, new Weapon("Test rifle", 8, 3));
            Test.Equal(1, uphill.HighGroundProtection);
            var melee = battle.PreviewDamageBreakdown(attacker, target, new Weapon("Blade", 8, 1));
            Test.Equal(0, melee.HighGroundProtection);
        });

        Test.Run("movement_records_the_legal_animation_path", () =>
        {
            var battle = new BattleState(1);
            var xander = battle.Units.Single(u => u.Id == "xander");
            Test.True(battle.Move(xander, new Cell(1, 9)), "move succeeds");
            Test.Equal(new Cell(1, 10), battle.LastMovePath[0]);
            Test.Equal(new Cell(1, 9), battle.LastMovePath[^1]);
            Test.True(battle.LastMovePath.All(c => battle.TerrainAt(c).Walkable), "animation path is legal");
        });

        Test.Run("undo_reverses_the_original_route_after_descending", () =>
        {
            var battle = new BattleState(1);
            var xander = battle.Units.Single(u => u.Id == "xander");
            xander.Position = new Cell(5, 6);
            Test.True(battle.Move(xander, new Cell(8, 9)), "downhill move succeeds");
            Test.True(battle.UndoMove(xander), "undo succeeds");
            Test.True(battle.LastMovePath.Count > 1, "undo has animation path");
            Test.Equal(new Cell(8, 9), battle.LastMovePath[0]);
            Test.Equal(new Cell(5, 6), battle.LastMovePath[^1]);
        });

        Test.Run("unit_turn_completes_after_move_then_action_or_action_then_move", () =>
        {
            var moveThenAct = new BattleState(1);
            var first = moveThenAct.Units.Single(u => u.Id == "xander");
            var firstTarget = moveThenAct.Units.First(u => u.Role == Role.Droid);
            firstTarget.Position = new Cell(2, 9);
            Test.True(moveThenAct.Move(first, new Cell(1, 9)), "first unit moves");
            Test.True(!moveThenAct.TurnComplete(first), "moving alone keeps the unit ready");
            Test.True(moveThenAct.Attack(first, firstTarget, 1), "first unit attacks after moving");
            Test.True(moveThenAct.TurnComplete(first), "an action after movement completes the unit");
            Test.Equal("nance", moveThenAct.NextReadyPlayer()!.Id);

            var actThenMove = new BattleState(1);
            var second = actThenMove.Units.Single(u => u.Id == "xander");
            var secondTarget = actThenMove.Units.First(u => u.Role == Role.Droid);
            secondTarget.Position = new Cell(2, 10);
            actThenMove.Units.Single(u => u.Id == "nance").Position = new Cell(4, 10);
            actThenMove.Units.Single(u => u.Id == "akiko").Position = new Cell(5, 10);
            Test.True(actThenMove.Attack(second, secondTarget, 1), "second unit attacks before moving");
            Test.True(!actThenMove.TurnComplete(second), "acting alone leaves movement available");
            Test.True(actThenMove.Move(second, new Cell(1, 9)), "second unit moves after acting");
            Test.True(actThenMove.TurnComplete(second), "movement after an action completes the unit");
            Test.Equal("nance", actThenMove.NextReadyPlayer()!.Id);
        });

        Test.Run("player_turn_completion_ignores_defeated_and_captured_units", () =>
        {
            var battle = new BattleState(1);
            var xander = battle.Units.Single(u => u.Id == "xander");
            var nance = battle.Units.Single(u => u.Id == "nance");
            var akiko = battle.Units.Single(u => u.Id == "akiko");
            xander.Hp = 0;
            nance.Captured = true;
            akiko.Moved = akiko.Acted = true;

            Test.True(battle.PlayerTurnComplete(), "inactive units do not block the enemy phase");
        });

        Test.Run("selection_change_waits_only_for_acted_unmoved_unit", () =>
        {
            var battle = new BattleState(1);
            var xander = battle.Units.Single(u => u.Id == "xander");
            var nance = battle.Units.Single(u => u.Id == "nance");
            xander.Acted = true;

            Test.True(battle.WaitForSelectionChange(xander, nance), "switching commits an acted hero");
            Test.True(battle.TurnComplete(xander), "automatic Wait completes the previous hero");
            Test.Equal("Xander McRaven waits.", battle.Log[^1]);

            var sameUnit = new BattleState(1);
            var sameXander = sameUnit.Units.Single(u => u.Id == "xander");
            sameXander.Acted = true;
            Test.True(!sameUnit.WaitForSelectionChange(sameXander, sameXander), "reselecting does not wait");
            Test.True(!sameXander.Moved, "reselecting preserves movement");

            var untouched = new BattleState(1);
            var untouchedXander = untouched.Units.Single(u => u.Id == "xander");
            var untouchedNance = untouched.Units.Single(u => u.Id == "nance");
            Test.True(!untouched.WaitForSelectionChange(untouchedXander, untouchedNance), "switching before acting is free");
            Test.True(!untouchedXander.Moved, "untouched hero keeps movement");

            var moved = new BattleState(1);
            var movedXander = moved.Units.Single(u => u.Id == "xander");
            var movedNance = moved.Units.Single(u => u.Id == "nance");
            movedXander.Moved = true;
            Test.True(!moved.WaitForSelectionChange(movedXander, movedNance), "moving without acting does not wait");
            Test.True(!movedXander.Acted, "moved hero keeps the action");
        });

        Test.Run("ompok_routes_around_a_blocked_bridge_approach", () =>
        {
            var battle = new BattleState(3);
            var ompok = battle.Units.Single(u => u.Role == Role.OmpokProxy);
            var blocker = battle.Units.Single(u => u.Id == "xander");
            ompok.Position = new Cell(4, 1); blocker.Position = new Cell(5, 1);
            battle.StartEnemy();
            for (var i = 0; i < 12 && battle.BridgeProgress == 0; i++) battle.Enemy(ompok);
            Test.True(battle.BridgeProgress > 0, "Ompok uses the open lower route");
        });

        Test.Run("battle_constructs_all_three_missions", () =>
        {
            for (var mission = 1; mission <= 3; mission++)
            {
                var battle = new BattleState(mission);
                Test.Equal(12, battle.Map.Length);
                Test.True(battle.Map.All(row => row.Length == 12), $"mission {mission} width");
                Test.True(battle.Units.Any(unit => unit.Team == Team.Player), $"mission {mission} player spawn");
            }
        });

        Test.Run("ranged_attack_effects_follow_weapon_range_and_allegiance", () =>
        {
            var presentation = new RangedAttackPresentationState();

            presentation.Start(Team.Player, 1);
            Test.Equal(RangedAttackEffect.None, presentation.Effect);
            Test.True(!presentation.Active, "melee attacks skip projectile presentation");

            presentation.Start(Team.Player, 4);
            Test.Equal(RangedAttackEffect.HeroBolt, presentation.Effect);
            Test.True(presentation.Active, "player ranged attacks use the hero bolt");

            presentation.Start(Team.Enemy, 5);
            Test.Equal(RangedAttackEffect.EnemyBeam, presentation.Effect);
            Test.True(presentation.Active, "enemy ranged attacks use the enemy beam");
        });

        Test.Run("hero_bolt_completes_after_quarter_second", () =>
        {
            var presentation = new RangedAttackPresentationState();
            presentation.Start(Team.Player, 4);

            presentation.Advance(0.125);
            Test.True(presentation.Active, "hero bolt remains active at its midpoint");
            Test.Equal(0.5f, presentation.Progress);
            Test.True(!presentation.CompletedThisFrame, "midpoint is not complete");

            presentation.Advance(0.125);
            Test.True(!presentation.Active, "hero bolt completes at 0.25 seconds");
            Test.Equal(1f, presentation.Progress);
            Test.True(presentation.CompletedThisFrame, "completion is reported on the finishing frame");
        });

        Test.Run("enemy_beam_completes_after_four_tenths_second", () =>
        {
            var presentation = new RangedAttackPresentationState();
            presentation.Start(Team.Enemy, 5);

            presentation.Advance(0.2);
            Test.True(presentation.Active, "enemy beam remains active at its midpoint");
            Test.Equal(0.5f, presentation.Progress);

            presentation.Advance(0.2);
            Test.True(!presentation.Active, "enemy beam completes at 0.4 seconds");
            Test.True(presentation.CompletedThisFrame, "beam completion is reported");
        });

        Test.Run("ranged_effect_large_delta_completes_only_once", () =>
        {
            var presentation = new RangedAttackPresentationState();
            presentation.Start(Team.Player, 3);

            presentation.Advance(1.0);
            Test.True(!presentation.Active, "large delta completes the effect");
            Test.True(presentation.CompletedThisFrame, "large delta reports completion");
            Test.Equal(1f, presentation.ImpactProgress);

            presentation.Advance(1.0);
            Test.True(!presentation.CompletedThisFrame, "completion is not repeated");
        });

        Test.Run("ranged_effect_preserves_preimpact_health", () =>
        {
            var presentation = new RangedAttackPresentationState();
            presentation.Prepare(Team.Enemy, 4);

            Test.Equal(16, presentation.DisplayedTargetHp(16, 9));
            Test.True(presentation.Pending, "prepared effect remains pending during movement");
            Test.True(presentation.Active, "prepared effect preserves the target during movement");
            Test.True(presentation.BlocksGameplayInput, "pending combat blocks commands");
            Test.True(presentation.DefersBattleResult, "pending combat defers result UI");

            presentation.Activate();
            presentation.Advance(0.32);
            Test.True(presentation.ImpactStartedThisFrame, "impact starts at eighty percent");
            Test.Equal(9, presentation.DisplayedTargetHp(16, 9));

            presentation.Advance(0.4);
            Test.Equal(9, presentation.DisplayedTargetHp(16, 9));
            Test.True(presentation.Pending, "portrait presentation remains pending after impact");
            Test.True(presentation.BlocksGameplayInput, "portrait still blocks commands");

            presentation.CompletePresentation();
            Test.True(!presentation.Pending, "dismissal completes the presentation");
            Test.True(!presentation.BlocksGameplayInput, "commands resume after dismissal");
            Test.True(!presentation.DefersBattleResult, "result UI may appear after dismissal");
        });

        Test.Run("enemy_turn_pacing_waits_then_previews", () =>
        {
            var pacing = new EnemyTurnPacingState();
            pacing.BeginIntermission();

            pacing.Advance(0.99);
            Test.Equal(EnemyTurnPacingPhase.Intermission, pacing.Phase);
            Test.True(!pacing.PreviewStartedThisFrame, "preview does not start before one second");

            pacing.Advance(0.01);
            Test.Equal(EnemyTurnPacingPhase.Preview, pacing.Phase);
            Test.True(pacing.PreviewStartedThisFrame, "preview starts at one second");
            Test.True(!pacing.ActionReadyThisFrame, "enemy remains stationary when preview begins");

            pacing.Advance(0.59);
            Test.Equal(EnemyTurnPacingPhase.Preview, pacing.Phase);
            Test.True(!pacing.ActionReadyThisFrame, "preview lasts six tenths of a second");

            pacing.Advance(0.01);
            Test.Equal(EnemyTurnPacingPhase.Idle, pacing.Phase);
            Test.True(pacing.ActionReadyThisFrame, "enemy acts after the preview");
        });

        Test.Run("enemy_turn_pacing_large_delta_preserves_preview_frame", () =>
        {
            var pacing = new EnemyTurnPacingState();
            pacing.BeginIntermission();

            pacing.Advance(4.0);

            Test.Equal(EnemyTurnPacingPhase.Preview, pacing.Phase);
            Test.True(pacing.PreviewStartedThisFrame, "large delta still begins a preview");
            Test.True(!pacing.ActionReadyThisFrame, "large delta cannot skip the preview frame");
        });

        Test.Run("tutorial_dismissal_preserves_active_lesson", () =>
        {
            var presentation = new TutorialPresentationState();
            presentation.Show("move");

            presentation.Dismiss();

            Test.Equal("move", presentation.LessonId);
            Test.True(!presentation.IsVisible, "dismissal hides the current presentation");
        });

        Test.Run("tutorial_incomplete_lesson_can_be_presented_again", () =>
        {
            var presentation = new TutorialPresentationState();
            presentation.Show("high-ground");
            presentation.Dismiss();

            presentation.Show("high-ground");

            Test.Equal("high-ground", presentation.LessonId);
            Test.True(presentation.IsVisible, "the incomplete lesson is visible again");
        });

        Test.Run("tutorial_completion_clears_lesson", () =>
        {
            var presentation = new TutorialPresentationState();
            presentation.Show("attack");

            presentation.Complete();

            Test.Equal("", presentation.LessonId);
            Test.True(!presentation.IsVisible, "completion hides the presentation");
        });

        Test.Run("tutorial_reset_clears_presentation", () =>
        {
            var presentation = new TutorialPresentationState();
            presentation.Show("console");

            presentation.Reset();

            Test.Equal("", presentation.LessonId);
            Test.True(!presentation.IsVisible, "reset hides the presentation");
        });

        return Test.ExitCode;
    }
}
