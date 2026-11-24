namespace FailedOverride.UI;

public static class BriefingSpeakerFocusLayout
{
    static readonly IReadOnlyDictionary<string, MissionTwoIntroRect> markerRegions =
        new Dictionary<string, MissionTwoIntroRect>(StringComparer.Ordinal)
        {
            ["issac"] = new(245, 210, 190, 245),
            ["jeane"] = new(445, 120, 235, 265),
            ["xander"] = new(680, 120, 230, 270),
            ["nance"] = new(925, 155, 220, 275),
        };

    public static readonly MissionTwoIntroRect BackgroundFrame = new(24, 82, 1352, 761);
    public const string HologramOutlineResource = "res://Assets/Cinematic/SpeakerFocus/ompok-outline.svg";
    public const int FocusZIndex = 2;
    public const float MarkerHaloTopFraction = .08f;

    public static MissionTwoIntroRect? MarkerFor(string portraitKey) =>
        markerRegions.TryGetValue(portraitKey, out var region) ? region : null;

    public static MissionTwoIntroRect? DisplayMarkerFor(string portraitKey)
    {
        var source = MarkerFor(portraitKey);
        if (source is null) return null;
        var scale = BackgroundFrame.Width / 1672f;
        return new(
            BackgroundFrame.X + source.Value.X * scale,
            BackgroundFrame.Y + source.Value.Y * scale,
            source.Value.Width * scale,
            source.Value.Height * scale);
    }

    public static bool IsHologram(string portraitKey) =>
        string.Equals(portraitKey, "ompok", StringComparison.Ordinal);
}
