using Godot;

namespace FailedOverride.UI;

public partial class OpeningCinematic : Control
{
    sealed record Beat(string Scene, string Background, string Speaker, string Portrait, string Dialogue, bool Right = false);

    readonly Beat[] beats =
    [
        new("ICE MOON OF ANAT · DOCKING STATION", "anat-docking-station", "CAPTAIN JEANE GERSON", "jeane", "Our cargo was unloaded and the new shipment loaded in record time. Thank you, Portmaster Gaxu. We’ll be on our way."),
        new("ICE MOON OF ANAT · DOCKING STATION", "anat-docking-station", "PORTMASTER GAXU", "gaxu", "Just one last item of business, if you please, Captain.", true),
        new("ICE MOON OF ANAT · DOCKING STATION", "anat-docking-station", "CAPTAIN JEANE GERSON", "jeane", "No thank you, Gaxu. We already paid your ‘surplus tax.’ We’re clean and good to go."),
        new("ICE MOON OF ANAT · DOCKING STATION", "anat-docking-station", "PORTMASTER GAXU", "gaxu", "Yes, of course. But I took the liberty of loading one extra item with your cargo bound for Cheromia.", true),
        new("ICE MOON OF ANAT · DOCKING STATION", "anat-docking-station", "CAPTAIN JEANE GERSON", "jeane", "An extra item? What is it—drugs? Forbidden alien tech? You know I don’t play that game, Portmaster."),
        new("ICE MOON OF ANAT · DOCKING STATION", "anat-docking-station", "PORTMASTER GAXU", "gaxu", "Nothing like that. Just a small crate. Carry your foodstuffs to Cheromia and it will be quietly unloaded when you arrive.", true),
        new("ICE MOON OF ANAT · DOCKING STATION", "anat-docking-station", "CAPTAIN JEANE GERSON", "jeane", "And what will this cargo be listed as on my shipping manifest?"),
        new("ICE MOON OF ANAT · DOCKING STATION", "anat-docking-station", "PORTMASTER GAXU", "gaxu", "There is no need to list it. Deliver it unopened and your future surplus taxes will be waived. Do we have an agreement?", true),
        new("ICE MOON OF ANAT · DOCKING STATION", "anat-docking-station", "CAPTAIN JEANE GERSON", "jeane", "It isn’t some naked teenager in cryosleep, is it?"),
        new("ICE MOON OF ANAT · DOCKING STATION", "anat-docking-station", "PORTMASTER GAXU", "gaxu", "No, no. Just don’t open it. And if curiosity gets the better of you, there are ways of knowing if the crate has been tampered with. Believe me—you do not want to do that.", true),
        new("STARSHIP AURORA · BRIDGE", "aurora-bridge", "XANDER MCRAVEN", "xander", "So he expects us to deliver a mystery box, unopened and without questions—and then we never have to bribe him to use his port again?"),
        new("STARSHIP AURORA · BRIDGE", "aurora-bridge", "CAPTAIN JEANE GERSON", "jeane", "That’s what he says.", true),
        new("STARSHIP AURORA · BRIDGE", "aurora-bridge", "NANCE PARKER", "nance", "You know it’s a naked teenager in cryosleep, right?"),
        new("STARSHIP AURORA · BRIDGE", "aurora-bridge", "CAPTAIN JEANE GERSON", "jeane", "I already asked. He says no.", true),
        new("STARSHIP AURORA · BRIDGE", "aurora-bridge", "AKIKO IKEDA", "akiko", "And you believe him? I say we crack it open and take a look."),
        new("STARSHIP AURORA · BRIDGE", "aurora-bridge", "CAPTAIN JEANE GERSON", "jeane", "Can’t do that. If we open it, the deal is off.", true),
        new("STARSHIP AURORA · CARGO BAY", "aurora-cargo-bay", "ISSAC WYNTER", "issac", "We have to carry it, but we can’t open it? That sounds very suspicious."),
        new("STARSHIP AURORA · CARGO BAY", "aurora-cargo-bay", "AKIKO IKEDA", "akiko", "That’s what I think too, but Cap says we leave it sealed.", true),
        new("STARSHIP AURORA · CARGO BAY", "aurora-cargo-bay", "ISSAC WYNTER", "issac", "All right. I’ll leave it be. But there can’t be any harm in scanning it, can there?"),
        new("STARSHIP AURORA · CARGO BAY", "aurora-cargo-bay", "AKIKO IKEDA", "akiko", "That sounds reasonable. Give it a once-over and see what you can find.", true),
        new("STARSHIP AURORA · CARGO BAY", "aurora-cargo-bay", "SYSTEM", "", "The scanner rises to a piercing whine. Issac screams—and the cargo bay falls silent."),
        new("STARSHIP AURORA · CARGO BAY", "aurora-cargo-bay", "ROBOT MINION", "droid", "You have broken the contract. Now you will pay the price."),
    ];

