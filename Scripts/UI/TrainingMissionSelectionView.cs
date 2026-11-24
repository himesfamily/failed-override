using FailedOverride.Campaign;
using Godot;

namespace FailedOverride.UI;

public partial class TrainingMissionSelectionView : PanelContainer
{
    readonly VBoxContainer content = new();
    TrainingMissionSelectionState state = null!;
    Action<int>? selected;
    Action? cancelled;

    public TrainingMissionSelectionView()
    {
        Visible = false;
        Position = new Vector2(155, 95);
        Size = new Vector2(1090, 880);
        MouseFilter = MouseFilterEnum.Stop;
        var panel = new StyleBoxFlat { BgColor = new Color("081321fa"), BorderColor = new Color("d2a84f") };
        panel.SetBorderWidthAll(3);
        panel.SetCornerRadiusAll(14);
        AddThemeStyleboxOverride("panel", panel);
        var margin = new MarginContainer();
        foreach (var edge in new[] { "left", "right", "top", "bottom" }) margin.AddThemeConstantOverride("margin_" + edge, 30);
        AddChild(margin);
        content.AddThemeConstantOverride("separation", 18);
        margin.AddChild(content);
    }

    public void Open(CampaignState campaign, Action<int> onSelected, Action onCancelled)
    {
        state = new TrainingMissionSelectionState(campaign);
        selected = onSelected;
        cancelled = onCancelled;
        Visible = true;
        Rebuild();
    }

    void Rebuild()
    {
        foreach (var child in content.GetChildren()) { content.RemoveChild(child); child.QueueFree(); }
        var title = new Label { Text = TrainingMissionSelectionPresentation.Title, HorizontalAlignment = HorizontalAlignment.Center };
        title.AddThemeFontSizeOverride("font_size", 36);
        title.AddThemeColorOverride("font_color", new Color("f3c979"));
        content.AddChild(title);
        var explanation = new Label
        {
            Text = TrainingMissionSelectionPresentation.Explanation,
            HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            CustomMinimumSize = new Vector2(0, 70)
        };
        explanation.AddThemeFontSizeOverride("font_size", 19);
        explanation.AddThemeColorOverride("font_color", new Color("d6e6f7"));
        content.AddChild(explanation);
        if (state.Missions.Count == 0)
        {
            var empty = new Label { Text = TrainingMissionSelectionPresentation.EmptyMessage, HorizontalAlignment = HorizontalAlignment.Center, SizeFlagsVertical = SizeFlags.ExpandFill, VerticalAlignment = VerticalAlignment.Center };
            empty.AddThemeFontSizeOverride("font_size", 22);
            content.AddChild(empty);
        }
        else
        {
            foreach (var mission in state.Missions) content.AddChild(MissionButton(mission));
        }
        var back = new Button { Text = "Back", CustomMinimumSize = new Vector2(0, 56) };
        back.Pressed += () => { Visible = false; cancelled?.Invoke(); };
        content.AddChild(back);
    }

    Button MissionButton(int mission)
    {
        var button = new Button { CustomMinimumSize = new Vector2(0, 150), SizeFlagsHorizontal = SizeFlags.ExpandFill };
        var style = new StyleBoxFlat { BgColor = new Color("132238"), BorderColor = new Color("55d7e8") };
        style.SetBorderWidthAll(3);
        style.SetCornerRadiusAll(10);
        button.AddThemeStyleboxOverride("normal", style);
        var column = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        button.AddChild(column);
        var heading = new Label { Text = TrainingMissionSelectionPresentation.MissionTitle(mission), HorizontalAlignment = HorizontalAlignment.Center, MouseFilter = MouseFilterEnum.Ignore };
        heading.AddThemeFontSizeOverride("font_size", 24);
        heading.AddThemeColorOverride("font_color", new Color("f3c979"));
        column.AddChild(heading);
        var objective = new Label { Text = TrainingMissionSelectionPresentation.Objective(mission), HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart, MouseFilter = MouseFilterEnum.Ignore };
        objective.AddThemeFontSizeOverride("font_size", 18);
        column.AddChild(objective);
        button.Pressed += () =>
        {
            if (!state.Select(mission)) return;
            Visible = false;
            selected?.Invoke(mission);
        };
        return button;
    }
}
