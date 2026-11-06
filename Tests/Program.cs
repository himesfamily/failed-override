using FailedOverride.Battle;
using FailedOverride.Campaign;
using FailedOverride.UI;
using FailedOverride.Units;
using System.Text.Json;

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
        Test.Run("mission_two_intro_preserves_the_approved_cast_and_story", () =>
        {
            var beats = MissionTwoIntroScript.Beats;
            Test.Equal(8, beats.Count);
            Test.Equal("ISSAC WYNTER", beats[0].Speaker);
            Test.True(beats[0].Dialogue.Contains("We were set up"), "intro explains the trap");
            Test.True(beats.Any(b => b.Dialogue.Contains("Emperor Ompok")), "intro names Ompok");
            Test.True(beats.Any(b => b.Dialogue.Contains("Cheromia computer core")), "intro explains the threat");
            Test.Equal("XANDER MCRAVEN", beats[^1].Speaker);
            Test.True(beats[^1].Dialogue.Contains("THIS moment"), "Xander announces the next wave");
            var allText = string.Join(" ", beats.Select(b => $"{b.Speaker} {b.Portrait} {b.Dialogue}"));
            Test.True(!allText.Contains("Tiger", StringComparison.OrdinalIgnoreCase), "Tiger remains unrevealed");
            Test.True(!allText.Contains("twin", StringComparison.OrdinalIgnoreCase), "the twins remain unrevealed");
            Test.True(!allText.Contains("Chrandy", StringComparison.OrdinalIgnoreCase), "Chrandy remains a silent visual cameo");
            Test.Equal("INTRUSION ALERT", MissionTwoIntroScript.TransitionKicker);
            Test.Equal("STARSHIP AURORA\nCARGO BAY", MissionTwoIntroScript.TransitionTitle);
            Test.True(MissionTwoIntroScript.PreserveFullComposition, "the wide room keeps Issac and Chrandy visible at opposite edges");
            Test.True(!MissionTwoIntroScript.ForegroundPortrait.Intersects(MissionTwoIntroScript.IssacFaceRegion), "speaker portrait leaves room Issac visible");
            Test.True(!MissionTwoIntroScript.ForegroundPortrait.Intersects(MissionTwoIntroScript.ChrandyFaceRegion), "speaker portrait leaves room Chrandy visible");
        });

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
            Test.True(MissionDefinition.For(2).SecurityPassage.HasValue, "mission 2 security passage");
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

        Test.Run("selection_change_completes_any_partial_turn", () =>
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
            Test.True(moved.WaitForSelectionChange(movedXander, movedNance), "switching commits a moved hero");
            Test.True(moved.TurnComplete(movedXander), "moved hero is put in Wait state");
        });

        Test.Run("selection_change_completes_partial_machine_turns", () =>
        {
            var summoned = new BattleState(1);
            var akiko = summoned.Units.Single(u => u.Id == "akiko");
            akiko.ClassProgressions.Add("akiko-combat-2");
            var summonCell = summoned.SummonCells(akiko).First();
            Test.True(summoned.Summon(akiko, summonCell), "support robot is summoned ready to act");
            var robot = summoned.Units.Single(u => u.Role == Role.Robot);
            robot.Moved = true;
            Test.True(summoned.WaitForSelectionChange(robot, summoned.Units.Single(u => u.Id == "xander")), "switching after a robot moves waits");
            Test.True(summoned.TurnComplete(robot), "summoned robot turn completes");
            Test.Equal("Support robot 1 waits.", summoned.Log[^1]);

            var reprogrammed = new BattleState(1);
            var droid = reprogrammed.Units.First(u => u.Role == Role.Droid);
            droid.Team = Team.Player;
            droid.ControllerId = "issac";
            droid.Acted = true;
            Test.True(reprogrammed.WaitForSelectionChange(droid, reprogrammed.Units.Single(u => u.Id == "nance")), "switching after a reprogrammed droid acts waits");
            Test.True(reprogrammed.TurnComplete(droid), "reprogrammed droid turn completes");

            var crowded = new BattleState(1);
            var crowdedAkiko = crowded.Units.Single(u => u.Id == "akiko");
            crowdedAkiko.ClassProgressions.Add("akiko-combat-2");
            Test.True(crowded.Summon(crowdedAkiko, crowded.SummonCells(crowdedAkiko).First()), "first support robot is summoned");
            crowdedAkiko.Acted = false;
            Test.True(crowded.Summon(crowdedAkiko, crowded.SummonCells(crowdedAkiko).First()), "second support robot is summoned");
            var robots = crowded.Units.Where(u => u.Role == Role.Robot).ToList();
            robots[0].Moved = true;
            robots[1].Acted = true;
            Test.True(crowded.WaitForSelectionChange(robots[0], robots[1]), "switching between several robots waits the moved robot");
            Test.True(crowded.WaitForSelectionChange(robots[1], crowded.Units.Single(u => u.Id == "xander")), "switching away waits the acted robot");
            Test.True(robots.All(crowded.TurnComplete), "all partially used robots are complete after switching away");
        });

        Test.Run("enemy_with_no_move_or_attack_waits", () =>
        {
            var battle = new BattleState(1);
            var enemy = battle.Units.First(u => u.Role == Role.Droid);
            enemy.Position = new Cell(10, 1);
            enemy.BaseMove = 0;
            battle.StartEnemy();
            battle.Enemy(enemy);
            Test.Equal($"{enemy.Name} waits.", battle.Log[^1]);

            var bridgeBattle = new BattleState(3);
            var ompok = bridgeBattle.Units.Single(u => u.Role == Role.OmpokProxy);
            ompok.BaseMove = 0;
            bridgeBattle.StartEnemy();
            bridgeBattle.Enemy(ompok);
            Test.Equal("Ompok waits.", bridgeBattle.Log[^1]);
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

        Test.Run("any_crew_member_can_enter_mission_two_security_passage", () =>
        {
            var noPhantomConsole = new BattleState(2);
            var consoleTester = noPhantomConsole.Units.Single(u => u.Id == "xander");
            consoleTester.Position = noPhantomConsole.Console.Neighbors().First(c => noPhantomConsole.Floor(c) && noPhantomConsole.At(c) == null);
            Test.True(!noPhantomConsole.Interact(consoleTester), "mission two has no invisible console interaction");

            var missionOneConsole = new BattleState(1);
            var consoleUser = missionOneConsole.Units.Single(u => u.Id == "xander");
            consoleUser.Position = missionOneConsole.Console.Neighbors().First(c => missionOneConsole.Floor(c) && missionOneConsole.At(c) == null);
            Test.True(missionOneConsole.Interact(consoleUser), "mission one console interaction remains available");
            Test.True(missionOneConsole.ConsoleUsed, "mission one console is marked used");
            Test.True(missionOneConsole.Units.Any(u => u.Team == Team.Enemy && u.Stunned > 0), "mission one console stuns an enemy");

            foreach (var id in new[] { "xander", "nance", "akiko", "issac" })
            {
                var battle = new BattleState(2);
                var actor = battle.Units.Single(u => u.Id == id);
                actor.Position = battle.SecurityPassage.Neighbors().First(c => battle.Floor(c) && battle.At(c) == null);
                var xpBefore = battle.Campaign.Progress(id).Xp;

                Test.True(battle.Move(actor, battle.SecurityPassage), $"{actor.Name} enters the passage");
                Test.Equal(Phase.Victory, battle.Phase);
                Test.Equal(xpBefore + 20, battle.Campaign.Progress(id).Xp);
                Test.Equal(20, battle.LastXpResult!.Amount);
                Test.Equal(id, battle.LastXpResult.CharacterId);
                Test.True(battle.LastMovePath.Count > 1, "winning move keeps its animation path");
                Test.True(battle.Log.Any(line => line == $"{actor.Name} entered the security center. Mission complete!"), "completion identifies the entering character");
            }

            var machineBattle = new BattleState(2);
            var machine = machineBattle.Units.First(u => u.Role == Role.Droid);
            machine.Team = Team.Player;
            machine.ControllerId = "issac";
            machine.Position = machineBattle.SecurityPassage.Neighbors().First(c => machineBattle.Floor(c) && machineBattle.At(c) == null);
            Test.True(machineBattle.Move(machine, machineBattle.SecurityPassage), "reprogrammed droid can stand on the passage");
            Test.Equal(Phase.Player, machineBattle.Phase);
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

        Test.Run("enemy_turn_pacing_skips_dead_queued_enemies", () =>
        {
            var battle = new BattleState(1);
            var enemies = new Queue<Unit>(battle.Units.Where(u => u.Team == Team.Enemy));
            enemies.Peek().Hp = 0;

            var next = EnemyTurnPacingState.TakeNextLiving(enemies);

            Test.True(next != null && next.Alive, "the next living enemy is selected");
            while (enemies.TryDequeue(out var remaining)) remaining.Hp = 0;
            var deadOnly = new Queue<Unit>(battle.Units.Where(u => u.Team == Team.Enemy));
            foreach (var enemy in deadOnly) enemy.Hp = 0;
            Test.True(EnemyTurnPacingState.TakeNextLiving(deadOnly) == null, "dead-only queue has no preview target");
            Test.Equal(0, deadOnly.Count);
        });

        Test.Run("progression_level_two_follows_training_path_and_carries_xp", () =>
        {
            foreach (var path in new[] { TrainingPath.Health, TrainingPath.Armor, TrainingPath.Movement, TrainingPath.Attack })
            {
                var campaign = new CampaignState();
                campaign.SetTrainingPath("xander", path);
                campaign.AwardXp("xander", 125);
                var p = campaign.Progress("xander");
                Test.Equal(2, p.Level);
                Test.Equal(25, p.Xp);
                Test.Equal(path == TrainingPath.Health ? 4 : 0, p.HealthBonus);
                Test.Equal(path == TrainingPath.Armor ? 1 : 0, p.ArmorBonus);
                Test.Equal(path == TrainingPath.Movement ? 1 : 0, p.MovementBonus);
                Test.Equal(path == TrainingPath.Attack ? 1 : 0, p.AttackBonus);
            }
        });

        Test.Run("progression_later_levels_follow_training_path", () =>
        {
            var campaign = new CampaignState();
            campaign.SetTrainingPath("xander", TrainingPath.Armor);
            campaign.AwardXp("xander", 200);
            var p = campaign.Progress("xander");
            Test.Equal(3, p.Level);
            Test.Equal(0, p.HealthBonus);
            Test.Equal(2, p.ArmorBonus);
            campaign.SetTrainingPath("xander", TrainingPath.Attack);
            campaign.AwardXp("xander", 100);
            Test.Equal(1, p.AttackBonus);
        });

        Test.Run("legacy_level_migrates_without_losing_attack_power", () =>
        {
            var campaign = new CampaignState();
            campaign.Levels["xander"] = 4;
            campaign.Experience["xander"] = 30;
            var p = campaign.Progress("xander");
            Test.Equal(4, p.Level);
            Test.Equal(30, p.Xp);
            Test.Equal(3, p.AttackBonus);
        });

        Test.Run("battle_awards_existing_attack_xp_and_utility_xp", () =>
        {
            var battle = new BattleState(1);
            var xander = battle.Units.Single(u => u.Id == "xander");
            var droid = battle.Units.First(u => u.Team == Team.Enemy);
            xander.Position = new Cell(2, 2);
            droid.Position = new Cell(2, 3);
            Test.True(battle.Attack(xander, droid, 1), "damaging attack succeeds");
            Test.Equal(25, battle.Campaign.Progress("xander").Xp);

            var support = new BattleState(1, battle.Campaign);
            var akiko = support.Units.Single(u => u.Id == "akiko");
            var empty = akiko.Position.Neighbors().First(c => support.Floor(c) && support.At(c) == null);
            Test.True(support.Summon(akiko, empty), "summon succeeds");
            Test.Equal(20, support.LastXpAward);
            Test.Equal(20, support.Campaign.Progress("akiko").Xp);
        });

        Test.Run("machine_attack_xp_is_credited_to_controller", () =>
        {
            var battle = new BattleState(1);
            var akiko = battle.Units.Single(u => u.Id == "akiko");
            var empty = akiko.Position.Neighbors().First(c => battle.Floor(c) && battle.At(c) == null);
            Test.True(battle.Summon(akiko, empty), "robot summoned");
            var robot = battle.Units.Single(u => u.Role == Role.Robot);
            robot.Acted = false;
            var droid = battle.Units.First(u => u.Team == Team.Enemy);
            robot.Position = new Cell(2, 2);
            droid.Position = new Cell(2, 3);
            var before = battle.Campaign.Progress("akiko").Xp;
            Test.True(battle.Attack(robot, droid, 0), "robot attacks");
            Test.Equal(before + 25, battle.Campaign.Progress("akiko").Xp);
            Test.Equal(0, robot.Xp);
        });

        Test.Run("experience_awards_queue_until_presented", () =>
        {
            var battle = new BattleState(1);
            var xander = battle.Units.Single(u => u.Id == "xander");
            var akiko = battle.Units.Single(u => u.Id == "akiko");
            var first = battle.Units.First(u => u.Team == Team.Enemy);
            var second = battle.Units.Where(u => u.Team == Team.Enemy).Skip(1).First();
            xander.Position = first.Position.Neighbors().First(battle.Floor);
            akiko.Position = second.Position.Neighbors().First(battle.Floor);

            Test.True(battle.Attack(xander, first, 1), "first attack awards XP");
            Test.True(battle.Attack(akiko, second, 0), "second attack awards XP");

            Test.Equal(2, battle.PendingXpAwardCount);
            Test.True(battle.TryDequeueXpAward(out var firstAward), "first award dequeues");
            Test.Equal("xander", firstAward.CharacterId);
            Test.True(battle.TryDequeueXpAward(out var secondAward), "second award dequeues");
            Test.Equal("akiko", secondAward.CharacterId);
        });

        Test.Run("transformation_never_awards_xp", () =>
        {
            var battle = new BattleState(1);
            var nance = battle.Units.Single(u => u.Id == "nance");
            Test.True(battle.Transform(nance), "transform succeeds");
            Test.Equal(0, battle.Campaign.Progress("nance").Xp);
        });

        Test.Run("rosters_enforce_four_unique_available_crew", () =>
        {
            var campaign=new CampaignState();
            Test.True(campaign.ValidateRoster(1,new[]{"xander","nance","akiko"}),"mission one fixed crew");
            Test.True(campaign.ValidateRoster(3,new[]{"xander","nance","issac","chrandy"}),"mission three four crew");
            Test.True(!campaign.ValidateRoster(3,new[]{"xander","nance","issac"}),"mission three needs four");
            Test.True(!campaign.ValidateRoster(3,new[]{"xander","xander","issac","chrandy"}),"duplicates rejected");
        });

        Test.Run("class_progression_requires_matching_first_tier", () =>
        {
            var campaign=new CampaignState();var p=campaign.Progress("xander");p.PendingClassProgressions=2;
            Test.True(!campaign.ChooseClassProgression("xander","xander-marksman-2"),"tier two locked");
            Test.True(campaign.ChooseClassProgression("xander","xander-marksman-1"),"tier one selected");
            Test.True(campaign.ChooseClassProgression("xander","xander-marksman-2"),"matching tier two unlocked");
            Test.True(!campaign.ChooseClassProgression("xander","xander-marksman-2"),"cannot select twice");
        });

        Test.Run("recon_progressions_reveal_and_reduce_revealed_armor", () =>
        {
            var campaign = new CampaignState();
            campaign.Progress("xander").ClassProgressions.UnionWith(["xander-recon-1", "xander-recon-2"]);
            var battle = new BattleState(3, campaign, ["xander", "nance", "akiko", "chrandy"]);
            var xander = battle.Units.Single(u => u.Id == "xander");
            var hidden = battle.Units.First(u => u.Hidden);
            var destination = hidden.Position.Neighbors().First(c => battle.Floor(c) && battle.At(c) == null);
            xander.Position = destination.Neighbors().First(c => battle.Floor(c) && battle.At(c) == null);

            Test.True(battle.Move(xander, destination), "Xander moves near hidden enemy");
            Test.True(!hidden.Hidden, "Recon Specialist reveals the enemy");
            xander.Acted = false;
            var weapon = xander.Weapons[0];
            Test.Equal(Math.Max(1, weapon.Power + xander.AttackBonus - Math.Max(0, hidden.Armor - 1)), battle.PreviewDamage(xander, hidden, weapon));
        });

        Test.Run("defeat_mobility_progressions_restore_movement", () =>
        {
            var campaign = new CampaignState();
            campaign.Progress("xander").ClassProgressions.UnionWith(["xander-vanguard-1", "xander-vanguard-2"]);
            campaign.Progress("nance").ClassProgressions.UnionWith(["nance-fury-1", "nance-fury-2"]);
            var battle = new BattleState(1, campaign);
            var xander = battle.Units.Single(u => u.Id == "xander");
            var nance = battle.Units.Single(u => u.Id == "nance");
            var enemies = battle.Units.Where(u => u.Team == Team.Enemy).Take(2).ToArray();
            xander.Position = enemies[0].Position.Neighbors().First(battle.Floor); xander.Moved = true; enemies[0].Hp = 1;
            nance.Bear = true; nance.Position = enemies[1].Position.Neighbors().First(battle.Floor); nance.Moved = true; enemies[1].Hp = 1;

            Test.True(battle.Attack(xander, enemies[0], 1), "Vibro-axe defeat");
            Test.True(!xander.Moved, "Vanguard restores movement");
            Test.True(battle.Attack(nance, enemies[1], 0), "Bear claws defeat");
            Test.True(!nance.Moved, "Primal Fury restores movement");
        });

        Test.Run("guardian_intercepts_once_and_anesthetist_stuns_once", () =>
        {
            var campaign = new CampaignState();
            campaign.Progress("nance").ClassProgressions.UnionWith(["nance-guardian-1", "nance-guardian-2"]);
            campaign.Progress("chrandy").ClassProgressions.UnionWith(["chrandy-anesthetist-1", "chrandy-anesthetist-2"]);
            var battle = new BattleState(3, campaign, ["xander", "nance", "akiko", "chrandy"]);
            var nance = battle.Units.Single(u => u.Id == "nance"); nance.Bear = true;
            var xander = battle.Units.Single(u => u.Id == "xander");
            var enemy = battle.Units.First(u => u.Team == Team.Enemy && u.Role != Role.OmpokProxy);
            xander.Position = new Cell(4, 4); nance.Position = new Cell(4, 5); enemy.Position = new Cell(4, 3);
            var xanderHp = xander.Hp; var nanceHp = nance.Hp;
            battle.StartEnemy(); battle.Enemy(enemy);
            Test.Equal(xanderHp, xander.Hp);
            Test.True(nance.Hp < nanceHp, "Nance intercepts the first adjacent attack");

            battle.StartPlayer();
            var chrandy = battle.Units.Single(u => u.Id == "chrandy");
            var ompok = battle.Units.Single(u => u.Role == Role.OmpokProxy);
            chrandy.Position = ompok.Position.Neighbors().First(battle.Floor);
            Test.True(battle.Attack(chrandy, ompok, 0), "first immune stun is allowed");
            Test.True(ompok.Stunned > 0, "Ompok is stunned");
            chrandy.Acted = false; ompok.Stunned = 0;
            Test.True(!battle.Attack(chrandy, ompok, 0), "immune stun is limited to once per mission");
        });

        Test.Run("first_mission_completion_grants_character_credit", () =>
        {
            var battle = new BattleState(1);
            var xander = battle.Units.Single(u => u.Id == "xander");
            xander.Position = battle.Issac.Position.Neighbors().First(battle.Floor);

            Test.True(battle.Interact(xander), "mission objective completes");

            Test.True(battle.Campaign.HasMissionCredit("xander", "failed-override"), "Xander receives mission credit");
            Test.Equal(1, battle.Campaign.Progress("xander").PendingClassProgressions);
        });

        Test.Run("veteran_replay_progression_is_rolled_back", () =>
        {
            var campaign = new CampaignState();
            campaign.Progress("xander").CompletedMissionIds.Add("failed-override");
            campaign.Progress("xander").Xp = 35;
            var battle = new BattleState(1, campaign);
            var xander = battle.Units.Single(u => u.Id == "xander");
            xander.Position = battle.Issac.Position.Neighbors().First(battle.Floor);

            Test.True(battle.Interact(xander), "replay objective completes");

            Test.Equal(35, campaign.Progress("xander").Xp);
            Test.Equal(0, campaign.Progress("xander").PendingClassProgressions);
        });

        Test.Run("mixed_replay_roster_commits_only_new_character_progress", () =>
        {
            var campaign = new CampaignState();
            campaign.Progress("xander").CompletedMissionIds.Add("mission-3");
            campaign.Progress("xander").Xp = 10;
            var roster = new[] { "xander", "nance", "issac", "chrandy" };
            var battle = new BattleState(3, campaign, roster);
            campaign.AwardXp("xander", 40);
            campaign.AwardXp("chrandy", 30);
            foreach (var enemy in battle.Units.Where(u => u.Team == Team.Enemy)) enemy.Hp = 0;

            battle.Evaluate();

            Test.Equal(10, campaign.Progress("xander").Xp);
            Test.Equal(0, campaign.Progress("xander").PendingClassProgressions);
            Test.Equal(30, campaign.Progress("chrandy").Xp);
            Test.Equal(1, campaign.Progress("chrandy").PendingClassProgressions);
            Test.True(campaign.HasMissionCredit("chrandy", "mission-3"), "new character receives credit");
            Test.Equal(40, battle.MissionXpGains["xander"]);
            Test.Equal(30, battle.MissionXpGains["chrandy"]);
            Test.True(battle.NewlyRewardedIds.Contains("chrandy"), "reward summary identifies new recipient");
            Test.True(!battle.NewlyRewardedIds.Contains("xander"), "veteran is review-only");
        });

        Test.Run("replay_reserve_catchup_only_rewards_eligible_characters", () =>
        {
            var campaign = new CampaignState();
            campaign.Progress("xander").CompletedMissionIds.Add("mission-3");
            campaign.Progress("akiko").CompletedMissionIds.Add("mission-3");
            var battle = new BattleState(3, campaign, new[] { "xander", "nance", "issac", "chrandy" });
            campaign.AwardXp("nance", 30);
            campaign.AwardXp("issac", 30);
            campaign.AwardXp("chrandy", 30);
            foreach (var enemy in battle.Units.Where(u => u.Team == Team.Enemy)) enemy.Hp = 0;

            battle.Evaluate();

            Test.Equal(0, campaign.Progress("akiko").Xp);
        });

        Test.Run("legacy_completed_missions_migrate_to_safe_character_credit", () =>
        {
            var save = new CampaignSaveData
            {
                CompletedMissions = ["failed-override", "mission-2", "mission-3"],
                Levels = new() { ["xander"] = 2 }
            };
            var campaign = save.ToState();

            foreach (var id in new[] { "xander", "nance", "akiko" })
                Test.True(campaign.HasMissionCredit(id, "failed-override"), $"{id} receives safe Mission 1 credit");
            foreach (var id in new[] { "xander", "nance", "akiko", "issac" })
                Test.True(campaign.HasMissionCredit(id, "mission-2"), $"{id} receives safe Mission 2 credit");
            foreach (var id in new[] { "xander", "nance", "akiko", "issac", "chrandy" })
                Test.True(campaign.HasMissionCredit(id, "mission-3"), $"{id} receives safe Mission 3 credit");
            Test.Equal(2, campaign.Progress("xander").Level);
        });

        Test.Run("partially_resolved_rewards_survive_migration", () =>
        {
            var save = new CampaignSaveData
            {
                ProgressionSchemaVersion = 1,
                CompletedMissions = ["failed-override"],
                Progressions = new()
                {
                    ["xander"] = new() { TrainingPath = TrainingPath.Attack, ClassProgressions = ["xander-marksman-1"], CompletedMissionIds = ["failed-override"] },
                    ["nance"] = new() { TrainingPath = TrainingPath.Health, PendingClassProgressions = 1, CompletedMissionIds = ["failed-override"] }
                }
            };

            var campaign = save.ToState();

            Test.True(campaign.Progress("xander").ClassProgressions.Contains("xander-marksman-1"), "confirmed reward remains acquired");
            Test.Equal(0, campaign.Progress("xander").PendingClassProgressions);
            Test.Equal(1, campaign.Progress("nance").PendingClassProgressions);
        });

        Test.Run("campaign_progression_round_trips_through_json", () =>
        {
            var campaign = new CampaignState();
            campaign.CompletedMissions.Add("failed-override");
            var progress = campaign.Progress("akiko");
            progress.Level = 3; progress.Xp = 45; progress.TrainingPath = TrainingPath.Armor;
            progress.ArmorBonus = 1; progress.PendingClassProgressions = 1;
            progress.ClassProgressions.Add("akiko-engineer-1"); progress.CompletedMissionIds.Add("failed-override");

            var json = JsonSerializer.Serialize(CampaignSaveData.From(campaign));
            var restored = JsonSerializer.Deserialize<CampaignSaveData>(json)!.ToState();

            Test.Equal(3, restored.Progress("akiko").Level);
            Test.Equal(45, restored.Progress("akiko").Xp);
            Test.Equal(TrainingPath.Armor, restored.Progress("akiko").TrainingPath);
            Test.True(restored.Progress("akiko").ClassProgressions.Contains("akiko-engineer-1"), "class progression round trips");
            Test.True(restored.HasMissionCredit("akiko", "failed-override"), "mission credit round trips");
        });

        Test.Run("xp_award_result_uses_amount_and_plain_language_reason", () =>
        {
            var result = new XpAwardResult("chrandy", 20, 1, 10, 1, 30, "Healed a teammate", []);
            var presentation = new ProgressionPresentationState();

            presentation.Begin(result);

            Test.Equal("+20 XP", presentation.GainLabel);
            Test.Equal("Healed a teammate", presentation.Reason);
        });

        Test.Run("xp_bar_interpolates_before_finishing", () =>
        {
            var presentation = new ProgressionPresentationState();
            presentation.Begin(new("xander", 25, 1, 20, 1, 45, "Damaging attack", []));

            presentation.Advance(.325);

            Test.True(presentation.DisplayedXp > 20 && presentation.DisplayedXp < 45, "XP bar is between old and new values");
            Test.True(presentation.Active, "animation remains active halfway through");
        });

        Test.Run("xp_bar_wraps_with_carryover_and_queues_level_up", () =>
        {
            var presentation = new ProgressionPresentationState();
            presentation.Begin(new("xander", 25, 1, 90, 2, 15, "Damaging attack", [new(2, TrainingPath.Attack)]));

            presentation.Advance(.65);

            Test.Equal(100, presentation.DisplayedXp);
            Test.True(presentation.ShowingLevelUp, "level-up notice waits for click");
            Test.Equal("LEVEL 2", presentation.LevelUpTitle);
            Test.Equal("Attack +1", presentation.LevelUpDetail);

            presentation.DismissLevelUp();
            Test.Equal(0, presentation.DisplayedXp);
            presentation.Advance(.65);
            Test.Equal(15, presentation.DisplayedXp);
        });

        Test.Run("multiple_level_up_notices_dismiss_in_order", () =>
        {
            var presentation = new ProgressionPresentationState();
            presentation.Begin(new("nance", 210, 1, 95, 4, 5, "Mission objective", [new(2, TrainingPath.Health), new(3, TrainingPath.Armor), new(4, TrainingPath.Attack)]));
            presentation.Advance(10);

            Test.Equal("Health +4", presentation.LevelUpDetail);
            presentation.DismissLevelUp();
            presentation.Advance(.65);
            Test.Equal("Armor +1", presentation.LevelUpDetail);
            presentation.DismissLevelUp();
            presentation.Advance(.65);
            Test.Equal("Attack +1", presentation.LevelUpDetail);
            presentation.DismissLevelUp();
            presentation.Advance(.65);
            Test.True(!presentation.Active, "presentation ends after final notice");
        });

        Test.Run("large_frame_delta_completes_xp_animation_once", () =>
        {
            var presentation = new ProgressionPresentationState();
            presentation.Begin(new("issac", 50, 2, 25, 2, 75, "Enemy defeated", []));

            presentation.Advance(20);
            presentation.Advance(20);

            Test.Equal(75, presentation.DisplayedXp);
            Test.True(!presentation.Active, "large delta leaves presentation complete");
        });

        Test.Run("crew_preparation_requires_training_path_before_confirmation", () =>
        {
            var campaign = new CampaignState();
            var state = new CrewPreparationState(campaign, 1, new[] { "xander", "nance", "akiko" });

            Test.True(!state.CanConfirmCharacter("xander"), "unassigned path blocks confirmation");
            state.SetTrainingPath("xander", TrainingPath.Attack);
            Test.True(state.CanConfirmCharacter("xander"), "selected path permits confirmation");
        });

        Test.Run("training_path_explanation_and_selection_labels_are_explicit", () =>
        {
            var explanation = CrewPreparationPresentation.TrainingPathExplanation;
            Test.True(explanation.Contains("100 XP"), "explains the Level Up threshold");
            Test.True(explanation.Contains("Every Level Up") && explanation.Contains("selected Training Path"), "explains that the path applies immediately");
            Test.True(explanation.Contains("change this path between missions"), "explains path changes");
            Test.True(explanation.Contains("Earlier Level Ups do not change"), "explains that choices are not retroactive");
            Test.True(explanation.Contains("without a Training Path") && explanation.Contains("random bonus"), "explains reserve catch-up behavior");
            Test.Equal("Choose one Training Path for each deployed character before beginning the mission.", CrewPreparationPresentation.TrainingPathInstruction);
            Test.Equal("✓ SELECTED\nHealth\n+4 Health", CrewPreparationPresentation.PathLabel(TrainingPath.Health, true));
            Test.Equal("✓ SELECTED\nArmor\n+1 Armor", CrewPreparationPresentation.PathLabel(TrainingPath.Armor, true));
            Test.Equal("✓ SELECTED\nMovement\n+1 Movement", CrewPreparationPresentation.PathLabel(TrainingPath.Movement, true));
            Test.Equal("✓ SELECTED\nAttack\n+1 damage", CrewPreparationPresentation.PathLabel(TrainingPath.Attack, true));
            Test.Equal("Health\n+4 Health", CrewPreparationPresentation.PathLabel(TrainingPath.Health, false));
        });

        Test.Run("class_progression_explanation_and_selection_labels_are_explicit", () =>
        {
            var explanation = CrewPreparationPresentation.ClassProgressionExplanation;
            Test.True(explanation.Contains("permanent reward"), "explains permanence");
            Test.True(explanation.Contains("first time"), "explains new-mission eligibility");
            Test.True(explanation.Contains("separate from automatic Level Ups"), "distinguishes Class Progressions from Level Ups");
            Test.True(explanation.Contains("Tier II") && explanation.Contains("matching Tier I"), "explains tier prerequisites");
            Test.Equal("Choose one Class Progression, then confirm this character's choices.", CrewPreparationPresentation.ClassProgressionInstruction);

            var progression = new ClassProgressionDefinition("xander-marksman-1", "Marksman I", 1, null, "Plasma pistol range +1.");
            Test.Equal("✓ SELECTED\nMarksman I\nPlasma pistol range +1.", CrewPreparationPresentation.ClassProgressionLabel(progression, true));
            Test.Equal("Marksman I\nPlasma pistol range +1.", CrewPreparationPresentation.ClassProgressionLabel(progression, false));
        });

        Test.Run("crew_preparation_advances_to_next_unconfirmed_selection", () =>
        {
            var state = new CrewPreparationState(new CampaignState(), 1, new[] { "xander", "nance", "akiko" });
            Test.Equal("nance", state.NextUnconfirmedSelectedId("xander", new HashSet<string> { "xander" }));
            Test.Equal("akiko", state.NextUnconfirmedSelectedId("nance", new HashSet<string> { "xander", "nance" }));
            Test.Equal("akiko", state.NextUnconfirmedSelectedId("akiko", new HashSet<string> { "xander", "nance", "akiko" }));

            var customOrder = new CrewPreparationState(new CampaignState(), 3, new[] { "chrandy", "akiko", "xander", "nance" });
            Test.Equal("akiko", customOrder.NextUnconfirmedSelectedId("chrandy", new HashSet<string> { "chrandy" }));

            Test.True(customOrder.ToggleDeployment("xander", false), "open character can be removed from deployment");
            Test.Equal("chrandy", customOrder.NextUnconfirmedSelectedId("xander", new HashSet<string> { "xander" }));
        });

        Test.Run("crew_preparation_enforces_four_unique_available_characters", () =>
        {
            var state = new CrewPreparationState(new CampaignState(), 3, new[] { "xander", "nance", "akiko", "issac" });

            Test.True(!state.ToggleDeployment("chrandy", true), "fifth deployment rejected");
            Test.True(state.ToggleDeployment("akiko", false), "crew member can be removed");
            Test.True(state.ToggleDeployment("chrandy", true), "replacement can be added");
            Test.Equal(4, state.SelectedIds.Count);
        });

        Test.Run("crew_preparation_requires_unresolved_progression_for_deployed_character", () =>
        {
            var campaign = new CampaignState();
            campaign.SetTrainingPath("xander", TrainingPath.Attack);
            campaign.Progress("xander").PendingClassProgressions = 1;
            var state = new CrewPreparationState(campaign, 1, new[] { "xander", "nance", "akiko" });

            Test.True(!state.CanConfirmCharacter("xander"), "pending choice blocks confirmation");
            Test.True(state.SetClassProgression("xander", "xander-marksman-1"), "eligible choice can be drafted");
            Test.True(state.CanConfirmCharacter("xander"), "drafted choice permits confirmation");
        });

        Test.Run("crew_preparation_draft_does_not_mutate_campaign_until_confirmed", () =>
        {
            var campaign = new CampaignState();
            campaign.Progress("xander").PendingClassProgressions = 1;
            var state = new CrewPreparationState(campaign, 1, new[] { "xander", "nance", "akiko" });
            state.SetTrainingPath("xander", TrainingPath.Movement);
            state.SetClassProgression("xander", "xander-vanguard-1");

            Test.Equal(TrainingPath.Unassigned, campaign.Progress("xander").TrainingPath);
            Test.True(!campaign.Progress("xander").ClassProgressions.Contains("xander-vanguard-1"), "ability remains draft");
            Test.True(state.ConfirmCharacter("xander"), "card confirms");
            Test.Equal(TrainingPath.Movement, campaign.Progress("xander").TrainingPath);
            Test.True(campaign.Progress("xander").ClassProgressions.Contains("xander-vanguard-1"), "ability becomes permanent");
        });

        Test.Run("crew_preparation_veteran_card_is_review_only_without_pending_reward", () =>
        {
            var campaign = new CampaignState();
            campaign.SetTrainingPath("xander", TrainingPath.Health);
            campaign.Progress("xander").ClassProgressions.Add("xander-marksman-1");
            campaign.Progress("xander").CompletedMissionIds.Add("failed-override");
            var state = new CrewPreparationState(campaign, 1, new[] { "xander", "nance", "akiko" });

            Test.True(state.IsVeteran("xander", "failed-override"), "mission credit marks veteran");
            Test.Equal(0, state.AvailableClassProgressions("xander").Count);
            Test.True(state.AcquiredClassProgressions("xander").Any(x => x.Id == "xander-marksman-1"), "acquired ability remains visible");
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
