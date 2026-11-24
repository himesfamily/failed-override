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
        Test.Run("crew_catalog_has_all_playable_profiles_and_canonical_names", () =>
        {
            var missionThree = CrewCatalog.AvailableIds(3);
            Test.Equal(6, missionThree.Count);
            Test.Equal(6, missionThree.Distinct(StringComparer.Ordinal).Count());
            Test.True(new[] { "xander", "nance", "akiko", "issac", "chrandy", "jeane" }.All(missionThree.Contains), "Mission 3 exposes the complete named crew");

            var captain = CrewCatalog.Profile("jeane");
            Test.Equal("Captain Jeane Gershon", captain.Name);
            Test.Equal("Captain", captain.ClassName);
            Test.Equal(Role.Captain, captain.Role);
            Test.Equal(24, captain.Health);
            Test.Equal(2, captain.Armor);
            Test.Equal(6, captain.Movement);
            Test.Equal("Dr. Chandry Perra", CrewCatalog.Profile("chrandy").Name);
        });

        Test.Run("named_crew_excludes_controlled_machines", () =>
        {
            foreach (var id in CrewCatalog.AvailableIds(3))
            {
                var profile = CrewCatalog.Profile(id);
                var unit = new Unit { Id = id, Name = profile.Name, Token = profile.Token, Role = profile.Role, Team = Team.Player };
                Test.True(unit.NamedCrew, $"{id} is a named crew member");
            }

            var robot = new Unit { Id = "robot-1", Name = "Support robot", Token = "R", Role = Role.Robot, Team = Team.Player, ControllerId = "akiko" };
            var droid = new Unit { Id = "droid-1", Name = "Reprogrammed droid", Token = "D", Role = Role.Droid, Team = Team.Player, ControllerId = "issac" };
            Test.True(!robot.NamedCrew, "summoned robots are not named crew");
            Test.True(!droid.NamedCrew, "reprogrammed robots are not named crew");
        });

        Test.Run("mission_two_wrap_up_hands_off_to_ompok", () =>
        {
            var beats = MissionTwoWrapUpScript.Beats;
            Test.Equal(5, beats.Count);
            Test.Equal("CAPTAIN JEANE GERSHON", beats[0].Speaker);
            Test.True(beats.Any(b => b.Dialogue.Contains("infected our ship's AI")), "Issac explains the intrusion");
            Test.Equal("EMPEROR OMPOK", beats[^1].Speaker);
            Test.True(beats[^1].Dialogue.Contains("Issac"), "canonical Issac spelling");
            Test.True(!string.Join(" ", beats.Select(b => b.Speaker + b.Dialogue)).Contains("GERSON"), "legacy Gerson spelling is absent");
        });

        Test.Run("mission_three_intro_ends_in_cargo_bay", () =>
        {
            var beats = MissionThreeIntroScript.Beats;
            Test.True(beats.Count >= 7, "long Ompok speech is split into readable beats");
            Test.Equal("EMPEROR OMPOK", beats[0].Speaker);
            Test.True(beats.Any(b => b.Dialogue.Contains("KILL ALL HUMANS")), "Ompok declares the attack");
            Test.True(beats.Any(b => b.Speaker == "CAPTAIN JEANE GERSHON"), "canonical captain spelling");
            Test.Equal("STARSHIP AURORA\nCARGO BAY", MissionThreeIntroScript.TransitionTitle);
        });

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
            Test.True(!allText.Contains("Chrandy", StringComparison.OrdinalIgnoreCase), "legacy Chandry spelling is absent from player-facing text");
            Test.Equal("INTRUSION ALERT", MissionTwoIntroScript.TransitionKicker);
            Test.Equal("STARSHIP AURORA\nCARGO BAY", MissionTwoIntroScript.TransitionTitle);
            Test.True(MissionTwoIntroScript.PreserveFullComposition, "the wide room keeps Issac and Chrandy visible at opposite edges");
            Test.True(MissionTwoIntroScript.ShowForegroundPortrait, "the active speaker is drawn forward over the room cast");
            Test.Equal(BriefingSpeakerFocusLayout.BackgroundFrame.X, MissionTwoIntroScript.BackgroundFrame.X);
            Test.Equal(BriefingSpeakerFocusLayout.BackgroundFrame.Y, MissionTwoIntroScript.BackgroundFrame.Y);
            Test.Equal(BriefingSpeakerFocusLayout.BackgroundFrame.Width, MissionTwoIntroScript.BackgroundFrame.Width);
            Test.Equal(BriefingSpeakerFocusLayout.BackgroundFrame.Height, MissionTwoIntroScript.BackgroundFrame.Height);
            Test.True(MissionTwoIntroScript.BackgroundFrame.Height < 900f, "the full wide background is letterboxed instead of cropped");
            Test.True(!MissionTwoIntroScript.ForegroundPortrait.Intersects(MissionTwoIntroScript.IssacFaceRegion), "speaker portrait leaves room Issac visible");
            Test.True(!MissionTwoIntroScript.ForegroundPortrait.Intersects(MissionTwoIntroScript.ChrandyFaceRegion), "speaker portrait leaves room Chrandy visible");
        });

        Test.Run("mission_one_attack_beat_uses_the_supplied_issac_scream", () =>
        {
            Test.Equal("res://Assets/Audio/SFX/issac_scream_wilhelm.mp3", OpeningCinematicAudio.IssacScreamResource);
            Test.True(OpeningCinematicAudio.ShouldPlayIssacScream("SYSTEM", "Issac screams—and the cargo bay falls silent."), "the attack narration triggers Issac's scream");
            Test.True(!OpeningCinematicAudio.ShouldPlayIssacScream("ISSAC WYNTER", "We have to carry it."), "ordinary Issac dialogue does not replay the scream");
        });

        Test.Run("briefing_speaker_focus_maps_every_scripted_group_speaker", () =>
        {
            var portraitKeys = MissionTwoIntroScript.Beats
                .Concat(MissionTwoWrapUpScript.Beats)
                .Concat(MissionThreeIntroScript.Beats)
                .Select(beat => beat.Portrait)
                .Distinct(StringComparer.Ordinal)
                .ToList();
            var unmapped = portraitKeys
                .Where(key => !BriefingSpeakerFocusLayout.IsHologram(key) && BriefingSpeakerFocusLayout.MarkerFor(key) is null)
                .ToList();

            Test.Equal(0, unmapped.Count);
            Test.True(BriefingSpeakerFocusLayout.IsHologram("ompok"), "Ompok uses hologram focus");
            Test.Equal<MissionTwoIntroRect?>(null, BriefingSpeakerFocusLayout.MarkerFor("ompok"));
            Test.Equal<MissionTwoIntroRect?>(null, BriefingSpeakerFocusLayout.MarkerFor("unknown"));
        });

        Test.Run("briefing_speaker_focus_preserves_safe_wide_frame", () =>
        {
            var frame = BriefingSpeakerFocusLayout.BackgroundFrame;
            Test.Equal(24f, frame.X);
            Test.Equal(82f, frame.Y);
            Test.Equal(1352f, frame.Width);
            Test.Equal(761f, frame.Height);
            Test.True(frame.X >= 24f && frame.X + frame.Width <= 1376f, "room art keeps horizontal safe margins");
            Test.True(Math.Abs(frame.Width / frame.Height - 1672f / 941f) < .01f, "room art preserves the approved wide composition");
        });

        Test.Run("briefing_speaker_focus_named_regions_match_the_room_cast", () =>
        {
            var issac = BriefingSpeakerFocusLayout.MarkerFor("issac");
            var jeane = BriefingSpeakerFocusLayout.MarkerFor("jeane");
            var xander = BriefingSpeakerFocusLayout.MarkerFor("xander");
            var nance = BriefingSpeakerFocusLayout.MarkerFor("nance");

            Test.True(issac is not null && jeane is not null && xander is not null && nance is not null, "every outlined crew member has a named room region");
            Test.True(issac!.Value.X < jeane!.Value.X, "Issac is the leftmost outlined speaker");
            Test.True(jeane.Value.X < xander!.Value.X, "Captain Gershon is the gray-haired captain second from the left");
            Test.True(xander.Value.X < nance!.Value.X, "Nance is the red-jacketed woman to Xander's right");
            Test.True(!issac.Value.Intersects(jeane.Value), "Issac and Jeane markers do not overlap");
            Test.True(!jeane.Value.Intersects(xander.Value), "Jeane and Xander markers do not overlap");
            Test.True(!xander.Value.Intersects(nance.Value), "Xander and Nance markers do not overlap");
            Test.Equal("res://Assets/Cinematic/SpeakerFocus/ompok-outline.svg", BriefingSpeakerFocusLayout.HologramOutlineResource);
            Test.True(BriefingSpeakerFocusLayout.FocusZIndex > 0, "speaker markers draw above the briefing artwork and Ompok hologram");

            var displayed = BriefingSpeakerFocusLayout.DisplayMarkerFor("xander");
            var frame = BriefingSpeakerFocusLayout.BackgroundFrame;
            Test.True(displayed is not null, "Xander marker converts to display coordinates");
            Test.True(displayed!.Value.X >= frame.X && displayed.Value.X + displayed.Value.Width <= frame.X + frame.Width, "display marker stays inside the letterboxed artwork");
            Test.True(Math.Abs(displayed.Value.Width / xander.Value.Width - frame.Width / 1672f) < .001f, "marker uses the same scale as the artwork");

            var hairlines = new Dictionary<string, float>
            {
                ["issac"] = 244f,
                ["jeane"] = 161f,
                ["xander"] = 147f,
                ["nance"] = 185f,
            };
            foreach (var (speakerKey, hairlineY) in hairlines)
            {
                var marker = BriefingSpeakerFocusLayout.MarkerFor(speakerKey)!.Value;
                var haloTop = marker.Y + marker.Height * BriefingSpeakerFocusLayout.MarkerHaloTopFraction;
                Test.True(haloTop <= hairlineY, $"{speakerKey} halo starts above the hairline");
            }
        });

        Test.Run("briefing_speaker_focus_switches_without_stale_or_duplicate_layers", () =>
        {
            var state = new BriefingSpeakerFocusTransitionState();

            var first = state.Select("jeane");
            Test.Equal<string?>(null, first.PreviousGroupKey);
            Test.Equal("jeane", first.CurrentGroupKey);
            Test.True(!first.HologramFocused, "crew focus is not a hologram");
            Test.True(state.LayerVisible(first.CurrentLayer!.Value), "the selected crew layer is visible");

            var repeated = state.Select("jeane");
            Test.Equal<string?>(null, repeated.PreviousGroupKey);
            Test.Equal("jeane", repeated.CurrentGroupKey);
            Test.True(repeated.ReusesCurrentLayer, "an uninterrupted repeated speaker may reuse the current texture");

            var switched = state.Select("issac");
            Test.Equal("jeane", switched.PreviousGroupKey);
            Test.Equal("issac", switched.CurrentGroupKey);
            Test.True(state.LayerVisible(switched.PreviousLayer!.Value), "the previous layer remains visible only for the crossfade");
            Test.True(state.LayerVisible(switched.CurrentLayer!.Value), "the incoming layer is visible for the crossfade");

            var hologram = state.Select("ompok");
            Test.Equal("issac", hologram.PreviousGroupKey);
            Test.Equal<string?>(null, hologram.CurrentGroupKey);
            Test.True(hologram.HologramFocused, "Ompok uses hologram focus");
            Test.True(hologram.ClearGroupImmediately, "Ompok clears crew focus synchronously");
            Test.True(!state.LayerVisible(0) && !state.LayerVisible(1), "rapid Ompok interruption cannot leave a stale crew outline");

            var unknown = state.Select("unknown");
            Test.Equal<string?>(null, unknown.PreviousGroupKey);
            Test.Equal<string?>(null, unknown.CurrentGroupKey);
            Test.True(!unknown.HologramFocused, "unknown speakers clear hologram focus");
            Test.True(unknown.ClearGroupImmediately, "unknown speakers synchronously clear both group layers");

            var rapidFirst = state.Select("jeane");
            var rapidSecond = state.Select("issac");
            var repeatedDuringFade = state.Select("issac");
            Test.True(state.LayerVisible(repeatedDuringFade.CurrentLayer!.Value), "repeated rapid input keeps the active layer visible");
            Test.True(!state.LayerVisible(1 - repeatedDuringFade.CurrentLayer.Value), "repeated rapid input settles the inactive layer immediately");
            state.Complete(rapidSecond);
            Test.True(state.LayerVisible(repeatedDuringFade.CurrentLayer.Value), "a stale tween completion cannot hide the current speaker");

            state.Select("jeane");
            state.Clear();
            Test.Equal<string?>(null, state.CurrentGroupKey);
            Test.True(!state.HologramFocused, "clear removes every focus target");
            var replayed = state.Select("issac");
            Test.True(!replayed.ReusesCurrentLayer, "replaying a cinematic reloads Issac's texture instead of reusing a stale Nance layer");
        });

        Test.Run("briefing_portrait_catalog_covers_every_scripted_human_speaker", () =>
        {
            var keys = MissionTwoIntroScript.Beats.Concat(MissionTwoWrapUpScript.Beats).Concat(MissionThreeIntroScript.Beats)
                .Select(beat => beat.Portrait).Where(key => key != "ompok").Distinct().ToList();
            foreach (var key in keys)
            {
                var portrait = BriefingSpeakerPortraitLayout.For(key);
                Test.True(portrait is not null, $"{key} has a portrait mapping");
                Test.True(portrait!.Resource.StartsWith("res://Assets/Portraits/"), $"{key} uses a portrait resource");
                Test.True(File.Exists(portrait.Resource[6..]), $"{key} portrait exists on disk");
                Test.True(portrait.Side is SpeakerPortraitSide.Left or SpeakerPortraitSide.Right, $"{key} has a side");
            }
            Test.True(keys.All(BriefingSpeakerPortraitLayout.AllHumanKeys.Contains), "catalog exposes every scripted human key");
        });

        Test.Run("briefing_portrait_frames_stay_inside_scene_above_dialogue", () =>
        {
            var background = BriefingSpeakerFocusLayout.BackgroundFrame;
            foreach (var key in BriefingSpeakerPortraitLayout.AllHumanKeys)
            {
                var frame = BriefingSpeakerPortraitLayout.DisplayFrameFor(key);
                Test.True(frame is not null, $"{key} has a display frame");
                Test.True(frame!.Value.X >= background.X && frame.Value.Y >= background.Y, $"{key} starts inside scene");
                Test.True(frame.Value.X + frame.Value.Width <= background.X + background.Width, $"{key} ends inside scene width");
                Test.True(frame.Value.Y + frame.Value.Height <= 730, $"{key} remains above dialogue");
            }
        });

        Test.Run("briefing_portrait_transitions_reuse_and_crossfade_cleanly", () =>
        {
            var state = new BriefingSpeakerPortraitTransitionState();
            var first = state.Select("issac");
            Test.Equal("issac", first.CurrentKey!);
            Test.True(first.CurrentLayer is not null && !first.ReusesCurrentLayer, "first portrait selects a layer");
            state.Complete(first);
            var repeated = state.Select("issac");
            Test.True(repeated.ReusesCurrentLayer, "repeated speaker reuses portrait without flashing");
            var changed = state.Select("jeane");
            Test.Equal("issac", changed.PreviousKey!);
            Test.Equal("jeane", changed.CurrentKey!);
            Test.True(changed.PreviousLayer != changed.CurrentLayer, "speaker change crossfades between layers");
            state.Complete(changed);
            Test.Equal(1, Enumerable.Range(0, 2).Count(state.LayerVisible));
        });

        Test.Run("briefing_portrait_transitions_settle_rapid_changes", () =>
        {
            var state = new BriefingSpeakerPortraitTransitionState();
            state.Select("issac");
            state.Select("jeane");
            var rapid = state.Select("xander");
            state.Complete(rapid);
            Test.Equal("xander", state.CurrentKey!);
            Test.Equal(1, Enumerable.Range(0, 2).Count(state.LayerVisible));
        });

        Test.Run("briefing_portrait_transitions_clear_for_hologram_and_restore_human", () =>
        {
            var state = new BriefingSpeakerPortraitTransitionState();
            state.Select("nance");
            var hologram = state.Select("ompok");
            Test.True(hologram.HologramFocused && !state.LayerVisible(0) && !state.LayerVisible(1), "hologram clears human layers");
            var human = state.Select("akiko");
            Test.True(!human.HologramFocused && human.CurrentKey == "akiko", "human portrait returns after hologram");
            state.Clear();
            Test.True(state.CurrentKey is null && !state.HologramFocused && !state.LayerVisible(0) && !state.LayerVisible(1), "clear removes all focus");
        });

        Test.Run("later_briefing_scripts_use_supported_portrait_or_hologram_keys", () =>
        {
            var beats = MissionTwoIntroScript.Beats.Concat(MissionTwoWrapUpScript.Beats).Concat(MissionThreeIntroScript.Beats).ToList();
            foreach (var beat in beats)
                Test.True(BriefingSpeakerFocusLayout.IsHologram(beat.Portrait) || BriefingSpeakerPortraitLayout.For(beat.Portrait) is not null,
                    $"{beat.Speaker} uses a supported portrait key: {beat.Portrait}");
            Test.True(beats.GroupBy(beat => beat.Portrait).Any(group => group.Count() > 1), "integration covers repeated speakers");
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

        Test.Run("mission_three_accepts_any_four_available_named_crew", () =>
        {
            var rosters = new[]
            {
                new[] { "xander", "nance", "chrandy", "jeane" },
                new[] { "nance", "akiko", "issac", "jeane" },
                new[] { "xander", "akiko", "chrandy", "jeane" },
            };
            var definition = MissionDefinition.For(3);
            Test.Equal(4, definition.PlayerSpawns.Count);
            Test.Equal(4, definition.PlayerSpawns.Distinct().Count());
            Test.True(definition.PlayerSpawns.All(cell => definition.At(cell).Walkable), "all player spawn cells are walkable");

            foreach (var roster in rosters)
            {
                var battle = new BattleState(3, new CampaignState(), roster);
                var named = battle.Units.Where(unit => unit.Team == Team.Player && unit.NamedCrew).ToList();
                Test.Equal(4, named.Count);
                Test.Equal(string.Join(',', roster), string.Join(',', named.Select(unit => unit.Id)));
                Test.Equal(string.Join(',', definition.PlayerSpawns), string.Join(',', named.Select(unit => unit.Position)));
                Test.True(named.Select(unit => unit.Position).Distinct().Count() == 4, "selected crew occupy distinct cells");
                battle.Evaluate();
                Test.True(!battle.Finished, "a roster without a particular specialist starts safely");
            }

            var campaign = new CampaignState();
            Test.Equal(6, campaign.AvailableCrew(3).Count);
            Test.True(campaign.ValidateRoster(3, ["xander", "nance", "chrandy", "jeane"]), "four available crew are valid");
            Test.True(!campaign.ValidateRoster(3, ["xander", "nance", "chrandy"]), "three crew are invalid");
            Test.True(!campaign.ValidateRoster(3, ["xander", "xander", "chrandy", "jeane"]), "duplicate crew are invalid");
            Test.True(!campaign.ValidateRoster(3, ["xander", "nance", "chrandy", "ompok"]), "unavailable IDs are invalid");
            Test.True(campaign.ValidateRoster(1, ["xander", "nance", "akiko"]), "Mission 1 remains fixed");
            Test.True(campaign.ValidateRoster(2, ["xander", "nance", "akiko", "issac"]), "Mission 2 remains fixed");
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

        Test.Run("every_enemy_role_is_a_reprogrammable_and_repairable_machine", () =>
        {
            var battle = new BattleState(3);
            var issac = battle.Units.Single(u => u.Id == "issac");
            var akiko = battle.Units.Single(u => u.Id == "akiko");
            var enemies = battle.Units.Where(u => u.Team == Team.Enemy).ToList();
            var expectedRoles = new[] { Role.OmpokProxy, Role.VaurBrute, Role.KheltStalker, Role.SynapticEngineer };

            foreach (var role in expectedRoles)
                Test.True(enemies.Any(u => u.Role == role), $"mission 3 includes {role}");

            foreach (var enemy in enemies)
            {
                Test.True(enemy.Machine, $"{enemy.Role} is classified as a machine");
                issac.Acted = false;
                issac.Position = new Cell(2, 2);
                enemy.Position = new Cell(2, 3);
                enemy.Hp = Math.Max(1, enemy.MaxHp / 2);
                Test.True(battle.Reprogram(issac, enemy), $"Issac can reprogram damaged {enemy.Role}");
            }

            Test.Equal(Phase.Victory, battle.Phase);

            var repairBattle = new BattleState(3);
            var repairIssac = repairBattle.Units.Single(u => u.Id == "issac");
            var repairAkiko = repairBattle.Units.Single(u => u.Id == "akiko");
            var converted = repairBattle.Units.First(u => u.Role == Role.VaurBrute);
            repairIssac.Position = new Cell(5, 4);
            converted.Position = new Cell(5, 5);
            converted.Hp = converted.MaxHp / 2;
            Test.True(repairBattle.Reprogram(repairIssac, converted), "specialized robot converts before repair");
            converted.Hp = Math.Max(1, converted.MaxHp - 6);
            converted.Position = new Cell(5, 5);
            repairAkiko.Position = new Cell(5, 6);
            repairAkiko.Acted = false;
            Test.True(repairBattle.Repair(repairAkiko, converted), "Akiko can repair a converted specialized machine");

            var defeatedBattle = new BattleState(3);
            var defeatedIssac = defeatedBattle.Units.Single(u => u.Id == "issac");
            var defeatedRobot = defeatedBattle.Units.First(u => u.Role == Role.OmpokProxy);
            defeatedIssac.Position = new Cell(4, 4);
            defeatedRobot.Position = new Cell(4, 5);
            defeatedRobot.Hp = 0;
            Test.True(!defeatedBattle.CanReprogram(defeatedIssac, defeatedRobot), "defeated robots are not valid reprogram targets");
            Test.True(!defeatedBattle.Reprogram(defeatedIssac, defeatedRobot), "reprogram refuses defeated robots");
            Test.True(!defeatedIssac.Acted, "an invalid defeated target does not consume Issac's action");
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

        Test.Run("training_simulation_policy_tracks_canonical_unlocks", () =>
        {
            var campaign = new CampaignState();
            Test.Equal("", string.Join(',', TrainingSimulationPolicy.CompletedMissionNumbers(campaign)));
            Test.Equal("", string.Join(',', TrainingSimulationPolicy.UnlockedCrewIds(campaign)));

            campaign.CompletedMissions.Add("failed-override");
            Test.Equal("1", string.Join(',', TrainingSimulationPolicy.CompletedMissionNumbers(campaign)));
            Test.Equal("xander,nance,akiko,issac", string.Join(',', TrainingSimulationPolicy.UnlockedCrewIds(campaign)));

            campaign.CompletedMissions.Add("mission-2");
            Test.Equal("1,2", string.Join(',', TrainingSimulationPolicy.CompletedMissionNumbers(campaign)));
            Test.Equal("xander,nance,akiko,issac,chrandy,jeane", string.Join(',', TrainingSimulationPolicy.UnlockedCrewIds(campaign)));
        });

        Test.Run("training_simulation_policy_rejects_locked_missions_and_invalid_rosters", () =>
        {
            var campaign = new CampaignState();
            campaign.CompletedMissions.Add("failed-override");
            Test.True(TrainingSimulationPolicy.ValidateMission(campaign, 1), "completed mission is selectable");
            Test.True(!TrainingSimulationPolicy.ValidateMission(campaign, 2), "incomplete mission is locked");
            Test.True(TrainingSimulationPolicy.ValidateRoster(campaign, 1, ["xander", "nance", "akiko", "issac"]), "four unlocked crew are valid");
            Test.True(!TrainingSimulationPolicy.ValidateRoster(campaign, 1, ["xander", "nance", "akiko"]), "simulation requires four crew");
            Test.True(!TrainingSimulationPolicy.ValidateRoster(campaign, 1, ["xander", "xander", "akiko", "issac"]), "duplicate crew are rejected");
            Test.True(!TrainingSimulationPolicy.ValidateRoster(campaign, 1, ["xander", "nance", "akiko", "chrandy"]), "locked crew are rejected");
        });

        Test.Run("training_simulation_launch_always_selects_a_roster", () =>
        {
            var campaignFlow = new MissionLaunchFlowState(1, MissionRunMode.Campaign, ["xander", "nance", "akiko"]);
            Test.Equal(MissionLaunchStage.CrewPreparation, campaignFlow.Stage);

            var simulationFlow = new MissionLaunchFlowState(1, MissionRunMode.TrainingSimulation, ["xander", "nance", "akiko", "issac"]);
            Test.Equal(MissionRunMode.TrainingSimulation, simulationFlow.Mode);
            Test.Equal(MissionLaunchStage.RosterSelection, simulationFlow.Stage);
            Test.True(!simulationFlow.ConfirmRoster(["xander", "nance", "akiko"]), "simulation cannot continue with three crew");
            Test.True(simulationFlow.ConfirmRoster(["xander", "nance", "akiko", "issac"]), "simulation accepts four crew");
        });

        Test.Run("training_mission_selector_lists_only_completed_missions_in_order", () =>
        {
            var campaign = new CampaignState();
            var empty = new TrainingMissionSelectionState(campaign);
            Test.Equal(0, empty.Missions.Count);
            Test.True(!empty.CanSelect(1), "empty campaign cannot select training");

            campaign.CompletedMissions.UnionWith(["mission-2", "failed-override"]);
            var state = new TrainingMissionSelectionState(campaign);
            Test.Equal("1,2", string.Join(',', state.Missions));
            Test.True(state.CanSelect(1), "completed Mission 1 is selectable");
            Test.True(state.Select(2), "completed Mission 2 can be selected");
            Test.Equal(2, state.SelectedMission);
            Test.True(!state.Select(3), "locked Mission 3 is rejected");
            Test.True(!state.Select(99), "unknown missions are rejected");
        });

        Test.Run("training_mission_selector_copy_explains_permanent_participant_rewards", () =>
        {
            Test.Equal("TRAINING SIMULATION", TrainingMissionSelectionPresentation.Title);
            Test.True(TrainingMissionSelectionPresentation.Explanation.Contains("full XP"), "copy promises full XP");
            Test.True(TrainingMissionSelectionPresentation.Explanation.Contains("Class Progressions"), "copy explains class rewards");
            Test.True(TrainingMissionSelectionPresentation.Explanation.Contains("deployed crew"), "copy limits rewards to participants");
            Test.Equal("No completed missions are available for simulation yet.", TrainingMissionSelectionPresentation.EmptyMessage);
            Test.Equal("Mission 1 · Failed Override", TrainingMissionSelectionPresentation.MissionTitle(1));
            Test.Equal("Rescue the simulated Issac from corrupted security droids.", TrainingMissionSelectionPresentation.Objective(1));
            Test.Equal("Reach the security-center passage and keep the crew alive.", TrainingMissionSelectionPresentation.Objective(2));
            Test.Equal("Defeat Ompok's proxy force before it takes the bridge.", TrainingMissionSelectionPresentation.Objective(3));
        });

        Test.Run("post_mission_navigation_separates_campaign_and_training_actions", () =>
        {
            var afterOne = new CampaignState();
            afterOne.CompletedMissions.Add("failed-override");
            var campaign = new PostMissionNavigationState(afterOne, 1, MissionRunMode.Campaign);
            Test.True(campaign.CanAdvanceCampaign, "Mission 1 campaign victory advances");
            Test.Equal(2, campaign.NextCampaignMission);
            Test.True(campaign.CanOpenTraining, "completed mission enables training");
            Test.Equal("Advance to Next Campaign Mission", campaign.PrimaryLabel);
            Test.Equal("Replay Previous Missions in Training Simulation", campaign.TrainingLabel);

            var afterThree = new CampaignState();
            afterThree.CompletedMissions.UnionWith(["failed-override", "mission-2", "mission-3"]);
            var finale = new PostMissionNavigationState(afterThree, 3, MissionRunMode.Campaign);
            Test.True(!finale.CanAdvanceCampaign, "current final mission has no campaign advance");
            Test.Equal(null, finale.NextCampaignMission);

            var training = new PostMissionNavigationState(afterOne, 1, MissionRunMode.TrainingSimulation);
            Test.Equal("Replay Another Training Mission", training.PrimaryLabel);
            Test.Equal("Return to Campaign", training.ReturnLabel);
            Test.Equal("Return to Main Menu", training.SecondaryLabel);
            Test.Equal(PostMissionReturnTarget.CampaignMission, training.ReturnTarget);
            var finalTraining = new PostMissionNavigationState(afterThree, 3, MissionRunMode.TrainingSimulation);
            Test.Equal(PostMissionReturnTarget.MainMenu, finalTraining.ReturnTarget);
            Test.Equal("Return to Main Menu", finalTraining.ReturnLabel);
            Test.True(!finalTraining.ShowSecondaryAction, "final simulation does not duplicate the main-menu action");
        });

        Test.Run("simulation_presentation_is_persistent_and_skips_story_scenes", () =>
        {
            Test.Equal("VIRTUAL REPLAY", MissionRunPresentation.BattleBanner(MissionRunMode.TrainingSimulation));
            Test.Equal("TRAINING SIMULATION COMPLETE", MissionRunPresentation.ResultHeading(MissionRunMode.TrainingSimulation, victory: true));
            Test.Equal("TRAINING SIMULATION FAILED", MissionRunPresentation.ResultHeading(MissionRunMode.TrainingSimulation, victory: false));
            Test.Equal("TRAINING SIMULATION REWARDS", MissionRunPresentation.RewardHeading(MissionRunMode.TrainingSimulation));
            Test.Equal("issac", MissionRunPresentation.ArtworkId("simulated-issac-captive"));
            Test.Equal("", MissionRunPresentation.BattleBanner(MissionRunMode.Campaign));
            Test.True(!MissionRunPresentation.ShouldPlayIntro(MissionRunMode.TrainingSimulation, 2), "simulation skips Mission 2 intro");
            Test.True(!MissionRunPresentation.ShouldPlayIntro(MissionRunMode.TrainingSimulation, 3), "simulation skips Mission 3 intro");
            Test.True(!MissionRunPresentation.ShouldPlayWrapUp(MissionRunMode.TrainingSimulation, 1), "simulation skips Mission 1 wrap-up");
            Test.True(!MissionRunPresentation.ShouldPlayWrapUp(MissionRunMode.TrainingSimulation, 2), "simulation skips Mission 2 wrap-up");
            Test.True(!MissionRunPresentation.ShouldPlayTutorial(MissionRunMode.TrainingSimulation, 1), "simulation skips tutorial");
            Test.True(MissionRunPresentation.ShouldPlayIntro(MissionRunMode.Campaign, 2), "campaign keeps Mission 2 intro");
            Test.True(MissionRunPresentation.ShouldPlayWrapUp(MissionRunMode.Campaign, 1), "campaign keeps Mission 1 wrap-up");
            Test.True(MissionRunPresentation.ShouldPlayTutorial(MissionRunMode.Campaign, 1), "campaign keeps tutorial");
        });

        Test.Run("simulation_crew_selection_uses_unlocked_roster_and_explicit_copy", () =>
        {
            var campaign = new CampaignState();
            campaign.CompletedMissions.Add("failed-override");
            var state = new CrewSelectionState(campaign, 1, [], MissionRunMode.TrainingSimulation);
            foreach (var id in new[] { "xander", "nance", "akiko", "issac" }) Test.True(state.Toggle(id), $"simulation selects {id}");
            Test.True(state.CanContinue, "four unlocked simulation crew can continue");
            Test.Equal("SELECT YOUR SIMULATION CREW", CrewSelectionPresentation.TitleFor(MissionRunMode.TrainingSimulation));
            Test.True(CrewSelectionPresentation.InstructionFor(MissionRunMode.TrainingSimulation).Contains("permanent"), "simulation crew copy explains permanent progress");
            Test.True(PostMissionNavigationState.CanOpenTrainingFromLanding(campaign), "landing training action appears after completion");
            Test.True(!PostMissionNavigationState.CanOpenTrainingFromLanding(new CampaignState()), "new campaign hides training action");
        });

        Test.Run("mission_three_roster_selects_exactly_four_of_six", () =>
        {
            var state = new CrewSelectionState(new CampaignState(), 3, []);
            Test.Equal("CREW SELECTED 0 / 4", state.CountLabel);
            Test.True(!state.CanContinue, "an empty roster cannot continue");
            foreach (var id in new[] { "xander", "nance", "akiko" }) Test.True(state.Toggle(id), $"selects {id}");
            Test.True(!state.CanContinue, "three characters cannot continue");
            Test.True(state.Toggle("jeane"), "selects Captain Gershon fourth");
            Test.True(state.CanContinue, "exactly four available characters can continue");
            Test.Equal("CREW SELECTED 4 / 4", state.CountLabel);
            Test.True(!state.CanSelect("issac"), "a fifth character is locked until one is removed");
            Test.True(state.Toggle("nance"), "a selected optional character can be removed");
            Test.True(!state.CanContinue, "removing one disables Continue");
        });

        Test.Run("selection_rejects_duplicates_and_unavailable_ids", () =>
        {
            var state = new CrewSelectionState(new CampaignState(), 3, new[] { "xander", "xander", "unknown", "akiko" });
            Test.Equal(2, state.SelectedIds.Count);
            Test.Equal("xander", state.SelectedIds[0]);
            Test.Equal("akiko", state.SelectedIds[1]);
            Test.True(!state.Toggle("unknown"), "unavailable IDs are rejected");
            Test.True(state.Toggle("xander"), "a duplicate seed produces only one removable selection");
            Test.True(!state.IsSelected("xander"), "the single normalized selection is removed");
        });

        Test.Run("required_crew_are_locked_and_count_toward_limit", () =>
        {
            var state = new CrewSelectionState(new CampaignState(), 2, []);
            Test.Equal(4, state.SelectedIds.Count);
            Test.True(state.CanContinue, "Mission 2's four required crew form a valid roster");
            foreach (var id in new[] { "xander", "nance", "akiko", "issac" })
            {
                Test.True(state.IsRequired(id), $"{id} is required");
                Test.True(state.IsSelected(id), $"{id} starts selected");
                Test.True(!state.CanSelect(id), $"{id} cannot be toggled off");
                Test.True(!state.Toggle(id), $"{id} remains locked");
            }
        });

        Test.Run("four_required_crew_lock_optional_cards", () =>
        {
            var state = new CrewSelectionState(new CampaignState(), 2, []);
            Test.True(!state.CanSelect("jeane"), "a card outside the mission roster cannot be selected when required slots are full");
            Test.Equal("Four crew members selected. Remove one optional crew member to choose another.", CrewSelectionPresentation.MaximumMessage);
        });

        Test.Run("selection_order_survives_toggle_and_reopen", () =>
        {
            var state = new CrewSelectionState(new CampaignState(), 3, new[] { "chrandy", "akiko", "jeane", "xander" });
            Test.True(state.Toggle("akiko"), "Akiko can be removed");
            Test.True(state.Toggle("issac"), "Issac fills the open slot");
            Test.Equal("chrandy,jeane,xander,issac", string.Join(',', state.SelectedIds));

            var reopened = new CrewSelectionState(new CampaignState(), 3, state.SelectedIds);
            Test.Equal("chrandy,jeane,xander,issac", string.Join(',', reopened.SelectedIds));
            Test.True(reopened.CanContinue, "reopened valid draft remains ready");
        });

        Test.Run("crew_selection_copy_is_explicit_and_accessible", () =>
        {
            Test.Equal("SELECT YOUR CREW", CrewSelectionPresentation.Title);
            Test.Equal("Choose 4 crew members for this mission.", CrewSelectionPresentation.Instruction);
            Test.Equal("✓ SELECTED", CrewSelectionPresentation.CardStatus(selected: true, required: false));
            Test.Equal("REQUIRED", CrewSelectionPresentation.CardStatus(selected: true, required: true));
            Test.Equal("", CrewSelectionPresentation.CardStatus(selected: false, required: false));
            Test.Equal("Continue to Preparation", CrewSelectionPresentation.ContinueLabel);

            var open = new CrewSelectionState(new CampaignState(), 3, ["xander", "nance", "akiko"]);
            var full = new CrewSelectionState(new CampaignState(), 3, ["xander", "nance", "akiko", "jeane"]);
            Test.Equal("", CrewSelectionPresentation.SelectionMessage(open));
            Test.Equal("Four crew members selected. Remove one optional crew member to choose another.", CrewSelectionPresentation.SelectionMessage(full));
        });

        Test.Run("class_progression_requires_matching_first_tier", () =>
        {
            var campaign=new CampaignState();var p=campaign.Progress("xander");p.PendingClassProgressions=2;
            Test.True(!campaign.ChooseClassProgression("xander","xander-marksman-2"),"tier two locked");
            Test.True(campaign.ChooseClassProgression("xander","xander-marksman-1"),"tier one selected");
            Test.True(campaign.ChooseClassProgression("xander","xander-marksman-2"),"matching tier two unlocked");
            Test.True(!campaign.ChooseClassProgression("xander","xander-marksman-2"),"cannot select twice");
        });

        Test.Run("captain_has_elite_base_stats_and_weapons", () =>
        {
            var battle = new BattleState(3, new CampaignState(), ["xander", "nance", "chrandy", "jeane"]);
            var captain = battle.Units.Single(unit => unit.Id == "jeane");
            Test.Equal(Role.Captain, captain.Role);
            Test.Equal(24, captain.MaxHp);
            Test.Equal(2, captain.Armor);
            Test.Equal(6, captain.Move);
            Test.Equal(2, captain.Weapons.Count);
            Test.Equal(new Weapon("Pulse rifle", 8, 4), captain.Weapons[0]);
            Test.Equal(new Weapon("Officer's blade", 10, 1), captain.Weapons[1]);
        });

        Test.Run("captain_issue_order_refreshes_one_named_ally_once", () =>
        {
            var battle = new BattleState(3, new CampaignState(), ["xander", "nance", "akiko", "jeane"]);
            var captain = battle.Units.Single(unit => unit.Id == "jeane");
            var xander = battle.Units.Single(unit => unit.Id == "xander");
            captain.Position = new Cell(5, 5);
            xander.Position = new Cell(5, 7);
            var destination = battle.Reach(xander).Keys.First(cell => cell != xander.Position && cell.Distance(captain.Position) <= 2);
            Test.True(battle.Move(xander, destination), "Xander has a movement action to refresh");
            xander.Acted = true;

            Test.True(battle.CanIssueOrder(captain, xander), "a nearby named ally who acted is a legal target");
            Test.True(battle.IssueOrder(captain, xander), "Captain issues her order");
            Test.True(!xander.Moved && !xander.Acted, "the ally receives a complete additional turn");
            Test.True(captain.Acted, "Issue Order costs the Captain's action");
            Test.Equal(0, battle.IssueOrderUsesRemaining);
            Test.True(!battle.UndoMove(xander), "the refreshed ally cannot undo the movement from before the order");
            captain.Acted = false;
            xander.Acted = true;
            Test.True(!battle.IssueOrder(captain, xander), "base Issue Order cannot be used twice");
            Test.Equal(20, battle.LastXpAward);
        });

        Test.Run("captain_issue_order_rejects_illegal_targets", () =>
        {
            var battle = new BattleState(3, new CampaignState(), ["xander", "nance", "akiko", "jeane"]);
            var captain = battle.Units.Single(unit => unit.Id == "jeane");
            var xander = battle.Units.Single(unit => unit.Id == "xander");
            var akiko = battle.Units.Single(unit => unit.Id == "akiko");
            captain.Position = new Cell(5, 5);
            xander.Position = new Cell(5, 7);
            xander.Moved = true;
            Test.True(!battle.CanIssueOrder(captain, captain), "Captain cannot order herself");
            Test.True(!battle.CanIssueOrder(captain, akiko), "a fresh ally does not need an order");
            xander.Position = new Cell(5, 8);
            Test.True(!battle.CanIssueOrder(captain, xander), "base order range is two");
            xander.Position = new Cell(5, 7);
            xander.Hp = 0;
            Test.True(!battle.CanIssueOrder(captain, xander), "a defeated ally cannot be ordered");
            xander.Hp = xander.MaxHp;
            xander.Captured = true;
            Test.True(!battle.CanIssueOrder(captain, xander), "a captured ally cannot be ordered");
            xander.Captured = false;

            var robot = new Unit { Id = "robot-test", Name = "Support robot", Token = "R", Role = Role.Robot, Team = Team.Player, Position = new Cell(5, 6), Hp = 10, BaseHp = 10, Moved = true, ControllerId = "akiko" };
            var droid = battle.Units.First(unit => unit.Team == Team.Enemy);
            droid.Team = Team.Player;
            droid.ControllerId = "issac";
            droid.Position = new Cell(4, 5);
            droid.Moved = true;
            Test.True(!battle.CanIssueOrder(captain, robot), "summoned robots cannot receive Captain's order");
            Test.True(!battle.CanIssueOrder(captain, droid), "reprogrammed robots cannot receive Captain's order");
            xander.Moved = xander.Acted = true;
            xander.SkippedByStun = true;
            Test.True(!battle.CanIssueOrder(captain, xander), "a stunned ally who skipped the turn cannot receive an order");
        });

        Test.Run("captain_class_progressions_apply_their_tiered_effects", () =>
        {
            var definitions = ClassProgressionCatalog.For("jeane");
            Test.Equal(6, definitions.Count);
            Test.True(definitions.Count(item => item.Tier == 1 && item.Prerequisite is null) == 3, "three tier-one Captain paths");
            Test.True(definitions.Count(item => item.Tier == 2 && item.Prerequisite is not null) == 3, "three prerequisite-gated tier-two rewards");

            var commandCampaign = new CampaignState();
            commandCampaign.Progress("jeane").ClassProgressions.UnionWith(["jeane-command-1", "jeane-command-2"]);
            var commandBattle = new BattleState(3, commandCampaign, ["xander", "nance", "chrandy", "jeane"]);
            var captain = commandBattle.Units.Single(unit => unit.Id == "jeane");
            var xander = commandBattle.Units.Single(unit => unit.Id == "xander");
            captain.Position = new Cell(5, 5);
            xander.Position = new Cell(5, 8);
            xander.Acted = true;
            Test.True(commandBattle.CanIssueOrder(captain, xander), "Command I increases range to three");
            Test.True(commandBattle.IssueOrder(captain, xander), "first Command II order succeeds");
            Test.Equal(1, commandBattle.IssueOrderUsesRemaining);
            commandBattle.StartEnemy(); commandBattle.StartPlayer();
            captain.Acted = false; xander.Acted = true;
            Test.True(!commandBattle.CanIssueOrder(captain, xander), "round 2 remains on cooldown");
            commandBattle.StartEnemy(); commandBattle.StartPlayer();
            captain.Acted = false; xander.Acted = true;
            Test.True(!commandBattle.CanIssueOrder(captain, xander), "round 3 remains on cooldown");
            commandBattle.StartEnemy(); commandBattle.StartPlayer();
            captain.Acted = false; xander.Acted = true;
            Test.True(commandBattle.CanIssueOrder(captain, xander), "round 4 unlocks the second order");
            Test.Equal(0, commandBattle.IssueOrderCooldownRounds);
            Test.True(commandBattle.IssueOrder(captain, xander), "second order succeeds after cooldown");

            var marksmanCampaign = new CampaignState();
            marksmanCampaign.Progress("jeane").ClassProgressions.UnionWith(["jeane-sharpshooter-1", "jeane-sharpshooter-2"]);
            var marksmanBattle = new BattleState(3, marksmanCampaign, ["xander", "nance", "chrandy", "jeane"]);
            var marksman = marksmanBattle.Units.Single(unit => unit.Id == "jeane");
            var armored = marksmanBattle.Units.First(unit => unit.Team == Team.Enemy);
            marksman.Position = new Cell(5, 5); armored.Position = new Cell(5, 8);
            Test.Equal(5, marksman.Weapons[0].Range);
            var pulseDamage = marksmanBattle.PreviewDamage(marksman, armored, marksman.Weapons[0]);
            var expectedPulse = Math.Max(1, marksman.Weapons[0].Power - Math.Max(0, armored.Armor - 1));
            Test.Equal(expectedPulse, pulseDamage);
            Test.Equal(Math.Max(1, marksman.Weapons[1].Power - armored.Armor), marksmanBattle.PreviewDamage(marksman, armored, marksman.Weapons[1]));

            var duelistCampaign = new CampaignState();
            duelistCampaign.Progress("jeane").ClassProgressions.UnionWith(["jeane-duelist-1", "jeane-duelist-2"]);
            var duelistBattle = new BattleState(3, duelistCampaign, ["xander", "nance", "chrandy", "jeane"]);
            var duelist = duelistBattle.Units.Single(unit => unit.Id == "jeane");
            var target = duelistBattle.Units.First(unit => unit.Team == Team.Enemy);
            Test.Equal(12, duelist.Weapons[1].Power);
            duelist.Position = new Cell(5, 5); target.Position = new Cell(5, 6); target.Hp = 1; duelist.Moved = true;
            Test.True(duelistBattle.Attack(duelist, target, 1), "Duelist defeats an adjacent enemy");
            Test.True(!duelist.Moved, "Duelist II restores movement after the melee defeat");
        });

        Test.Run("mission_one_and_two_skip_roster_selection", () =>
        {
            Test.Equal(MissionLaunchStage.CrewPreparation, new MissionLaunchFlowState(1, []).Stage);
            Test.Equal(MissionLaunchStage.CrewPreparation, new MissionLaunchFlowState(2, []).Stage);
        });

        Test.Run("mission_three_opens_selection_before_preparation", () =>
        {
            var flow = new MissionLaunchFlowState(3, []);
            Test.Equal(MissionLaunchStage.RosterSelection, flow.Stage);
            Test.True(!flow.ConfirmRoster(["xander", "nance", "akiko"]), "three crew cannot advance");
            Test.True(flow.ConfirmRoster(["xander", "nance", "akiko", "issac"]), "four crew advance");
            Test.Equal(MissionLaunchStage.CrewPreparation, flow.Stage);
        });

        Test.Run("preparation_back_preserves_roster_draft", () =>
        {
            var roster = new[] { "jeane", "chrandy", "nance", "issac" };
            var flow = new MissionLaunchFlowState(3, []);
            flow.ConfirmRoster(roster);
            flow.BackToRoster();
            Test.Equal(MissionLaunchStage.RosterSelection, flow.Stage);
            Test.True(flow.DraftRoster.SequenceEqual(roster), "draft order is preserved");
        });

        Test.Run("mission_three_intro_receives_confirmed_four", () =>
        {
            var roster = new[] { "jeane", "chrandy", "xander", "akiko" };
            var flow = new MissionLaunchFlowState(3, []);
            flow.ConfirmRoster(roster);
            Test.True(flow.BeginCinematic(), "prepared roster begins cinematic");
            Test.Equal(MissionLaunchStage.Cinematic, flow.Stage);
            Test.True(flow.DraftRoster.SequenceEqual(roster), "cinematic receives confirmed roster");
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

        Test.Run("simulation_veterans_keep_full_xp_and_gain_available_progression", () =>
        {
            var campaign = new CampaignState();
            campaign.CompletedMissions.Add("failed-override");
            campaign.Progress("xander").CompletedMissionIds.Add("failed-override");
            campaign.Progress("xander").Xp = 35;
            var battle = new BattleState(1, campaign, ["xander", "nance", "akiko", "issac"], MissionRunMode.TrainingSimulation);
            var xander = battle.Units.Single(u => u.Id == "xander");
            xander.Position = battle.Issac.Position.Neighbors().First(battle.Floor);

            Test.True(battle.Interact(xander), "replay objective completes");

            Test.Equal(55, campaign.Progress("xander").Xp);
            Test.Equal(1, campaign.Progress("xander").PendingClassProgressions);
            Test.Equal("failed-override", string.Join(',', campaign.CompletedMissions));
        });

        Test.Run("simulation_commits_only_deployed_character_progress", () =>
        {
            var campaign = new CampaignState();
            campaign.CompletedMissions.UnionWith(["failed-override", "mission-2", "mission-3"]);
            campaign.Progress("xander").Xp = 10;
            var roster = new[] { "xander", "nance", "issac", "chrandy" };
            var battle = new BattleState(3, campaign, roster, MissionRunMode.TrainingSimulation);
            campaign.AwardXp("xander", 40);
            campaign.AwardXp("chrandy", 30);
            foreach (var enemy in battle.Units.Where(u => u.Team == Team.Enemy)) enemy.Hp = 0;

            battle.Evaluate();

            Test.Equal(50, campaign.Progress("xander").Xp);
            Test.Equal(1, campaign.Progress("xander").PendingClassProgressions);
            Test.Equal(30, campaign.Progress("chrandy").Xp);
            Test.Equal(1, campaign.Progress("chrandy").PendingClassProgressions);
            Test.Equal(0, campaign.Progress("akiko").Xp);
            Test.Equal(0, campaign.Progress("akiko").PendingClassProgressions);
            Test.Equal(40, battle.MissionXpGains["xander"]);
            Test.Equal(30, battle.MissionXpGains["chrandy"]);
            Test.True(battle.NewlyRewardedIds.SetEquals(roster), "each deployed crew member with catalog room receives a reward");
        });

        Test.Run("campaign_victory_never_awards_reserve_catchup_xp", () =>
        {
            var campaign = new CampaignState();
            var battle = new BattleState(3, campaign, new[] { "xander", "nance", "issac", "chrandy" });
            campaign.AwardXp("nance", 30);
            campaign.AwardXp("issac", 30);
            campaign.AwardXp("chrandy", 30);
            foreach (var enemy in battle.Units.Where(u => u.Team == Team.Enemy)) enemy.Hp = 0;

            battle.Evaluate();

            Test.Equal(0, campaign.Progress("akiko").Xp);
            Test.Equal(0, campaign.Progress("akiko").PendingClassProgressions);
            Test.True(campaign.CompletedMissions.Contains("mission-3"), "campaign victory completes the canonical mission");
        });

        Test.Run("class_progression_rewards_stop_when_the_catalog_is_exhausted", () =>
        {
            var campaign = new CampaignState();
            campaign.CompletedMissions.UnionWith(["failed-override", "mission-2", "mission-3"]);
            campaign.Progress("xander").ClassProgressions.UnionWith(ClassProgressionCatalog.For("xander").Select(choice => choice.Id));
            var battle = new BattleState(3, campaign, ["xander", "nance", "akiko", "jeane"], MissionRunMode.TrainingSimulation);
            foreach (var enemy in battle.Units.Where(unit => unit.Team == Team.Enemy)) enemy.Hp = 0;

            battle.Evaluate();

            Test.Equal(0, campaign.Progress("xander").PendingClassProgressions);
            Test.True(!battle.NewlyRewardedIds.Contains("xander"), "exhausted character is omitted from the reward queue");
        });

        Test.Run("mission_failure_restores_progression_and_bonds", () =>
        {
            var campaign = new CampaignState();
            campaign.Progress("xander").Xp = 12;
            campaign.Bonds[campaign.Key("xander", "nance")] = 1;
            var battle = new BattleState(3, campaign, ["xander", "nance", "akiko", "jeane"]);
            campaign.AwardXp("xander", 70);
            campaign.GrowBonds(["xander", "nance", "akiko", "jeane"]);
            foreach (var crew in battle.Units.Where(unit => unit.Team == Team.Player)) crew.Hp = 0;

            battle.Evaluate();

            Test.Equal(12, campaign.Progress("xander").Xp);
            Test.Equal(1, campaign.Bond("xander", "nance"));
            Test.Equal(0, campaign.Bond("xander", "akiko"));
        });

        Test.Run("mission_two_defeat_rules_distinguish_campaign_and_simulation", () =>
        {
            var simulationCampaign = new CampaignState();
            simulationCampaign.CompletedMissions.UnionWith(["failed-override", "mission-2"]);
            var simulation = new BattleState(2, simulationCampaign, ["xander", "nance", "akiko", "jeane"], MissionRunMode.TrainingSimulation);
            simulation.Evaluate();
            Test.Equal(Phase.Player, simulation.Phase);

            var campaign = new CampaignState();
            var story = new BattleState(2, campaign);
            foreach (var crew in story.Units.Where(unit => unit.Team == Team.Player && unit.Id != "issac")) crew.Hp = 0;
            story.Evaluate();
            Test.Equal(Phase.Defeat, story.Phase);
        });

        Test.Run("defeat_restores_absent_records_and_legacy_only_progress_exactly", () =>
        {
            var campaign = new CampaignState();
            campaign.CompletedMissions.Add("failed-override");
            campaign.Levels["jeane"] = 5;
            campaign.Experience["jeane"] = 42;
            var battle = new BattleState(1, campaign, ["xander", "nance", "akiko", "issac"], MissionRunMode.TrainingSimulation);
            Test.True(campaign.Progressions.ContainsKey("xander"), "battle materializes deployed progression after launch snapshot");
            battle.Issac.Hp = 0;
            battle.Evaluate();

            Test.Equal(Phase.Defeat, battle.Phase);
            Test.True(!campaign.Progressions.ContainsKey("xander"), "rollback removes a record absent before launch");
            Test.Equal(5, campaign.Levels["jeane"]);
            Test.Equal(42, campaign.Experience["jeane"]);
            Test.True(!campaign.Progressions.ContainsKey("jeane"), "legacy-only reserve remains legacy-only");
        });

        Test.Run("mission_one_simulation_keeps_playable_and_captive_issac_distinct", () =>
        {
            var campaign = new CampaignState();
            campaign.CompletedMissions.Add("failed-override");
            var battle = new BattleState(1, campaign, ["xander", "nance", "akiko", "issac"], MissionRunMode.TrainingSimulation);
            var playable = battle.Units.Single(unit => unit.Id == "issac");
            var captive = battle.Issac;
            Test.Equal(Team.Player, playable.Team);
            Test.Equal(Team.Civilian, captive.Team);
            Test.True(playable.Id != captive.Id, "the simulated captive has a distinct identity");
            playable.Position = captive.Position.Neighbors().First(battle.Floor);

            Test.True(battle.Interact(playable), "Issac can rescue the simulated Issac objective");
            Test.Equal(Phase.Victory, battle.Phase);
        });

        Test.Run("training_simulation_end_to_end_preserves_campaign_and_rolls_back_failure", () =>
        {
            var campaign = new CampaignState();
            foreach (var id in new[] { "xander", "nance", "akiko", "issac" })
                campaign.SetTrainingPath(id, TrainingPath.Attack);

            var campaignBattle = new BattleState(1, campaign);
            var campaignXander = campaignBattle.Units.Single(unit => unit.Id == "xander");
            campaignXander.Position = campaignBattle.Issac.Position.Neighbors().First(campaignBattle.Floor);
            Test.True(campaignBattle.Interact(campaignXander), "canonical Mission 1 can be completed");
            Test.True(TrainingSimulationPolicy.CompletedMissionNumbers(campaign).SequenceEqual([1]), "completion unlocks Mission 1 simulation");

            var roster = new[] { "xander", "nance", "akiko", "issac" };
            var reserveBefore = campaign.Progress("jeane").Clone();
            var xanderBefore = campaign.Progress("xander").Clone();
            var pendingBefore = xanderBefore.PendingClassProgressions;
            var simulation = new BattleState(1, campaign, roster, MissionRunMode.TrainingSimulation);
            var xander = simulation.Units.Single(unit => unit.Id == "xander");
            var enemy = simulation.Units.First(unit => unit.Team == Team.Enemy);
            xander.Position = enemy.Position.Neighbors().First(simulation.Floor);
            Test.True(simulation.Attack(xander, enemy, 1), "simulation participant earns combat XP");
            xander.Acted = false;
            xander.Position = simulation.Issac.Position.Neighbors().First(simulation.Floor);
            Test.True(simulation.Interact(xander), "simulation rescue completes the replay");

            Test.Equal(xanderBefore.Xp + 45, campaign.Progress("xander").Xp);
            Test.Equal(pendingBefore + 1, campaign.Progress("xander").PendingClassProgressions);
            Test.Equal(reserveBefore.Xp, campaign.Progress("jeane").Xp);
            Test.Equal(reserveBefore.PendingClassProgressions, campaign.Progress("jeane").PendingClassProgressions);
            Test.True(campaign.CompletedMissions.SetEquals(["failed-override"]), "simulation does not mutate canonical completion");

            var beforeFailedReplay = campaign.Progress("xander").Clone();
            var failed = new BattleState(1, campaign, roster, MissionRunMode.TrainingSimulation);
            var failedXander = failed.Units.Single(unit => unit.Id == "xander");
            var failedEnemy = failed.Units.First(unit => unit.Team == Team.Enemy);
            failedXander.Position = failedEnemy.Position.Neighbors().First(failed.Floor);
            Test.True(failed.Attack(failedXander, failedEnemy, 1), "failed replay temporarily awards XP");
            failed.Issac.Hp = 0;
            failed.Evaluate();
            Test.Equal(Phase.Defeat, failed.Phase);
            Test.Equal(beforeFailedReplay.Xp, campaign.Progress("xander").Xp);
            Test.Equal(beforeFailedReplay.PendingClassProgressions, campaign.Progress("xander").PendingClassProgressions);
            Test.True(campaign.CompletedMissions.SetEquals(["failed-override"]), "defeat rollback preserves canonical completion");
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
            Test.True(!campaign.HasMissionCredit("jeane", "mission-3"), "legacy saves predate Gershon becoming deployable");
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
