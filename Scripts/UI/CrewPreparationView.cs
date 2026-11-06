using FailedOverride.Campaign;
using Godot;

namespace FailedOverride.UI;

public partial class CrewPreparationView : PanelContainer
{
    static readonly Dictionary<string, (string Name, string ClassName, int Health, int Armor, int Movement)> Crew = new()
    {
        ["xander"] = ("Xander McRaven", "Scout", 18, 1, 6),
        ["nance"] = ("Nance Parker", "Shapeshifter", 20, 1, 5),
        ["akiko"] = ("Akiko Ikeda", "Tech", 16, 1, 5),
        ["issac"] = ("Issac Wynter", "Science Officer", 18, 0, 4),
        ["chrandy"] = ("Dr. Chrandy Perra", "Medic", 17, 1, 5)
    };

    readonly VBoxContainer content = new();
    CampaignState campaign = null!;
    CrewPreparationState state = null!;
    IReadOnlyDictionary<string, Texture2D> portraits = new Dictionary<string, Texture2D>();
    IReadOnlyDictionary<string, int> xpGains = new Dictionary<string, int>();
    IReadOnlySet<string> newlyRewarded = new HashSet<string>();
    Action<int, IReadOnlyList<string>>? begin;
    Action? finishRewards;
    Action? confirmedChoice;
    bool rewardMode;
    string currentId = "";
    readonly HashSet<string> confirmed = [];

