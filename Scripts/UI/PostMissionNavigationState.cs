using FailedOverride.Campaign;

namespace FailedOverride.UI;

public enum PostMissionReturnTarget { CampaignMission, MainMenu }

public sealed class PostMissionNavigationState
{
    public MissionRunMode Mode { get; }
    public int? NextCampaignMission { get; }
    public bool CanAdvanceCampaign => Mode == MissionRunMode.Campaign && NextCampaignMission.HasValue;
    public bool CanOpenTraining { get; }
    public string PrimaryLabel => Mode == MissionRunMode.TrainingSimulation ? "Replay Another Training Mission" : "Advance to Next Campaign Mission";
    public string TrainingLabel => "Replay Previous Missions in Training Simulation";
    public string SecondaryLabel => Mode == MissionRunMode.TrainingSimulation ? "Return to Main Menu" : TrainingLabel;
    public string ReturnLabel => Mode == MissionRunMode.TrainingSimulation && NextCampaignMission.HasValue ? "Return to Campaign" : "Return to Main Menu";
    public bool ShowSecondaryAction => Mode != MissionRunMode.TrainingSimulation || NextCampaignMission.HasValue;
    public PostMissionReturnTarget ReturnTarget => NextCampaignMission.HasValue ? PostMissionReturnTarget.CampaignMission : PostMissionReturnTarget.MainMenu;

    public PostMissionNavigationState(CampaignState campaign, int mission, MissionRunMode mode)
    {
        Mode = mode;
        NextCampaignMission = NextIncompleteCampaignMission(campaign);
        CanOpenTraining = CanOpenTrainingFromLanding(campaign);
    }

    public static bool CanOpenTrainingFromLanding(CampaignState campaign) =>
        TrainingSimulationPolicy.CompletedMissionNumbers(campaign).Count > 0;

    public static int? NextIncompleteCampaignMission(CampaignState campaign)
    {
        if (!campaign.CompletedMissions.Contains("failed-override")) return 1;
        if (!campaign.CompletedMissions.Contains("mission-2")) return 2;
        if (!campaign.CompletedMissions.Contains("mission-3")) return 3;
        return null;
    }
}
