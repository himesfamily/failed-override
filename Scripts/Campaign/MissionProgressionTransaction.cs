namespace FailedOverride.Campaign;

public sealed class MissionProgressionTransaction
{
    readonly CampaignState campaign;
    readonly MissionRunMode mode;
    readonly int mission;
    readonly IReadOnlyList<string> deployedIds;
    readonly Dictionary<string, CharacterProgression> progressionSnapshot;
    readonly Dictionary<string, int> levelSnapshot;
    readonly Dictionary<string, int> experienceSnapshot;
    readonly Dictionary<string, int> bondSnapshot;
    readonly HashSet<string> completedMissionSnapshot;
    bool closed;

    public HashSet<string> NewlyRewardedIds { get; } = [];

    public MissionProgressionTransaction(CampaignState campaign, MissionRunMode mode, int mission, IReadOnlyList<string> deployedIds)
    {
        this.campaign = campaign;
        this.mode = mode;
        this.mission = mission;
        this.deployedIds = deployedIds;
        progressionSnapshot = campaign.SnapshotProgressions();
        levelSnapshot = new(campaign.Levels);
        experienceSnapshot = new(campaign.Experience);
        bondSnapshot = new(campaign.Bonds);
        completedMissionSnapshot = [.. campaign.CompletedMissions];
    }

    public void CommitVictory(IReadOnlyDictionary<string, int> xpGains)
    {
        if (closed) return;
        closed = true;
        var missionId = mission switch
        {
            1 => "failed-override",
            2 => "mission-2",
            3 => "mission-3",
            _ => throw new ArgumentOutOfRangeException(nameof(mission))
        };
        if (mode == MissionRunMode.Campaign) campaign.CompletedMissions.Add(missionId);
        foreach (var id in deployedIds)
        {
            campaign.Progress(id).CompletedMissionIds.Add(missionId);
            if (campaign.TryGrantPendingClassProgression(id)) NewlyRewardedIds.Add(id);
        }
        campaign.GrowBonds(deployedIds);
    }

    public void Rollback()
    {
        if (closed) return;
        closed = true;
        campaign.RestoreProgressions(progressionSnapshot, levelSnapshot, experienceSnapshot);
        campaign.Bonds.Clear();
        foreach (var pair in bondSnapshot) campaign.Bonds[pair.Key] = pair.Value;
        campaign.CompletedMissions.Clear();
        campaign.CompletedMissions.UnionWith(completedMissionSnapshot);
    }
}
