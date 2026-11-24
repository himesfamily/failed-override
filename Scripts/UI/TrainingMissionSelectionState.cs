using FailedOverride.Campaign;

namespace FailedOverride.UI;

public sealed class TrainingMissionSelectionState
{
    readonly HashSet<int> missions;
    public IReadOnlyList<int> Missions { get; }
    public int? SelectedMission { get; private set; }

    public TrainingMissionSelectionState(CampaignState campaign)
    {
        Missions = TrainingSimulationPolicy.CompletedMissionNumbers(campaign);
        missions = [.. Missions];
    }

    public bool CanSelect(int mission) => missions.Contains(mission);

    public bool Select(int mission)
    {
        if (!CanSelect(mission)) return false;
        SelectedMission = mission;
        return true;
    }
}
