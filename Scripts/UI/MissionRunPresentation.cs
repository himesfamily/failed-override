using FailedOverride.Campaign;

namespace FailedOverride.UI;

public static class MissionRunPresentation
{
    public static string BattleBanner(MissionRunMode mode) => mode == MissionRunMode.TrainingSimulation ? "VIRTUAL REPLAY" : "";
    public static string ResultHeading(MissionRunMode mode, bool victory) => mode == MissionRunMode.TrainingSimulation
        ? victory ? "TRAINING SIMULATION COMPLETE" : "TRAINING SIMULATION FAILED"
        : victory ? "MISSION COMPLETE" : "MISSION FAILED";
    public static string RewardHeading(MissionRunMode mode) => mode == MissionRunMode.TrainingSimulation ? "TRAINING SIMULATION REWARDS" : "MISSION REWARDS";
    public static string ArtworkId(string unitId) => unitId == "simulated-issac-captive" ? "issac" : unitId;
    public static bool ShouldPlayIntro(MissionRunMode mode, int mission) => mode == MissionRunMode.Campaign && mission is 2 or 3;
    public static bool ShouldPlayWrapUp(MissionRunMode mode, int mission) => mode == MissionRunMode.Campaign && mission is 1 or 2;
    public static bool ShouldPlayTutorial(MissionRunMode mode, int mission) => mode == MissionRunMode.Campaign && mission == 1;
}