    public CrewPreparationView()
    {
        Visible = false;
        Position = new Vector2(70, 52);
        Size = new Vector2(1260, 930);
        MouseFilter = MouseFilterEnum.Stop;
        var style = new StyleBoxFlat { BgColor = new Color("0b1727f7"), BorderColor = new Color("d2a84f") };
        style.SetBorderWidthAll(3);
        style.SetCornerRadiusAll(12);
        AddThemeStyleboxOverride("panel", style);
        var margin = new MarginContainer();
        foreach (var edge in new[] { "left", "right", "top", "bottom" }) margin.AddThemeConstantOverride("margin_" + edge, 22);
        AddChild(margin);
        var scroll = new ScrollContainer
        {
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            VerticalScrollMode = ScrollContainer.ScrollMode.Auto,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        margin.AddChild(scroll);
        content.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        scroll.AddChild(content);
    }

    public void OpenPreparation(CampaignState value, int mission, IEnumerable<string> selected, IReadOnlyDictionary<string, Texture2D> portraitTextures, Action<int, IReadOnlyList<string>> onBegin, Action onConfirmedChoice)
    {
        campaign = value;
        state = new(value, mission, selected);
        portraits = portraitTextures;
        begin = onBegin;
        finishRewards = null;
        confirmedChoice = onConfirmedChoice;
        rewardMode = false;
        confirmed.Clear();
        currentId = value.AvailableCrew(mission).First();
        Visible = true;
        Rebuild();
    }

    public void OpenRewards(CampaignState value, int mission, IEnumerable<string> deployed, IReadOnlyDictionary<string, Texture2D> portraitTextures, IReadOnlyDictionary<string, int> missionXpGains, IReadOnlySet<string> rewardRecipients, Action onFinished, Action onConfirmedChoice)
    {
        campaign = value;
        var roster = deployed.ToList();
        state = new(value, mission, roster);
        portraits = portraitTextures;
        xpGains = missionXpGains;
        newlyRewarded = rewardRecipients;
        finishRewards = onFinished;
        confirmedChoice = onConfirmedChoice;
        begin = null;
        rewardMode = true;
        confirmed.Clear();
        currentId = roster.First();
        Visible = true;
        Rebuild();
    }

    void Rebuild()
    {
        foreach (var child in content.GetChildren()) { content.RemoveChild(child); child.QueueFree(); }
        var header = new Label { Text = rewardMode ? "MISSION REWARDS" : $"CREW PREPARATION · MISSION {state.Mission}" };
        header.AddThemeFontSizeOverride("font_size", 30);
        header.AddThemeColorOverride("font_color", new Color("f3c979"));
        content.AddChild(header);
        if (rewardMode) content.AddChild(new Label { Text = RewardSummary(), AutowrapMode = TextServer.AutowrapMode.WordSmart, CustomMinimumSize = new Vector2(0, 52), SizeFlagsHorizontal = SizeFlags.ExpandFill });
        else BuildTrainingExplanation();
        BuildCrewTabs();
        if (rewardMode && confirmed.IsSupersetOf(state.SelectedIds)) BuildCompletedSummary();
        else BuildCharacterCard(currentId);
    }

    string RewardSummary() => string.Join("   •   ", state.SelectedIds.Select(id =>
        $"{Crew[id].Name}: +{xpGains.GetValueOrDefault(id)} XP{(newlyRewarded.Contains(id) ? " + Class Progression" : " (practice replay)")}"));

    void BuildTrainingExplanation()
    {
        var panel = new PanelContainer { CustomMinimumSize = new Vector2(0, 138) };
        var panelStyle = new StyleBoxFlat { BgColor = new Color("132c42"), BorderColor = new Color("55e6ff") };
        panelStyle.SetBorderWidthAll(2);
        panelStyle.SetCornerRadiusAll(9);
        panel.AddThemeStyleboxOverride("panel", panelStyle);
        content.AddChild(panel);
        var margin = new MarginContainer();
        foreach (var edge in new[] { "left", "right", "top", "bottom" }) margin.AddThemeConstantOverride("margin_" + edge, 11);
        panel.AddChild(margin);
        var column = new VBoxContainer();
        column.AddThemeConstantOverride("separation", 5);
        margin.AddChild(column);
        var title = new Label { Text = "HOW LEVEL UPS WORK" };
        title.AddThemeFontSizeOverride("font_size", 19);
        title.AddThemeColorOverride("font_color", new Color("75f3ff"));
        column.AddChild(title);
        var explanation = new Label { Text = CrewPreparationPresentation.TrainingPathExplanation, AutowrapMode = TextServer.AutowrapMode.WordSmart };
        explanation.AddThemeFontSizeOverride("font_size", 16);
        explanation.AddThemeColorOverride("font_color", new Color("eef8ff"));
        column.AddChild(explanation);
        var instruction = new Label { Text = CrewPreparationPresentation.TrainingPathInstruction };
        instruction.AddThemeFontSizeOverride("font_size", 17);
        instruction.AddThemeColorOverride("font_color", new Color("f3c979"));
        column.AddChild(instruction);
    }

    void BuildCrewTabs()
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 8);
        content.AddChild(row);
        foreach (var id in rewardMode ? state.SelectedIds : campaign.AvailableCrew(state.Mission))
        {
            var button = new Button { Text = Crew[id].Name, ToggleMode = true, ButtonPressed = id == currentId, CustomMinimumSize = new Vector2(0, 42) };
            button.Pressed += () => { currentId = id; Rebuild(); };
            row.AddChild(button);
            if (!rewardMode)
            {
                var deploy = new CheckButton { Text = state.IsRequired(id) ? "Required" : "Deploy", ButtonPressed = state.IsSelected(id), Disabled = state.IsRequired(id) };
                deploy.Toggled += selected => { state.ToggleDeployment(id, selected); Rebuild(); };
                row.AddChild(deploy);
            }
        }
    }

    void BuildCharacterCard(string id)
    {
        var profile = Crew[id];
        var progress = campaign.Progress(id);
        var card = new HBoxContainer();
        card.AddThemeConstantOverride("separation", 22);
        content.AddChild(card);
        var portrait = new TextureRect { Texture = portraits.GetValueOrDefault(id), CustomMinimumSize = new Vector2(230, 330), ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered };
        card.AddChild(portrait);
        var details = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        details.AddThemeConstantOverride("separation", 9);
        card.AddChild(details);
        var name = new Label { Text = $"{profile.Name}  ·  {profile.ClassName}  ·  Level {progress.Level}" };
        name.AddThemeFontSizeOverride("font_size", 24);
        details.AddChild(name);
        details.AddChild(new Label { Text = $"Health {profile.Health + progress.HealthBonus}     Armor {profile.Armor + progress.ArmorBonus}     Movement {profile.Movement + progress.MovementBonus}     Attack +{progress.AttackBonus}" });
        var xp = new ProgressBar { MinValue = 0, MaxValue = 100, Value = progress.Xp, ShowPercentage = false, CustomMinimumSize = new Vector2(0, 24) };
        details.AddChild(xp);
        details.AddChild(new Label { Text = $"{progress.Xp} / 100 XP" });
        var pathTitle = new Label { Text = "TRAINING PATH · determines every automatic Level Up" };
        pathTitle.AddThemeColorOverride("font_color", new Color("55e6ff"));
        pathTitle.AddThemeFontSizeOverride("font_size", 18);
        details.AddChild(pathTitle);
        var paths = new HBoxContainer();
        details.AddChild(paths);
        foreach (var path in new[] { TrainingPath.Health, TrainingPath.Armor, TrainingPath.Movement, TrainingPath.Attack })
        {
            var localPath = path;
            var selected = state.TrainingPathFor(id) == path;
            var button = new Button { Text = CrewPreparationPresentation.PathLabel(path, selected), ToggleMode = true, ButtonPressed = selected, CustomMinimumSize = new Vector2(145, 72) };
            ApplyPathStyle(button, selected);
            button.Pressed += () => { state.SetTrainingPath(id, localPath); confirmed.Remove(id); Rebuild(); };
            paths.AddChild(button);
        }
        var acquired = state.AcquiredClassProgressions(id);
        details.AddChild(new Label { Text = "ACQUIRED CLASS PROGRESSIONS\n" + (acquired.Count == 0 ? "None yet" : string.Join("\n", acquired.Select(x => $"• {x.Name}: {x.Description}"))), AutowrapMode = TextServer.AutowrapMode.WordSmart });
        var available = state.AvailableClassProgressions(id);
        if (available.Count > 0)
        {
            BuildClassProgressionExplanation(details);
            var reward = new Label { Text = "★ CLASS PROGRESSION EARNED · choose one" };
            reward.AddThemeColorOverride("font_color", new Color("f3c979"));
            reward.AddThemeFontSizeOverride("font_size", 20);
            details.AddChild(reward);
            foreach (var progression in available)
            {
                var local = progression;
                var selected = state.DraftClassProgressionFor(id) == progression.Id;
                var choice = new Button
                {
                    Text = CrewPreparationPresentation.ClassProgressionLabel(progression, selected),
                    ToggleMode = true,
                    ButtonPressed = selected,
                    AutowrapMode = TextServer.AutowrapMode.WordSmart,
                    CustomMinimumSize = new Vector2(0, selected ? 82 : 68)
                };
                ApplyPathStyle(choice, selected);
                choice.Pressed += () => { state.SetClassProgression(id, local.Id); confirmed.Remove(id); Rebuild(); };
                details.AddChild(choice);
            }
        }
        else if (rewardMode && !newlyRewarded.Contains(id))
        {
            details.AddChild(new Label { Text = "MISSION ALREADY COMPLETED · Existing abilities may be reviewed; no additional permanent reward is granted." });
        }
        var confirm = new Button { Text = $"Confirm {profile.Name}'s Choices", Disabled = !state.CanConfirmCharacter(id), CustomMinimumSize = new Vector2(0, 50) };
        confirm.Pressed += () => ConfirmCurrent(id);
        details.AddChild(confirm);
        if (!rewardMode)
        {
            var beginButton = new Button { Text = "Begin Mission", Disabled = !state.CanBeginMission(), CustomMinimumSize = new Vector2(0, 54) };
            beginButton.Pressed += () => { if (state.CanBeginMission()) { Visible = false; begin?.Invoke(state.Mission, state.SelectedIds.ToList()); } };
            details.AddChild(beginButton);
        }
    }

    static void BuildClassProgressionExplanation(VBoxContainer details)
    {
        var panel = new PanelContainer { CustomMinimumSize = new Vector2(0, 126) };
        var panelStyle = new StyleBoxFlat { BgColor = new Color("132c42"), BorderColor = new Color("55e6ff") };
        panelStyle.SetBorderWidthAll(2);
        panelStyle.SetCornerRadiusAll(9);
        panel.AddThemeStyleboxOverride("panel", panelStyle);
        details.AddChild(panel);

        var margin = new MarginContainer();
        foreach (var edge in new[] { "left", "right", "top", "bottom" }) margin.AddThemeConstantOverride("margin_" + edge, 10);
        panel.AddChild(margin);
        var column = new VBoxContainer();
        column.AddThemeConstantOverride("separation", 4);
        margin.AddChild(column);

        var title = new Label { Text = "HOW CLASS PROGRESSIONS WORK" };
        title.AddThemeFontSizeOverride("font_size", 18);
        title.AddThemeColorOverride("font_color", new Color("75f3ff"));
        column.AddChild(title);
        var explanation = new Label { Text = CrewPreparationPresentation.ClassProgressionExplanation, AutowrapMode = TextServer.AutowrapMode.WordSmart };
        explanation.AddThemeFontSizeOverride("font_size", 15);
        explanation.AddThemeColorOverride("font_color", new Color("eef8ff"));
        column.AddChild(explanation);
        var instruction = new Label { Text = CrewPreparationPresentation.ClassProgressionInstruction };
        instruction.AddThemeFontSizeOverride("font_size", 16);
        instruction.AddThemeColorOverride("font_color", new Color("f3c979"));
        column.AddChild(instruction);
    }

    void ConfirmCurrent(string id)
    {
        if (!state.ConfirmCharacter(id)) return;
        confirmedChoice?.Invoke();
        if (campaign.Progress(id).PendingClassProgressions > 0) { Rebuild(); return; }
        confirmed.Add(id);
        currentId = state.NextUnconfirmedSelectedId(id, confirmed);
        Rebuild();
    }

    void BuildCompletedSummary()
    {
        var summary = new Label { Text = "REWARDS CONFIRMED\n\n" + string.Join("\n", state.SelectedIds.Select(id => $"{Crew[id].Name} · Training Path: {campaign.Progress(id).TrainingPath} · {campaign.Progress(id).ClassProgressions.Count} Class Progressions")), HorizontalAlignment = HorizontalAlignment.Center };
        summary.AddThemeFontSizeOverride("font_size", 22);
        content.AddChild(summary);
        var finish = new Button { Text = "Continue", CustomMinimumSize = new Vector2(0, 56) };
        finish.Pressed += () => { Visible = false; finishRewards?.Invoke(); };
        content.AddChild(finish);
    }

    static void ApplyPathStyle(Button button, bool selected)
    {
        StyleBoxFlat Style(Color background, Color border, int width)
        {
            var style = new StyleBoxFlat { BgColor = background, BorderColor = border };
            style.SetBorderWidthAll(width);
            style.SetCornerRadiusAll(8);
            return style;
        }
        var normal = selected ? Style(new Color("17677b"), new Color("75f3ff"), 3) : Style(new Color("16263b"), new Color("58708e"), 2);
        var hover = selected ? Style(new Color("20859a"), new Color("d8fcff"), 3) : Style(new Color("203953"), new Color("75f3ff"), 2);
        var pressed = selected ? Style(new Color("17677b"), new Color("d8fcff"), 3) : Style(new Color("0c3746"), new Color("d8fcff"), 3);
        var focus = Style(new Color(0, 0, 0, 0), new Color("f3c979"), 2);
        focus.DrawCenter = false;
        button.AddThemeStyleboxOverride("normal", normal);
        button.AddThemeStyleboxOverride("hover", hover);
        button.AddThemeStyleboxOverride("pressed", pressed);
        button.AddThemeStyleboxOverride("hover_pressed", selected ? hover : pressed);
        button.AddThemeStyleboxOverride("focus", focus);
        button.AddThemeColorOverride("font_color", selected ? new Color("ffffff") : new Color("d4e0ee"));
        button.AddThemeColorOverride("font_hover_color", new Color("ffffff"));
        button.AddThemeColorOverride("font_pressed_color", new Color("ffffff"));
        button.AddThemeColorOverride("font_hover_pressed_color", new Color("ffffff"));
        button.AddThemeFontSizeOverride("font_size", selected ? 16 : 15);
    }
}
