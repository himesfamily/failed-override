using Godot;

namespace FailedOverride.UI;

public partial class DebriefingCinematic : Control
{
    IReadOnlyList<MissionTwoIntroBeat> beats = [];
    string sceneTitle = "";
    string transitionTitle = "";
    string transitionKicker = "";
    string finishPrompt = "";
    string skipText = "Skip Cinematic";
    TextureRect hologram = null!;
    BriefingSpeakerFocusView focus = null!;
    Label speaker = null!;
    Label dialogue = null!;
    Label progress = null!;
    Control transitionLayer = null!;
    int index;
    bool showingTransition;

    public event Action? Finished;

    public void Configure(IReadOnlyList<MissionTwoIntroBeat> script, string scene, string skipText, string kicker = "", string transition = "", string prompt = "")
    {
        beats = script;
        sceneTitle = scene;
        transitionKicker = kicker;
        transitionTitle = transition;
        finishPrompt = prompt;
        this.skipText = skipText;
    }

    public override void _Ready()
    {
        Position = Vector2.Zero;
        Size = new Vector2(1400, 1070);
        ZIndex = 107;
        Visible = false;
        MouseFilter = MouseFilterEnum.Stop;

        AddChild(new ColorRect { Position = Vector2.Zero, Size = Size, Color = new Color(.005f, .01f, .025f), MouseFilter = MouseFilterEnum.Ignore });
        var frame = BriefingSpeakerFocusLayout.BackgroundFrame;
        AddChild(new TextureRect { Texture = GD.Load<Texture2D>("res://Assets/Cinematic/aurora-debriefing-room.png"), Position = new Vector2(frame.X, frame.Y), Size = new Vector2(frame.Width, frame.Height), ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.Scale, MouseFilter = MouseFilterEnum.Ignore });
        AddChild(new ColorRect { Position = new Vector2(frame.X, frame.Y), Size = new Vector2(frame.Width, frame.Height), Color = new Color(.01f, .02f, .05f, .48f), MouseFilter = MouseFilterEnum.Ignore });
        focus = new BriefingSpeakerFocusView();
        AddChild(focus);

        var enemyAtlas = GD.Load<Texture2D>("res://Assets/Atlases/enemies.png");
        var ompokTexture = new AtlasTexture { Atlas = enemyAtlas, Region = new Rect2(1024, 0, 512, 512) };
        hologram = new TextureRect { Texture = ompokTexture, Position = new Vector2(520, 125), Size = new Vector2(360, 360), ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, Modulate = new Color(.25f, .9f, 1f, .78f), MouseFilter = MouseFilterEnum.Ignore };
        AddChild(hologram);

        var advance = new Button { Position = Vector2.Zero, Size = Size, Flat = true, FocusMode = FocusModeEnum.None, MouseDefaultCursorShape = CursorShape.PointingHand };
        advance.Pressed += Advance;
        AddChild(advance);

        var scene = new Label { Text = sceneTitle, Position = new Vector2(48, 34), Size = new Vector2(1000, 50), MouseFilter = MouseFilterEnum.Ignore };
        scene.AddThemeFontSizeOverride("font_size", 25);
        scene.AddThemeColorOverride("font_color", new Color("f3c979"));
        scene.AddThemeColorOverride("font_shadow_color", Colors.Black);
        scene.AddThemeConstantOverride("shadow_offset_x", 2);
        scene.AddThemeConstantOverride("shadow_offset_y", 2);
        AddChild(scene);

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
        dialogue.AddThemeFontSizeOverride("font_size", 21);
        column.AddChild(dialogue);
        progress = new Label { HorizontalAlignment = HorizontalAlignment.Right, MouseFilter = MouseFilterEnum.Ignore };
        progress.AddThemeColorOverride("font_color", new Color("9fb4ca"));
        column.AddChild(progress);

        transitionLayer = BuildTransition();
        AddChild(transitionLayer);
        var skip = new Button { Text = skipText, Position = new Vector2(1160, 28), Size = new Vector2(190, 48), ZIndex = 6 };
        skip.Pressed += Finish;
        AddChild(skip);
    }

    Control BuildTransition()
    {
        var layer = new Control { Position = Vector2.Zero, Size = Size, ZIndex = 5, Visible = false, MouseFilter = MouseFilterEnum.Ignore };
        layer.AddChild(new ColorRect { Position = Vector2.Zero, Size = Size, Color = new Color(.005f, .01f, .025f, .98f), MouseFilter = MouseFilterEnum.Ignore });
        var kicker = new Label { Text = transitionKicker, Position = new Vector2(260, 330), Size = new Vector2(880, 55), HorizontalAlignment = HorizontalAlignment.Center, MouseFilter = MouseFilterEnum.Ignore };
        kicker.AddThemeFontSizeOverride("font_size", 23); kicker.AddThemeColorOverride("font_color", new Color("ef596f")); layer.AddChild(kicker);
        var title = new Label { Text = transitionTitle, Position = new Vector2(180, 430), Size = new Vector2(1040, 170), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, MouseFilter = MouseFilterEnum.Ignore };
        title.AddThemeFontSizeOverride("font_size", 43); title.AddThemeColorOverride("font_color", new Color("f3c979")); layer.AddChild(title);
        var prompt = new Label { Text = finishPrompt, Position = new Vector2(350, 700), Size = new Vector2(700, 45), HorizontalAlignment = HorizontalAlignment.Center, MouseFilter = MouseFilterEnum.Ignore };
        prompt.AddThemeFontSizeOverride("font_size", 18); prompt.AddThemeColorOverride("font_color", new Color("c8d6e5")); layer.AddChild(prompt);
        return layer;
    }

    public void Play() { index = 0; showingTransition = false; transitionLayer.Visible = false; focus.Clear(hologram); Visible = true; ShowBeat(); }
    void Advance()
    {
        if (!Visible) return;
        if (showingTransition) { Finish(); return; }
        if (++index < beats.Count) ShowBeat();
        else if (!string.IsNullOrEmpty(transitionTitle)) ShowTransition();
        else Finish();
    }
    void ShowBeat() { var beat = beats[index]; speaker.Text = beat.Speaker; dialogue.Text = beat.Dialogue; hologram.Visible = beat.Portrait == "ompok"; focus.ShowSpeaker(beat.Portrait, hologram); progress.Text = $"CLICK TO CONTINUE   •   {index + 1}/{beats.Count}"; }
    void ShowTransition() { showingTransition = true; focus.Clear(hologram); hologram.Visible = false; transitionLayer.Visible = true; transitionLayer.Modulate = new Color(1, 1, 1, 0); CreateTween().TweenProperty(transitionLayer, "modulate:a", 1f, .55f); }
    void Finish() { if (!Visible) return; focus.Clear(hologram); Visible = false; Finished?.Invoke(); }
    public override void _UnhandledInput(InputEvent e) { if (!Visible || e is not InputEventKey key || !key.Pressed || key.Echo) return; if (key.Keycode == Key.Escape) Finish(); else if (key.Keycode is Key.Space or Key.Enter) Advance(); GetViewport().SetInputAsHandled(); }
}
