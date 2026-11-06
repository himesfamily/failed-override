namespace FailedOverride.UI;

public sealed record MissionTwoIntroBeat(string Speaker, string Portrait, string Dialogue, bool Right = false);
public readonly record struct MissionTwoIntroRect(float X, float Y, float Width, float Height)
{
    public bool Intersects(MissionTwoIntroRect other) => X < other.X + other.Width && X + Width > other.X && Y < other.Y + other.Height && Y + Height > other.Y;
}

public static class MissionTwoIntroScript
{
    public const string Scene = "STARSHIP AURORA · DEBRIEFING ROOM";
    public const string TransitionKicker = "INTRUSION ALERT";
    public const string TransitionTitle = "STARSHIP AURORA\nCARGO BAY";
    public const bool PreserveFullComposition = true;
    public static readonly MissionTwoIntroRect ForegroundPortrait = new(535, 145, 330, 500);
    public static readonly MissionTwoIntroRect IssacFaceRegion = new(105, 280, 255, 245);
    public static readonly MissionTwoIntroRect ChrandyFaceRegion = new(1135, 205, 210, 305);

    public static readonly IReadOnlyList<MissionTwoIntroBeat> Beats =
    [
        new("ISSAC WYNTER", "issac", "I now know the truth about our little delivery package. We were set up. The package was set to deploy robots to take over our ship as soon as we left Ice Moon Station."),
        new("ISSAC WYNTER", "issac", "The package was created by Emperor Ompok, the ruler of the Cyborg Directive. It didn't matter what we did—the attack by the robots was inevitable."),
        new("ISSAC WYNTER", "issac", "When it opened, it scanned its surroundings and used nearby tech to create the attack robots. Their mission was to kill us and take control of our ship before we reached Cheromia."),
        new("ISSAC WYNTER", "issac", "Once there, they would plant Ompok's AI into the Cheromia computer core and replicate itself across the system."),
        new("CAPTAIN JEANE GERSON", "jeane", "So we stopped them?", true),
        new("ISSAC WYNTER", "issac", "Yes. For the time being, anyway. But we need to keep an eye out for any robots that may be created. The AI may still be in our ship."),
        new("NANCE PARKER", "nance", "Great! So they could attack again at any moment.", true),
        new("XANDER MCRAVEN", "xander", "Not any moment—THIS moment! Here comes another wave for us to deal with!"),
    ];
}
