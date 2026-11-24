using FailedOverride.Campaign;
using Godot;

namespace FailedOverride.UI;

public partial class CrewSelectionView : PanelContainer
{
    readonly VBoxContainer content = new();
    CampaignState campaign = null!;
    CrewSelectionState state = null!;
    IReadOnlyDictionary<string, Texture2D> portraits = new Dictionary<string, Texture2D>();
    Action<IReadOnlyList<string>>? continued;
    Action? backed;

    public CrewSelectionView()
    {
        Visible = false;
        Position = new Vector2(18, 18);
        Size = new Vector2(1364, 1014);
        MouseFilter = MouseFilterEnum.Stop;
        var panel = new StyleBoxFlat { BgColor = new Color("081321fa"), BorderColor = new Color("d2a84f") };
        panel.SetBorderWidthAll(3);
        panel.SetCornerRadiusAll(14);
        AddThemeStyleboxOverride("panel", panel);

        var margin = new MarginContainer();
        foreach (var edge in new[] { "left", "right", "top", "bottom" }) margin.AddThemeConstantOverride("margin_" + edge, 24);
        AddChild(margin);
        content.AddThemeConstantOverride("separation", 12);
        margin.AddChild(content);
    }

    public void Open(
        CampaignState value,
        int mission,
        IEnumerable<string> selected,
        IReadOnlyDictionary<string, Texture2D> portraitTextures,
        Action<IReadOnlyList<string>> onContinue,
        Action? onBack)
        => Open(value, mission, selected, MissionRunMode.Campaign, portraitTextures, onContinue, onBack);

    public void Open(
        CampaignState value,
        int mission,
        IEnumerable<string> selected,
        MissionRunMode mode,
        IReadOnlyDictionary<string, Texture2D> portraitTextures,
        Action<IReadOnlyList<string>> onContinue,
        Action? onBack)
    {
        campaign = value;
        state = new CrewSelectionState(value, mission, selected, mode);
        portraits = portraitTextures;
        continued = onContinue;
        backed = onBack;
        Visible = true;
        Rebuild();
    }

