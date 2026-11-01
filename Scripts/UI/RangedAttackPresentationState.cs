using FailedOverride.Units;

namespace FailedOverride.UI;

public enum RangedAttackEffect
{
    None,
    HeroBolt,
    EnemyBeam
}

public sealed class RangedAttackPresentationState
{
    const double HeroDuration = 0.25;
    const double EnemyDuration = 0.4;
    double elapsed;
    double duration;

    public bool Active { get; private set; }
    public RangedAttackEffect Effect { get; private set; }
    public float Progress => duration <= 0 ? 0 : Math.Clamp((float)(elapsed / duration), 0, 1);
    public float ImpactProgress => Progress >= 1 ? 1 : Math.Clamp((Progress - 0.8f) / 0.2f, 0, 1);
    public bool CompletedThisFrame { get; private set; }

    public void Start(Team team, int weaponRange)
    {
        elapsed = 0;
        CompletedThisFrame = false;
        Effect = weaponRange <= 1
            ? RangedAttackEffect.None
            : team switch
            {
                Team.Player => RangedAttackEffect.HeroBolt,
                Team.Enemy => RangedAttackEffect.EnemyBeam,
                _ => RangedAttackEffect.None
            };
        duration = Effect switch
        {
            RangedAttackEffect.HeroBolt => HeroDuration,
            RangedAttackEffect.EnemyBeam => EnemyDuration,
            _ => 0
        };
        Active = Effect != RangedAttackEffect.None;
    }

    public void Advance(double delta)
    {
        CompletedThisFrame = false;
        if (!Active) return;
        elapsed = Math.Min(duration, elapsed + Math.Max(0, delta));
        if (elapsed < duration) return;
        Active = false;
        CompletedThisFrame = true;
    }
}
