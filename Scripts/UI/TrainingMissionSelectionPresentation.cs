namespace FailedOverride.UI;

public static class TrainingMissionSelectionPresentation
{
    public const string Title = "TRAINING SIMULATION";
    public const string Explanation = "Virtual replays award full XP and available Class Progressions to deployed crew. Crew left behind receive no progress.";
    public const string EmptyMessage = "No completed missions are available for simulation yet.";

    public static string MissionTitle(int mission) => mission switch
    {
        1 => "Mission 1 · Failed Override",
        2 => "Mission 2 · Security Center",
        3 => "Mission 3 · Unexpected Cargo",
        _ => throw new ArgumentOutOfRangeException(nameof(mission))
    };

    public static string Objective(int mission) => mission switch
    {
        1 => "Rescue the simulated Issac from corrupted security droids.",
        2 => "Reach the security-center passage and keep the crew alive.",
        3 => "Defeat Ompok's proxy force before it takes the bridge.",
        _ => throw new ArgumentOutOfRangeException(nameof(mission))
    };
}
