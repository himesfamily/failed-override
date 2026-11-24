namespace FailedOverride.UI;

public static class OpeningCinematicAudio
{
    public const string IssacScreamResource = "res://Assets/Audio/SFX/issac_scream_wilhelm.mp3";

    public static bool ShouldPlayIssacScream(string speaker, string dialogue) =>
        string.Equals(speaker, "SYSTEM", StringComparison.Ordinal) &&
        dialogue.Contains("Issac screams", StringComparison.Ordinal);
}
