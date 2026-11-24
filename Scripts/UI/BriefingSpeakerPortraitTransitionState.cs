namespace FailedOverride.UI;

public sealed record BriefingSpeakerPortraitTransition(
    string? PreviousKey,
    string? CurrentKey,
    bool HologramFocused,
    int? PreviousLayer,
    int? CurrentLayer,
    bool ReusesCurrentLayer);

public sealed class BriefingSpeakerPortraitTransitionState
{
    readonly bool[] visible = new bool[2];
    int activeLayer;
    public string? CurrentKey { get; private set; }
    public bool HologramFocused { get; private set; }
    public bool LayerVisible(int layer) => visible[layer];

    public BriefingSpeakerPortraitTransition Select(string portraitKey)
    {
        SettleInterruptedTransition();
        if (BriefingSpeakerFocusLayout.IsHologram(portraitKey))
        {
            var previous = CurrentKey;
            int? previousLayer = previous is null ? null : activeLayer;
            CurrentKey = null;
            HologramFocused = true;
            HideLayers();
            return new(previous, null, true, previousLayer, null, false);
        }

        if (BriefingSpeakerPortraitLayout.For(portraitKey) is null)
        {
            var previous = CurrentKey;
            int? previousLayer = previous is null ? null : activeLayer;
            Clear();
            return new(previous, null, false, previousLayer, null, false);
        }

        if (CurrentKey == portraitKey)
        {
            HologramFocused = false;
            visible[activeLayer] = true;
            return new(null, CurrentKey, false, null, activeLayer, true);
        }

        var oldKey = CurrentKey;
        int? oldLayer = oldKey is null ? null : activeLayer;
        activeLayer = 1 - activeLayer;
        visible[activeLayer] = true;
        CurrentKey = portraitKey;
        HologramFocused = false;
        return new(oldKey, CurrentKey, false, oldLayer, activeLayer, false);
    }

    public void Complete(BriefingSpeakerPortraitTransition transition)
    {
        if (transition.PreviousLayer is int previous && previous != activeLayer) visible[previous] = false;
    }

    public void Clear() { CurrentKey = null; HologramFocused = false; HideLayers(); }
    void SettleInterruptedTransition() { for (var i = 0; i < visible.Length; i++) visible[i] = CurrentKey is not null && i == activeLayer; }
    void HideLayers() { visible[0] = false; visible[1] = false; }
}