    readonly Dictionary<string, Texture2D> backgrounds = [];
    readonly Dictionary<string, Texture2D> portraits = [];
    TextureRect background = null!;
    TextureRect portrait = null!;
    Label scene = null!;
    Label speaker = null!;
    Label dialogue = null!;
    Label progress = null!;
    Control transitionLayer = null!;
    Label transitionKicker = null!;
    Label transitionTitle = null!;
    bool showingTransition;
    int index;
    AudioStreamPlayer screamPlayer = null!;
    public event Action? Finished;

    public override void _Ready()
    {
        Position = Vector2.Zero;
        Size = new Vector2(1400, 1070);
        ZIndex = 100;
        Visible = false;
        MouseFilter = MouseFilterEnum.Stop;

        backgrounds["anat-docking-station"] = GD.Load<Texture2D>("res://Assets/Cinematic/anat-docking-station.png");
        backgrounds["aurora-bridge"] = GD.Load<Texture2D>("res://Assets/Cinematic/aurora-bridge.png");
        backgrounds["aurora-cargo-bay"] = GD.Load<Texture2D>("res://Assets/Cinematic/aurora-cargo-bay.png");
        portraits["jeane"] = GD.Load<Texture2D>("res://Assets/Portraits/jeane-gerson-transparent.png");
        portraits["gaxu"] = GD.Load<Texture2D>("res://Assets/Portraits/portmaster-gaxu.png");
        portraits["xander"] = GD.Load<Texture2D>("res://Assets/Portraits/xander-mcraven-transparent.png");
        portraits["nance"] = GD.Load<Texture2D>("res://Assets/Portraits/nance-parker-transparent.png");
        portraits["akiko"] = GD.Load<Texture2D>("res://Assets/Portraits/akiko-ikeda-transparent.png");
        portraits["issac"] = GD.Load<Texture2D>("res://Assets/Portraits/issac-wynter-transparent.png");
        var atlas = GD.Load<Texture2D>("res://Assets/Atlases/enemies.png");
        var atlasSize = atlas.GetSize();
        portraits["droid"] = new AtlasTexture { Atlas = atlas, Region = new Rect2(0, 0, atlasSize.X / 3f, atlasSize.Y / 2f) };
        screamPlayer = new AudioStreamPlayer { Bus = "Effects", VolumeDb = 5f, Stream = GD.Load<AudioStream>("res://Assets/Audio/SFX/issac_scream.wav") };
        AddChild(screamPlayer);

        background = new TextureRect { Position = Vector2.Zero, Size = Size, ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered, MouseFilter = MouseFilterEnum.Ignore };
        AddChild(background);
        AddChild(new ColorRect { Position = Vector2.Zero, Size = Size, Color = new Color(0.01f, 0.02f, 0.05f, .22f), MouseFilter = MouseFilterEnum.Ignore });

        var advance = new Button { Position = Vector2.Zero, Size = Size, Flat = true, FocusMode = FocusModeEnum.None, MouseDefaultCursorShape = CursorShape.PointingHand };
        advance.Pressed += Advance;
        AddChild(advance);

        scene = new Label { Position = new Vector2(48, 34), Size = new Vector2(920, 50), MouseFilter = MouseFilterEnum.Ignore };
        scene.AddThemeFontSizeOverride("font_size", 25);
        scene.AddThemeColorOverride("font_color", new Color("f3c979"));
        scene.AddThemeColorOverride("font_shadow_color", Colors.Black);
        scene.AddThemeConstantOverride("shadow_offset_x", 2);
        scene.AddThemeConstantOverride("shadow_offset_y", 2);
        AddChild(scene);

        portrait = new TextureRect { Position = new Vector2(55, 115), Size = new Vector2(430, 625), ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, MouseFilter = MouseFilterEnum.Ignore };
        AddChild(portrait);

        var dialoguePanel = new PanelContainer { Position = new Vector2(70, 755), Size = new Vector2(1260, 250), MouseFilter = MouseFilterEnum.Ignore };
        var panelStyle = new StyleBoxFlat { BgColor = new Color(0.025f, .055f, .105f, .95f), BorderColor = new Color("55d7e8"), BorderWidthLeft = 3, BorderWidthTop = 3, BorderWidthRight = 3, BorderWidthBottom = 3, CornerRadiusTopLeft = 14, CornerRadiusTopRight = 14, CornerRadiusBottomLeft = 14, CornerRadiusBottomRight = 14 };
        dialoguePanel.AddThemeStyleboxOverride("panel", panelStyle);
        AddChild(dialoguePanel);
        var margin = new MarginContainer { MouseFilter = MouseFilterEnum.Ignore };
        foreach (var edge in new[] { "left", "right", "top", "bottom" }) margin.AddThemeConstantOverride("margin_" + edge, 24);
        dialoguePanel.AddChild(margin);
        var column = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        column.AddThemeConstantOverride("separation", 10);
        margin.AddChild(column);
        speaker = new Label { CustomMinimumSize = new Vector2(0, 38), MouseFilter = MouseFilterEnum.Ignore };
        speaker.AddThemeFontSizeOverride("font_size", 25);
        speaker.AddThemeColorOverride("font_color", new Color("f3c979"));
        column.AddChild(speaker);
        dialogue = new Label { CustomMinimumSize = new Vector2(0, 112), AutowrapMode = TextServer.AutowrapMode.WordSmart, VerticalAlignment = VerticalAlignment.Center, MouseFilter = MouseFilterEnum.Ignore };
        dialogue.AddThemeFontSizeOverride("font_size", 24);
        column.AddChild(dialogue);
        progress = new Label { HorizontalAlignment = HorizontalAlignment.Right, MouseFilter = MouseFilterEnum.Ignore };
        progress.AddThemeFontSizeOverride("font_size", 16);
        progress.AddThemeColorOverride("font_color", new Color("9fb4ca"));
        column.AddChild(progress);

        transitionLayer = new Control { Position = Vector2.Zero, Size = Size, ZIndex = 4, Visible = false, MouseFilter = MouseFilterEnum.Ignore };
        AddChild(transitionLayer);
        transitionLayer.AddChild(new ColorRect { Position = Vector2.Zero, Size = Size, Color = new Color(0.005f, 0.01f, 0.025f, .97f), MouseFilter = MouseFilterEnum.Ignore });
        var transitionRule = new ColorRect { Position = new Vector2(430, 414), Size = new Vector2(540, 3), Color = new Color("55d7e8"), MouseFilter = MouseFilterEnum.Ignore };
        transitionLayer.AddChild(transitionRule);
        transitionKicker = new Label { Position = new Vector2(260, 330), Size = new Vector2(880, 55), HorizontalAlignment = HorizontalAlignment.Center, MouseFilter = MouseFilterEnum.Ignore };
        transitionKicker.AddThemeFontSizeOverride("font_size", 22);
        transitionKicker.AddThemeColorOverride("font_color", new Color("9fb4ca"));
        transitionLayer.AddChild(transitionKicker);
        transitionTitle = new Label { Position = new Vector2(180, 450), Size = new Vector2(1040, 150), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart, MouseFilter = MouseFilterEnum.Ignore };
        transitionTitle.AddThemeFontSizeOverride("font_size", 43);
        transitionTitle.AddThemeColorOverride("font_color", new Color("f3c979"));
        transitionTitle.AddThemeColorOverride("font_shadow_color", Colors.Black);
        transitionTitle.AddThemeConstantOverride("shadow_offset_x", 3);
        transitionTitle.AddThemeConstantOverride("shadow_offset_y", 3);
        transitionLayer.AddChild(transitionTitle);
        var transitionPrompt = new Label { Text = "CLICK TO ENTER THE NEXT SCENE", Position = new Vector2(350, 700), Size = new Vector2(700, 45), HorizontalAlignment = HorizontalAlignment.Center, MouseFilter = MouseFilterEnum.Ignore };
        transitionPrompt.AddThemeFontSizeOverride("font_size", 18);
        transitionPrompt.AddThemeColorOverride("font_color", new Color("c8d6e5"));
        transitionLayer.AddChild(transitionPrompt);

        var skip = new Button { Text = "Skip Cinematic", Position = new Vector2(1160, 28), Size = new Vector2(190, 48), ZIndex = 5 };
        skip.Pressed += Finish;
        AddChild(skip);
    }

