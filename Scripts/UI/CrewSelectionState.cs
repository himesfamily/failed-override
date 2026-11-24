using FailedOverride.Campaign;

namespace FailedOverride.UI;

public sealed class CrewSelectionState
{
    const int MaximumCrew = 4;
    readonly CampaignState campaign;
    readonly HashSet<string> selected = [];
    readonly List<string> selectedOrder = [];

    public int Mission { get; }
    public MissionRunMode Mode { get; }
    public IReadOnlyList<string> SelectedIds => selectedOrder;
    public string CountLabel => $"CREW SELECTED {selectedOrder.Count} / {MaximumCrew}";
    public bool CanContinue => campaign.ValidateRoster(Mission, selectedOrder, Mode);

    public CrewSelectionState(CampaignState campaign, int mission, IEnumerable<string> selectedIds)
        : this(campaign, mission, selectedIds, MissionRunMode.Campaign) { }

    public CrewSelectionState(CampaignState campaign, int mission, IEnumerable<string> selectedIds, MissionRunMode mode)
    {
        this.campaign = campaign;
        Mission = mission;
        Mode = mode;
        foreach (var id in campaign.RequiredCrew(mission, mode)) AddSeed(id);
        foreach (var id in selectedIds) AddSeed(id);
    }

    void AddSeed(string id)
    {
        if (selectedOrder.Count >= MaximumCrew || !campaign.AvailableCrew(Mission, Mode).Contains(id) || !selected.Add(id)) return;
        selectedOrder.Add(id);
    }

    public bool IsSelected(string id) => selected.Contains(id);
    public bool IsRequired(string id) => campaign.RequiredCrew(Mission, Mode).Contains(id);

    public bool CanSelect(string id)
    {
        if (!campaign.AvailableCrew(Mission, Mode).Contains(id) || IsRequired(id)) return false;
        return IsSelected(id) || selectedOrder.Count < MaximumCrew;
    }

    public bool Toggle(string id)
    {
        if (!CanSelect(id)) return false;
        if (selected.Remove(id))
        {
            selectedOrder.Remove(id);
            return true;
        }
        selected.Add(id);
        selectedOrder.Add(id);
        return true;
    }
}
