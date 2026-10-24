using Godot;

namespace FailedOverride.UI;

public partial class MissionOneWrapUp : Control
{
    sealed record Beat(string Speaker, string Portrait, string Dialogue, bool Right = false);
    readonly Beat[] beats =
    [
        new("CAPTAIN JEANE GERSON", "jeane", "What happened? I told you not to touch that thing."),
        new("AKIKO IKEDA", "akiko", "We didn't touch anything. Issac just scanned it.", true),
        new("CAPTAIN JEANE GERSON", "jeane", "Well, that must have been enough to trigger the defenses. How screwed are we?"),
        new("ISSAC WYNTER", "issac", "I tried to override one of the hostile bots when they activated, but they shot me with a stasis ray that left me paralyzed. I wasn't able to move, but my mind was still free to explore their programming. We better get the crew together to talk about this. We are in for some serious pain if we aren't careful.", true),
        new("CAPTAIN JEANE GERSON", "jeane", "It is a little late for ‘careful.’ But let's meet in the debriefing room to fill everyone in on what is happening."),
    ];

    readonly Dictionary<string, Texture2D> portraits = [];
    TextureRect portrait = null!;
    Label speaker = null!;
    Label dialogue = null!;
    Label progress = null!;
    int index;
    public event Action? Finished;

    public override void _Ready()
    {
        Position = Vector2.Zero;
        Size = new Vector2(1400, 1070);
        ZIndex = 105;
        Visible = false;
        MouseFilter = MouseFilterEnum.Stop;

        portraits["jeane"] = GD.Load<Texture2D>("res://Assets/Portraits/jeane-gerson-transparent.png");
        portraits["akiko"] = GD.Load<Texture2D>("res://Assets/Portraits/akiko-ikeda-transparent.png");
        portraits["issac"] = GD.Load<Texture2D>("res://Assets/Portraits/issac-wynter-transparent.png");

        AddChild(new TextureRect { Texture = GD.Load<Texture2D>("res://Assets/Cinematic/aurora-cargo-bay.png"), Position = Vector2.Zero, Size = Size, ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered, MouseFilter = MouseFilterEnum.Ignore });
        AddChild(new ColorRect { Position = Vector2.Zero, Size = Size, Color = new Color(.01f, .02f, .05f, .28f), MouseFilter = MouseFilterEnum.Ignore });
        var advance = new Button { Position = Vector2.Zero, Size = Size, Flat = true, FocusMode = FocusModeEnum.None, MouseDefaultCursorShape = CursorShape.PointingHand };
        advance.Pressed += Advance;
        AddChild(advance);

        var scene = new Label { Text = "MISSION 1 COMPLETE · AURORA CARGO BAY", Position = new Vector2(48, 34), Size = new Vector2(1000, 50), MouseFilter = MouseFilterEnum.Ignore };
        scene.AddThemeFontSizeOverride("font_size", 25);
        scene.AddThemeColorOverride("font_color", new Color("f3c979"));
        scene.AddThemeColorOverride("font_shadow_color", Colors.Black);
        scene.AddThemeConstantOverride("shadow_offset_x", 2);
        scene.AddThemeConstantOverride("shadow_offset_y", 2);
        AddChild(scene);

        portrait = new TextureRect { Position = new Vector2(55, 105), Size = new Vector2(430, 615), ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, MouseFilter = MouseFilterEnum.Ignore };
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
        dialogue.AddThemeFontSizeOverride("font_size", 21);
        column.AddChild(dialogue);
        progress = new Label { HorizontalAlignment = HorizontalAlignment.Right, MouseFilter = MouseFilterEnum.Ignore };
        progress.AddThemeColorOverride("font_color", new Color("9fb4ca"));
        column.AddChild(progress);

        var skip = new Button { Text = "Skip Wrap-Up", Position = new Vector2(1160, 28), Size = new Vector2(190, 48), ZIndex = 5 };
        skip.Pressed += Finish;
        AddChild(skip);
    }

    public void Play() { index = 0; Visible = true; ShowBeat(); }
    void Advance() { if (!Visible) return; if (++index >= beats.Length) Finish(); else ShowBeat(); }
    void Finish() { if (!Visible) return; Visible = false; Finished?.Invoke(); }
    void ShowBeat()
    {
        var beat = beats[index];
        speaker.Text = beat.Speaker;
        dialogue.Text = beat.Dialogue;
        portrait.Texture = portraits[beat.Portrait];
        portrait.Position = new Vector2(beat.Right ? 915 : 55, 105);
        progress.Text = $"CLICK TO CONTINUE   •   {index + 1}/{beats.Length}";
    }
    public override void _UnhandledInput(InputEvent e)
    {
        if (!Visible || e is not InputEventKey key || !key.Pressed || key.Echo) return;
        if (key.Keycode == Key.Escape) Finish(); else if (key.Keycode is Key.Space or Key.Enter) Advance();
        GetViewport().SetInputAsHandled();
    }
}
