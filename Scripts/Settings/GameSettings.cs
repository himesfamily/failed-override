using Godot;
using System.Text.Json;

namespace FailedOverride.Settings;

public sealed class GameSettingsData
{
    public float MasterVolume { get; set; } = 0.8f;
    public float MusicVolume { get; set; } = 0.7f;
    public float EffectsVolume { get; set; } = 0.8f;
    public string DisplayMode { get; set; } = "Standard";
    public bool Tutorial { get; set; } = true;
}

public static class GameSettingsStorage
{
    const string Path = "user://failed_override_settings.json";

    public static GameSettingsData Load()
    {
        if (!Godot.FileAccess.FileExists(Path)) return new();
        using var file = Godot.FileAccess.Open(Path, Godot.FileAccess.ModeFlags.Read);
        try { return JsonSerializer.Deserialize<GameSettingsData>(file.GetAsText()) ?? new(); }
        catch { return new(); }
    }

    public static void Save(GameSettingsData settings)
    {
        using var file = Godot.FileAccess.Open(Path, Godot.FileAccess.ModeFlags.Write);
        file.StoreString(JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static void Apply(GameSettingsData settings)
    {
        EnsureBus("Music");
        EnsureBus("Effects");
        SetVolume("Master", settings.MasterVolume);
        SetVolume("Music", settings.MusicVolume);
        SetVolume("Effects", settings.EffectsVolume);
        ApplyDisplay(settings.DisplayMode);
    }

    public static void ApplyAudio(GameSettingsData settings)
    {
        EnsureBus("Music");
        EnsureBus("Effects");
        SetVolume("Master", settings.MasterVolume);
        SetVolume("Music", settings.MusicVolume);
        SetVolume("Effects", settings.EffectsVolume);
    }

    public static void ApplyDisplay(string mode)
    {
        if (mode == "Fullscreen")
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
            return;
        }

        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        var size = mode switch
        {
            "Compact" => new Vector2I(1120, 856),
            "Large" => new Vector2I(1680, 1284),
            _ => new Vector2I(1400, 1070)
        };
        DisplayServer.WindowSetSize(size);
        var screen = DisplayServer.WindowGetCurrentScreen();
        var screenSize = DisplayServer.ScreenGetSize(screen);
        DisplayServer.WindowSetPosition((screenSize - size) / 2);
    }

    static void EnsureBus(string name)
    {
        if (AudioServer.GetBusIndex(name) >= 0) return;
        AudioServer.AddBus();
        AudioServer.SetBusName(AudioServer.BusCount - 1, name);
    }

    static void SetVolume(string bus, float linear)
    {
        var index = AudioServer.GetBusIndex(bus);
        if (index < 0) return;
        AudioServer.SetBusVolumeDb(index, Mathf.LinearToDb(Mathf.Max(0.001f, linear)));
        AudioServer.SetBusMute(index, linear <= 0.001f);
    }
}