    void Rebuild()
    {
        foreach (var child in content.GetChildren()) { content.RemoveChild(child); child.QueueFree(); }

        var title = new Label { Text = CrewSelectionPresentation.TitleFor(state.Mode), HorizontalAlignment = HorizontalAlignment.Center };
        title.AddThemeFontSizeOverride("font_size", 34);
        title.AddThemeColorOverride("font_color", new Color("f3c979"));
        content.AddChild(title);
        var instruction = new Label { Text = $"MISSION {state.Mission} · {CrewSelectionPresentation.InstructionFor(state.Mode)}", HorizontalAlignment = HorizontalAlignment.Center };
        instruction.AddThemeFontSizeOverride("font_size", 20);
        content.AddChild(instruction);

        var grid = new GridContainer { Columns = 3, SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        grid.AddThemeConstantOverride("h_separation", 12);
        grid.AddThemeConstantOverride("v_separation", 12);
        content.AddChild(grid);
        foreach (var id in campaign.AvailableCrew(state.Mission, state.Mode)) grid.AddChild(BuildCard(id));

        var message = new Label
        {
            Text = CrewSelectionPresentation.SelectionMessage(state),
            HorizontalAlignment = HorizontalAlignment.Center,
            CustomMinimumSize = new Vector2(0, 28)
        };
        message.AddThemeColorOverride("font_color", new Color("f3c979"));
        content.AddChild(message);

        var footer = new HBoxContainer();
        footer.AddThemeConstantOverride("separation", 14);
        content.AddChild(footer);
        var back = new Button { Text = "Back", Visible = backed is not null, CustomMinimumSize = new Vector2(170, 54) };
        back.Pressed += () => { Visible = false; backed?.Invoke(); };
        footer.AddChild(back);
        var count = new Label { Text = state.CountLabel, SizeFlagsHorizontal = SizeFlags.ExpandFill, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
        count.AddThemeFontSizeOverride("font_size", 22);
        count.AddThemeColorOverride("font_color", new Color("75f3ff"));
        footer.AddChild(count);
        var next = new Button { Text = CrewSelectionPresentation.ContinueLabel, Disabled = !state.CanContinue, CustomMinimumSize = new Vector2(270, 54) };
        next.Pressed += () => { if (!state.CanContinue) return; Visible = false; continued?.Invoke(state.SelectedIds.ToList()); };
        footer.AddChild(next);
    }

    Button BuildCard(string id)
    {
        var profile = CrewCatalog.Profile(id);
        var progress = campaign.Progress(id);
        var selected = state.IsSelected(id);
        var required = state.IsRequired(id);
        var card = new Button
        {
            ToggleMode = true,
            ButtonPressed = selected,
            Disabled = !state.CanSelect(id),
            CustomMinimumSize = new Vector2(420, 350),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            MouseDefaultCursorShape = required ? CursorShape.Arrow : CursorShape.PointingHand
        };
        ApplyCardStyle(card, selected, required);
        card.Pressed += () => { if (state.Toggle(id)) Rebuild(); };

        var column = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        column.AddThemeConstantOverride("separation", 4);
        card.AddChild(column);
        var portrait = new TextureRect
        {
            Texture = portraits.GetValueOrDefault(id),
            CustomMinimumSize = new Vector2(0, 188),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore
        };
        column.AddChild(portrait);
        var name = new Label { Text = profile.Name, HorizontalAlignment = HorizontalAlignment.Center, MouseFilter = MouseFilterEnum.Ignore };
        name.AddThemeFontSizeOverride("font_size", 20);
        name.AddThemeColorOverride("font_color", new Color("ffffff"));
        column.AddChild(name);
        column.AddChild(CardLabel($"{profile.ClassName} · Level {progress.Level}", 16));
        column.AddChild(CardLabel($"Health {profile.Health + progress.HealthBonus}   Armor {profile.Armor + progress.ArmorBonus}   Movement {profile.Movement + progress.MovementBonus}   Attack +{progress.AttackBonus}", 14));
        var summary = CardLabel(profile.RoleSummary, 14);
        summary.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        summary.CustomMinimumSize = new Vector2(0, 42);
        column.AddChild(summary);
        var status = CardLabel(CrewSelectionPresentation.CardStatus(selected, required), 17);
        status.AddThemeColorOverride("font_color", required ? new Color("f3c979") : new Color("75f3ff"));
        column.AddChild(status);
        return card;
    }

    static Label CardLabel(string text, int size)
    {
        var label = new Label { Text = text, HorizontalAlignment = HorizontalAlignment.Center, MouseFilter = MouseFilterEnum.Ignore };
        label.AddThemeFontSizeOverride("font_size", size);
        return label;
    }

    static void ApplyCardStyle(Button card, bool selected, bool required)
    {
        StyleBoxFlat Style(Color background, Color border, int width)
        {
            var style = new StyleBoxFlat { BgColor = background, BorderColor = border };
            style.SetBorderWidthAll(width);
            style.SetCornerRadiusAll(10);
            style.ContentMarginLeft = style.ContentMarginRight = 12;
            style.ContentMarginTop = style.ContentMarginBottom = 10;
            return style;
        }
        var border = required ? new Color("f3c979") : selected ? new Color("75f3ff") : new Color("58708e");
        var background = selected || required ? new Color("123e52") : new Color("132238");
        card.AddThemeStyleboxOverride("normal", Style(background, border, selected || required ? 4 : 2));
        card.AddThemeStyleboxOverride("hover", Style(new Color("1b4c62"), new Color("d8fcff"), 4));
        card.AddThemeStyleboxOverride("pressed", Style(new Color("0c3746"), new Color("ffffff"), 4));
        card.AddThemeStyleboxOverride("hover_pressed", Style(new Color("1b4c62"), new Color("ffffff"), 4));
        card.AddThemeStyleboxOverride("disabled", Style(background, border, selected || required ? 4 : 2));
    }
}
