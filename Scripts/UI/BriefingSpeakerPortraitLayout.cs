namespace FailedOverride.UI;

public enum SpeakerPortraitSide { Left, Right }

public sealed record BriefingSpeakerPortrait(string Resource, SpeakerPortraitSide Side);

public static class BriefingSpeakerPortraitLayout
{
    static readonly IReadOnlyDictionary<string, BriefingSpeakerPortrait> portraits =
        new Dictionary<string, BriefingSpeakerPortrait>(StringComparer.Ordinal)
        {
            ["issac"] = new("res://Assets/Portraits/issac-wynter-transparent.png", SpeakerPortraitSide.Left),
            ["jeane"] = new("res://Assets/Portraits/jeane-gerson-transparent.png", SpeakerPortraitSide.Right),
            ["nance"] = new("res://Assets/Portraits/nance-parker-transparent.png", SpeakerPortraitSide.Left),
            ["xander"] = new("res://Assets/Portraits/xander-mcraven-transparent.png", SpeakerPortraitSide.Right),
            ["akiko"] = new("res://Assets/Portraits/akiko-ikeda-transparent.png", SpeakerPortraitSide.Left),
            ["chrandy"] = new("res://Assets/Portraits/dr-chrandy-perra.png", SpeakerPortraitSide.Right),
        };

    public static IEnumerable<string> AllHumanKeys => portraits.Keys;
    public static BriefingSpeakerPortrait? For(string portraitKey) => portraits.GetValueOrDefault(portraitKey);

    public static MissionTwoIntroRect? DisplayFrameFor(string portraitKey)
    {
        var portrait = For(portraitKey);
        if (portrait is null) return null;
        return portrait.Side == SpeakerPortraitSide.Left
            ? new MissionTwoIntroRect(45, 96, 450, 624)
            : new MissionTwoIntroRect(905, 96, 450, 624);
    }
}
