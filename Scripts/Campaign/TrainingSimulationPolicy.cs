namespace FailedOverride.Campaign;

public static class TrainingSimulationPolicy
{
    static readonly (int Mission, string Id)[] Missions =
    [
        (1, "failed-override"),
        (2, "mission-2"),
        (3, "mission-3")
    ];

    public static IReadOnlyList<int> CompletedMissionNumbers(CampaignState campaign) =>
        Missions.Where(entry => campaign.CompletedMissions.Contains(entry.Id))
            .Select(entry => entry.Mission)
            .ToList();

    public static IReadOnlyList<string> UnlockedCrewIds(CampaignState campaign)
    {
        if (campaign.CompletedMissions.Contains("mission-2") || campaign.CompletedMissions.Contains("mission-3"))
            return CrewCatalog.AvailableIds(3);
        if (campaign.CompletedMissions.Contains("failed-override"))
            return CrewCatalog.AvailableIds(2);
        return [];
    }

    public static bool ValidateMission(CampaignState campaign, int mission) =>
        CompletedMissionNumbers(campaign).Contains(mission);

    public static bool ValidateRoster(CampaignState campaign, int mission, IEnumerable<string> ids)
    {
        var roster = ids.ToList();
        var unlocked = UnlockedCrewIds(campaign);
        return ValidateMission(campaign, mission)
            && roster.Count == 4
            && roster.Distinct().Count() == roster.Count
            && roster.All(unlocked.Contains);
    }
}
