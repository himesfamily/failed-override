using FailedOverride.Battle;
using FailedOverride.Campaign;

namespace FailedOverride.UI;

public sealed class ProgressionPresentationState
{
    const double Duration = .65;
    readonly Queue<object> steps = new();
    XpAwardResult? result;
    double elapsed;
    sealed record Segment(int Start, int End);

    public bool Active { get; private set; }
    public bool ShowingLevelUp { get; private set; }
    public int DisplayedXp { get; private set; }
    public string GainLabel => result == null ? "" : $"+{result.Amount} XP";
    public string Reason => result?.Reason ?? "";
    public string LevelUpTitle { get; private set; } = "";
    public string LevelUpDetail { get; private set; } = "";

    public void Begin(XpAwardResult award)
    {
        result = award;
        elapsed = 0;
        DisplayedXp = award.StartXp;
        steps.Clear();
        var start = award.StartXp;
        foreach (var level in award.LevelUps)
        {
            steps.Enqueue(new Segment(start, 100));
            steps.Enqueue(level);
            start = 0;
        }
        steps.Enqueue(new Segment(start, award.EndXp));
        ShowingLevelUp = false;
        LevelUpTitle = LevelUpDetail = "";
        Active = award.Amount > 0 || award.LevelUps.Count > 0;
        BeginNextStep();
    }

    public void Advance(double delta)
    {
        if (!Active || ShowingLevelUp || result == null) return;
        if (steps.Count == 0) { Active = false; return; }
        if (steps.Peek() is not Segment segment) return;
        elapsed = Math.Min(Duration, elapsed + Math.Max(0, delta));
        var progress = elapsed / Duration;
        DisplayedXp = segment.Start + (int)Math.Round((segment.End - segment.Start) * progress);
        if (elapsed < Duration) return;
        DisplayedXp = segment.End;
        steps.Dequeue();
        BeginNextStep();
    }

    public void DismissLevelUp()
    {
        if (!ShowingLevelUp) return;
        ShowingLevelUp = false;
        if (steps.Count > 0 && steps.Peek() is LevelUpAward) steps.Dequeue();
        DisplayedXp = 0;
        BeginNextStep();
    }

    void BeginNextStep()
    {
        elapsed = 0;
        if (steps.Count == 0) { Active = false; return; }
        if (steps.Peek() is Segment segment) { DisplayedXp = segment.Start; return; }
        var award = (LevelUpAward)steps.Peek();
        ShowingLevelUp = true;
        LevelUpTitle = $"LEVEL {award.Level}";
        LevelUpDetail = award.Path switch
        {
            TrainingPath.Health => "Health +4",
            TrainingPath.Armor => "Armor +1",
            TrainingPath.Movement => "Movement +1",
            TrainingPath.Attack => "Attack +1",
            _ => "Level increased"
        };
    }
}