    public void Play()
    {
        index = 0;
        showingTransition = false;
        transitionLayer.Visible = false;
        Visible = true;
        ShowBeat();
    }

    void Advance()
    {
        if (!Visible) return;
        if (showingTransition)
        {
            showingTransition = false;
            transitionLayer.Visible = false;
            ShowBeat();
            return;
        }
        var next = index + 1;
        if (next >= beats.Length) { Finish(); return; }
        var locationChanged = beats[next].Scene != beats[index].Scene;
        index = next;
        if (locationChanged) ShowTransition(beats[index]);
        else ShowBeat();
    }

    void ShowTransition(Beat nextBeat)
    {
        showingTransition = true;
        background.Texture = backgrounds[nextBeat.Background];
        portrait.Visible = false;
        scene.Text = "";
        transitionLayer.Visible = true;
        transitionKicker.Text = nextBeat.Background == "aurora-bridge" ? "THE AURORA CLEARS ANAT ORBIT" : "LATER · BELOW DECKS";
        transitionTitle.Text = nextBeat.Scene.Replace(" · ", "\n");
        transitionLayer.Modulate = new Color(1, 1, 1, 0);
        CreateTween().TweenProperty(transitionLayer, "modulate:a", 1f, .45f);
    }

    void Finish()
    {
        if (!Visible) return;
        Visible = false;
        Finished?.Invoke();
    }

    void ShowBeat()
    {
        var beat = beats[index];
        background.Texture = backgrounds[beat.Background];
        scene.Text = beat.Scene;
        speaker.Text = beat.Speaker;
        dialogue.Text = beat.Dialogue;
        if (beat.Speaker == "SYSTEM" && !screamPlayer.Playing) screamPlayer.Play();
        portrait.Texture = beat.Portrait != "" ? portraits[beat.Portrait] : null;
        portrait.Visible = portrait.Texture != null;
        portrait.Position = new Vector2(beat.Right ? 915 : 55, 115);
        progress.Text = $"CLICK TO CONTINUE   •   {index + 1}/{beats.Length}";
        Modulate = Colors.White;
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (!Visible || e is not InputEventKey key || !key.Pressed || key.Echo) return;
        if (key.Keycode == Key.Escape) Finish();
        else if (key.Keycode is Key.Space or Key.Enter) Advance();
        GetViewport().SetInputAsHandled();
    }
}
