using Godot;

namespace FailedOverride.UI;

public partial class MissionTwoIntro : Control
{
    readonly Dictionary<string, Texture2D> portraits = [];
    TextureRect portrait = null!;
    Label speaker = null!;
    Label dialogue = null!;
    Label progress = null!;
    Control transitionLayer = null!;
    int index;
    bool showingFinalTransition;

    public event Action? Finished;

    public override void _Ready()
    {
        Position = Vector2.Zero;
        Size = new Vector2(1400, 1070);
        ZIndex = 106;
        Visible = false;
        MouseFilter = MouseFilterEnum.Stop;

        portraits["jeane"] = GD.Load<Texture2D>("res://Assets/Portraits/jeane-gerson-transparent.png");
        portraits["issac"] = GD.Load<Texture2D>("res://Assets/Portraits/issac-wynter-transparent.png");
        portraits["nance"] = GD.Load<Texture2D>("res://Assets/Portraits/nance-parker-transparent.png");
        portraits["xander"] = GD.Load<Texture2D>("res://Assets/Portraits/xander-mcraven-transparent.png");

        AddChild(new ColorRect { Position = Vector2.Zero, Size = Size, Color = new Color(.005f, .01f, .025f), MouseFilter = MouseFilterEnum.Ignore });
        AddChild(new TextureRect
        {
            Texture = GD.Load<Texture2D>("res://Assets/Cinematic/aurora-debriefing-room.png"),
            Position = Vector2.Zero,
            Size = Size,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = MissionTwoIntroScript.PreserveFullComposition ? TextureRect.StretchModeEnum.KeepAspectCentered : TextureRect.StretchModeEnum.KeepAspectCovered,
            MouseFilter = MouseFilterEnum.Ignore,
        });
        AddChild(new ColorRect { Position = Vector2.Zero, Size = Size, Color = new Color(.01f, .02f, .05f, .2f), MouseFilter = MouseFilterEnum.Ignore });

        var advance = new Button { Position = Vector2.Zero, Size = Size, Flat = true, FocusMode = FocusModeEnum.None, MouseDefaultCursorShape = CursorShape.PointingHand };
        advance.Pressed += Advance;
        AddChild(advance);

        var scene = new Label { Text = MissionTwoIntroScript.Scene, Position = new Vector2(48, 34), Size = new Vector2(1000, 50), MouseFilter = MouseFilterEnum.Ignore };
        scene.AddThemeFontSizeOverride("font_size", 25);
        scene.AddThemeColorOverride("font_color", new Color("f3c979"));
        scene.AddThemeColorOverride("font_shadow_color", Colors.Black);
        scene.AddThemeConstantOverride("shadow_offset_x", 2);
        scene.AddThemeConstantOverride("shadow_offset_y", 2);
        AddChild(scene);

        var portraitLayout = MissionTwoIntroScript.ForegroundPortrait;
        portrait = new TextureRect { Position = new Vector2(portraitLayout.X, portraitLayout.Y), Size = new Vector2(portraitLayout.Width, portraitLayout.Height), ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, MouseFilter = MouseFilterEnum.Ignore };
        AddChild(portrait);

        var panel = new PanelContainer { Position = new Vector2(70, 730), Size = new Vector2(1260, 300), MouseFilter = MouseFilterEnum.Ignore };
        panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = new Color(.025f, .055f, .105f, .96f), BorderColor = new Color("55d7e8"), BorderWidthLeft = 3, BorderWidthTop = 3, BorderWidthRight = 3, BorderWidthBottom = 3, CornerRadiusTopLeft = 14, CornerRadiusTopRight = 14, CornerRadiusBottomLeft = 14, CornerRadiusBottomRight = 14 });
        AddChild(panel);
        var margin = new MarginContainer { MouseFilter = MouseFilterEnum.Ignore };
        foreach (var edge in new[] { "left", "right", "top", "bottom" }) margin.AddThemeConstantOverride("margin_" + edge, 22);
        panel.AddChild(margin);
        var column = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        column.AddThemeConstantOverride("separation", 8);
        margin.AddChild(column);
        speaker = new Label { CustomMinimumSize = new Vector2(0, 35), MouseFilter = MouseFilterEnum.Ignore };
        speaker.AddThemeFontSizeOverride("font_size", 24);
        speaker.AddThemeColorOverride("font_color", new Color("f3c979"));
        column.AddChild(speaker);
        dialogue = new Label { CustomMinimumSize = new Vector2(0, 165), AutowrapMode = TextServer.AutowrapMode.WordSmart, VerticalAlignment = VerticalAlignment.Center, MouseFilter = MouseFilterEnum.Ignore };
        dialogue.AddThemeFontSizeOverride("font_size", 22);
        column.AddChild(dialogue);
        progress = new Label { HorizontalAlignment = HorizontalAlignment.Right, MouseFilter = MouseFilterEnum.Ignore };
        progress.AddThemeColorOverride("font_color", new Color("9fb4ca"));
        column.AddChild(progress);

        transitionLayer = BuildFinalTransition();
        AddChild(transitionLayer);

        var skip = new Button { Text = "Skip Cinematic", Position = new Vector2(1160, 28), Size = new Vector2(190, 48), ZIndex = 5 };
        skip.Pressed += Finish;
        AddChild(skip);
    }

    Control BuildFinalTransition()
    {
        var layer = new Control { Position = Vector2.Zero, Size = Size, ZIndex = 4, Visible = false, MouseFilter = MouseFilterEnum.Ignore };
        layer.AddChild(new ColorRect { Position = Vector2.Zero, Size = Size, Color = new Color(.005f, .01f, .025f, .98f), MouseFilter = MouseFilterEnum.Ignore });
        var kicker = new Label { Text = MissionTwoIntroScript.TransitionKicker, Position = new Vector2(260, 330), Size = new Vector2(880, 55), HorizontalAlignment = HorizontalAlignment.Center, MouseFilter = MouseFilterEnum.Ignore };
        kicker.AddThemeFontSizeOverride("font_size", 23);
        kicker.AddThemeColorOverride("font_color", new Color("ef596f"));
        layer.AddChild(kicker);
        layer.AddChild(new ColorRect { Position = new Vector2(430, 414), Size = new Vector2(540, 3), Color = new Color("ef596f"), MouseFilter = MouseFilterEnum.Ignore });
        var title = new Label { Text = MissionTwoIntroScript.TransitionTitle, Position = new Vector2(180, 450), Size = new Vector2(1040, 150), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart, MouseFilter = MouseFilterEnum.Ignore };
        title.AddThemeFontSizeOverride("font_size", 43);
        title.AddThemeColorOverride("font_color", new Color("f3c979"));
        layer.AddChild(title);
        var prompt = new Label { Text = "CLICK TO BEGIN MISSION 2", Position = new Vector2(350, 700), Size = new Vector2(700, 45), HorizontalAlignment = HorizontalAlignment.Center, MouseFilter = MouseFilterEnum.Ignore };
        prompt.AddThemeFontSizeOverride("font_size", 18);
        prompt.AddThemeColorOverride("font_color", new Color("c8d6e5"));
        layer.AddChild(prompt);
        return layer;
    }

    public void Play()
    {
        index = 0;
        showingFinalTransition = false;
        transitionLayer.Visible = false;
        Visible = true;
        ShowBeat();
    }

    void Advance()
    {
        if (!Visible) return;
        if (showingFinalTransition) { Finish(); return; }
        index++;
        if (index >= MissionTwoIntroScript.Beats.Count) ShowFinalTransition();
        else ShowBeat();
    }

    void ShowFinalTransition()
    {
        showingFinalTransition = true;
        portrait.Visible = false;
        transitionLayer.Visible = true;
        transitionLayer.Modulate = new Color(1, 1, 1, 0);
        CreateTween().TweenProperty(transitionLayer, "modulate:a", 1f, .55f);
    }

    void ShowBeat()
    {
        var beat = MissionTwoIntroScript.Beats[index];
        speaker.Text = beat.Speaker;
        dialogue.Text = beat.Dialogue;
        portrait.Texture = portraits[beat.Portrait];
        portrait.Visible = true;
        portrait.Position = new Vector2(MissionTwoIntroScript.ForegroundPortrait.X, MissionTwoIntroScript.ForegroundPortrait.Y);
        progress.Text = $"CLICK TO CONTINUE   •   {index + 1}/{MissionTwoIntroScript.Beats.Count}";
    }

    void Finish()
    {
        if (!Visible) return;
        Visible = false;
        Finished?.Invoke();
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (!Visible || e is not InputEventKey key || !key.Pressed || key.Echo) return;
        if (key.Keycode == Key.Escape) Finish();
        else if (key.Keycode is Key.Space or Key.Enter) Advance();
        GetViewport().SetInputAsHandled();
    }
}
