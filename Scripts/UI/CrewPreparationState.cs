using FailedOverride.Campaign;

namespace FailedOverride.UI;

public sealed class CrewPreparationState
{
    readonly CampaignState campaign;
    readonly Dictionary<string, TrainingPath> draftPaths = [];
    readonly Dictionary<string, string> draftClassProgressions = [];
    readonly HashSet<string> selectedIds;
    readonly List<string> selectedOrder;

    public int Mission { get; }
    public IReadOnlyCollection<string> SelectedIds => selectedOrder;

    public CrewPreparationState(CampaignState campaign, int mission, IEnumerable<string> selected)
    {
        this.campaign = campaign;
        Mission = mission;
        selectedOrder = [.. selected.Distinct()];
        selectedIds = [.. selectedOrder];
        foreach (var id in campaign.AvailableCrew(mission)) draftPaths[id] = campaign.Progress(id).TrainingPath;
    }

    public TrainingPath TrainingPathFor(string id) => draftPaths.GetValueOrDefault(id, campaign.Progress(id).TrainingPath);
    public string? DraftClassProgressionFor(string id) => draftClassProgressions.GetValueOrDefault(id);
    public bool IsSelected(string id) => selectedIds.Contains(id);
    public bool IsRequired(string id) => campaign.RequiredCrew(Mission).Contains(id);
    public bool IsVeteran(string id, string missionId) => campaign.HasMissionCredit(id, missionId);

    public void SetTrainingPath(string id, TrainingPath path)
    {
        if (campaign.AvailableCrew(Mission).Contains(id)) draftPaths[id] = path;
    }

    public bool ToggleDeployment(string id, bool selected)
    {
        if (!campaign.AvailableCrew(Mission).Contains(id)) return false;
        if (!selected)
        {
            if (IsRequired(id)) return false;
            if (!selectedIds.Remove(id)) return false;
            selectedOrder.Remove(id);
            return true;
        }
        if (selectedIds.Contains(id)) return true;
        if (selectedIds.Count >= 4) return false;
        selectedIds.Add(id);
        selectedOrder.Add(id);
        return true;
    }

    public IReadOnlyList<ClassProgressionDefinition> AvailableClassProgressions(string id) =>
        campaign.Progress(id).PendingClassProgressions > 0 ? campaign.AvailableClassProgressions(id) : [];

    public IReadOnlyList<ClassProgressionDefinition> AcquiredClassProgressions(string id)
    {
        var acquired = campaign.Progress(id).ClassProgressions;
        return ClassProgressionCatalog.For(id).Where(x => acquired.Contains(x.Id)).ToList();
    }

    public bool SetClassProgression(string id, string progressionId)
    {
        if (!AvailableClassProgressions(id).Any(x => x.Id == progressionId)) return false;
        draftClassProgressions[id] = progressionId;
        return true;
    }

    public bool CanConfirmCharacter(string id)
    {
        if (TrainingPathFor(id) == TrainingPath.Unassigned) return false;
        return campaign.Progress(id).PendingClassProgressions == 0 ||
               draftClassProgressions.TryGetValue(id, out var choice) && AvailableClassProgressions(id).Any(x => x.Id == choice);
    }

    public string NextUnconfirmedSelectedId(string currentId, IReadOnlySet<string> confirmed)
    {
        var currentIndex = selectedOrder.IndexOf(currentId);
        if (currentIndex < 0) return selectedOrder.FirstOrDefault(id => !confirmed.Contains(id)) ?? currentId;
        for (var offset = 1; offset < selectedOrder.Count; offset++)
        {
            var candidate = selectedOrder[(currentIndex + offset) % selectedOrder.Count];
            if (!confirmed.Contains(candidate)) return candidate;
        }
        return currentId;
    }

    public bool ConfirmCharacter(string id)
    {
        if (!CanConfirmCharacter(id)) return false;
        campaign.SetTrainingPath(id, TrainingPathFor(id));
        if (draftClassProgressions.Remove(id, out var choice) && !campaign.ChooseClassProgression(id, choice)) return false;
        return true;
    }

    public bool CanBeginMission() =>
        campaign.ValidateRoster(Mission, selectedIds) &&
        selectedIds.All(id => campaign.Progress(id).TrainingPath != TrainingPath.Unassigned && campaign.Progress(id).PendingClassProgressions == 0);
}
