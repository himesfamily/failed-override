namespace FailedOverride.UI;

public sealed record BriefingSpeakerFocusTransition(
    string? PreviousGroupKey,
    string? CurrentGroupKey,
    bool HologramFocused,
    int? PreviousLayer,
    int? CurrentLayer,
    bool ClearGroupImmediately,
    bool ReusesCurrentLayer);

public sealed class BriefingSpeakerFocusTransitionState
{
    public string? CurrentGroupKey { get; private set; }
    public bool HologramFocused { get; private set; }
    readonly bool[] layerVisible = new bool[2];
    int activeLayer;

    public bool LayerVisible(int layer) => layerVisible[layer];

    public BriefingSpeakerFocusTransition Select(string portraitKey)
    {
        SettleInterruptedTransition();

        if (BriefingSpeakerFocusLayout.IsHologram(portraitKey))
        {
            var previous = CurrentGroupKey;
            int? previousLayer = previous is null ? null : activeLayer;
            CurrentGroupKey = null;
            HologramFocused = true;
            HideBothLayers();
            return new(previous, null, true, previousLayer, null, true, false);
        }

        var marker = BriefingSpeakerFocusLayout.MarkerFor(portraitKey);
        if (marker is null)
        {
            var previous = CurrentGroupKey;
            int? previousLayer = previous is null ? null : activeLayer;
            Clear();
            return new(previous, null, false, previousLayer, null, true, false);
        }

        if (string.Equals(CurrentGroupKey, portraitKey, StringComparison.Ordinal))
        {
            HologramFocused = false;
            layerVisible[activeLayer] = true;
            return new(null, CurrentGroupKey, false, null, activeLayer, false, true);
        }

        var previousGroupKey = CurrentGroupKey;
        int? previousGroupLayer = previousGroupKey is null ? null : activeLayer;
        var nextLayer = 1 - activeLayer;
        layerVisible[nextLayer] = true;
        activeLayer = nextLayer;
        CurrentGroupKey = portraitKey;
        HologramFocused = false;
        return new(previousGroupKey, CurrentGroupKey, false, previousGroupLayer, activeLayer, false, false);
    }

    public void Complete(BriefingSpeakerFocusTransition transition)
    {
        if (transition.PreviousLayer is int previousLayer && previousLayer != activeLayer)
            layerVisible[previousLayer] = false;
    }

    public void Clear()
    {
        CurrentGroupKey = null;
        HologramFocused = false;
        HideBothLayers();
    }

    void SettleInterruptedTransition()
    {
        for (var index = 0; index < layerVisible.Length; index++)
            layerVisible[index] = CurrentGroupKey is not null && index == activeLayer;
    }

    void HideBothLayers()
    {
        layerVisible[0] = false;
        layerVisible[1] = false;
    }
}
