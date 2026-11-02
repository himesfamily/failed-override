using FailedOverride.Units;

namespace FailedOverride.UI;

public enum EnemyTurnPacingPhase
{
    Idle,
    Intermission,
    Preview
}

public sealed class EnemyTurnPacingState
{
    const double IntermissionDuration = 1.0;
    const double PreviewDuration = 0.6;
    double remaining;

    public EnemyTurnPacingPhase Phase { get; private set; }
    public bool PreviewStartedThisFrame { get; private set; }
    public bool ActionReadyThisFrame { get; private set; }

    public static Unit? TakeNextLiving(Queue<Unit> enemies)
    {
        while (enemies.TryDequeue(out var enemy))
            if (enemy.Alive) return enemy;
        return null;
    }

    public void BeginIntermission()
    {
        Phase = EnemyTurnPacingPhase.Intermission;
        remaining = IntermissionDuration;
        PreviewStartedThisFrame = false;
        ActionReadyThisFrame = false;
    }

    public void Advance(double delta)
    {
        PreviewStartedThisFrame = false;
        ActionReadyThisFrame = false;
        if (Phase == EnemyTurnPacingPhase.Idle) return;
        remaining -= Math.Max(0, delta);
        if (remaining > 0.000000001) return;
        if (Phase == EnemyTurnPacingPhase.Intermission)
        {
            Phase = EnemyTurnPacingPhase.Preview;
            remaining = PreviewDuration;
            PreviewStartedThisFrame = true;
            return;
        }
        Phase = EnemyTurnPacingPhase.Idle;
        remaining = 0;
        ActionReadyThisFrame = true;
    }
}
