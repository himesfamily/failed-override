using FailedOverride.Battle;
using FailedOverride.Campaign;
using FailedOverride.Units;
using Godot;

namespace FailedOverride.UI;

public partial class CharacterProgressCard : PanelContainer
{
    readonly ProgressionPresentationState presentation = new();
    readonly TextureRect portrait = new();
    readonly Label title = new();
    readonly Label stats = new();
    readonly Label path = new();
    readonly ProgressBar xpBar = new();
    readonly Label xpText = new();
    readonly Label gain = new();
    readonly PanelContainer levelUpPanel = new();
    readonly Label levelUpText = new();
    Unit? unit;

    public bool PresentationActive => presentation.Active;
    public bool ShowingLevelUp => presentation.ShowingLevelUp;

    public CharacterProgressCard()
    {
        CustomMinimumSize = new(0, 188);
        var margin = new MarginContainer();
        foreach (var edge in new[] { "left", "right", "top", "bottom" }) margin.AddThemeConstantOverride("margin_" + edge, 10);
        AddChild(margin);
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 12);
        margin.AddChild(row);
        portrait.CustomMinimumSize = new(112, 150);
        portrait.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        portrait.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        row.AddChild(portrait);
        var details = new VBoxContainer();
        details.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        row.AddChild(details);
        title.AddThemeFontSizeOverride("font_size", 20);
        title.AddThemeColorOverride("font_color", new Color("f3c979"));
        details.AddChild(title);
        details.AddChild(stats);
        details.AddChild(path);
        xpBar.MinValue = 0;
        xpBar.MaxValue = 100;
        xpBar.ShowPercentage = false;
        xpBar.CustomMinimumSize = new(0, 20);
        details.AddChild(xpBar);
        details.AddChild(xpText);
        gain.AddThemeColorOverride("font_color", new Color("f3c979"));
        gain.AddThemeFontSizeOverride("font_size", 17);
        details.AddChild(gain);
        levelUpPanel.Visible = false;
        levelUpPanel.MouseFilter = MouseFilterEnum.Stop;
        levelUpPanel.MouseDefaultCursorShape = CursorShape.PointingHand;
        levelUpPanel.GuiInput += OnLevelUpInput;
        var style = new StyleBoxFlat { BgColor = new Color("17263d"), BorderColor = new Color("f3c979") };
        style.SetBorderWidthAll(3);
        style.SetCornerRadiusAll(8);
        levelUpPanel.AddThemeStyleboxOverride("panel", style);
        levelUpText.HorizontalAlignment = HorizontalAlignment.Center;
        levelUpText.VerticalAlignment = VerticalAlignment.Center;
        levelUpText.CustomMinimumSize = new(0, 70);
        levelUpText.AddThemeFontSizeOverride("font_size", 22);
        levelUpPanel.AddChild(levelUpText);
        details.AddChild(levelUpPanel);
    }

    public void SetExpanded(bool expanded) => CustomMinimumSize = new(0, expanded ? 260 : 188);

    public void SetCharacter(Unit? value, CharacterProgression? progress, Texture2D? texture)
    {
        unit = value;
        Visible = value != null;
        if (value == null) return;
        portrait.Texture = texture;
        title.Text = $"{value.Name}  ·  {(value.Bear ? "Bear" : value.Role)}  ·  Level {value.Level}";
        var status = value.Captured ? "Captured" : value.SkippedByStun ? "Stunned · turn skipped" : value.Acted && value.Moved ? "Turn complete" : value.Acted ? "Action used · movement available" : value.Moved ? "Moved · action available" : "Move and action available";
        stats.Text = $"Health {value.Hp}/{value.MaxHp}     Armor {value.Armor}\nMovement {value.Move}     Attack +{value.AttackBonus}\n{status}";
        path.Text = $"Training Path: {PathName(progress?.TrainingPath ?? TrainingPath.Unassigned)}";
        if (!presentation.Active)
        {
            xpBar.Value = value.Xp;
            xpText.Text = $"{value.Xp} / 100 XP";
            gain.Text = "";
        }
    }

    public void Present(XpAwardResult result)
    {
        presentation.Begin(result);
        RefreshPresentation();
    }

    public void Advance(double delta)
    {
        if (!presentation.Active) return;
        presentation.Advance(delta);
        RefreshPresentation();
    }

    public void DismissLevelUp()
    {
        presentation.DismissLevelUp();
        RefreshPresentation();
    }

    void RefreshPresentation()
    {
        xpBar.Value = presentation.DisplayedXp;
        xpText.Text = $"{presentation.DisplayedXp} / 100 XP";
        gain.Text = presentation.ShowingLevelUp ? "" : $"{presentation.GainLabel}  ·  {presentation.Reason}";
        levelUpPanel.Visible = presentation.ShowingLevelUp;
        levelUpText.Text = presentation.ShowingLevelUp
            ? $"{presentation.LevelUpTitle}\n{presentation.LevelUpDetail}\nClick to continue"
            : "";
        if (!presentation.Active && unit != null)
        {
            xpBar.Value = unit.Xp;
            xpText.Text = $"{unit.Xp} / 100 XP";
            gain.Text = "";
        }
    }

    void OnLevelUpInput(InputEvent input)
    {
        if (input is not InputEventMouseButton button || !button.Pressed) return;
        DismissLevelUp();
        AcceptEvent();
    }

    static string PathName(TrainingPath value) => value == TrainingPath.Unassigned ? "Not selected" : value.ToString();
}
