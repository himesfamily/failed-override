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

    public bool Pending { get; private set; }
    public bool Active { get; private set; }
    public bool ImpactResolved { get; private set; }
    public RangedAttackEffect Effect { get; private set; }
    public float Progress => duration <= 0 ? 0 : Math.Clamp((float)(elapsed / duration), 0, 1);
    public float ImpactProgress => Progress >= 1 ? 1 : Math.Clamp((Progress - 0.8f) / 0.2f, 0, 1);
    public bool CompletedThisFrame { get; private set; }
    public bool ImpactStartedThisFrame { get; private set; }
    public bool BlocksGameplayInput => Pending;
    public bool DefersBattleResult => Pending;
    public bool PreserveTargetSnapshot => Pending && !ImpactResolved;

    public int DisplayedTargetHp(int hpBefore, int hpAfter) => PreserveTargetSnapshot ? hpBefore : hpAfter;

    public void Prepare(Team team, int weaponRange)
    {
        elapsed = 0;
        CompletedThisFrame = false;
        ImpactStartedThisFrame = false;
        ImpactResolved = false;
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
        Pending = Effect != RangedAttackEffect.None;
        Active = Pending;
    }

    public void Activate()
    {
        Active = Pending && Effect != RangedAttackEffect.None;
    }

    public void Start(Team team, int weaponRange)
    {
        Prepare(team, weaponRange);
        Activate();
    }

    public void Advance(double delta)
    {
        CompletedThisFrame = false;
        ImpactStartedThisFrame = false;
        if (!Active) return;
        var previousProgress = Progress;
        elapsed = Math.Min(duration, elapsed + Math.Max(0, delta));
        if (!ImpactResolved && previousProgress < 0.8f && Progress >= 0.8f)
        {
            ImpactResolved = true;
            ImpactStartedThisFrame = true;
        }
        if (elapsed < duration) return;
        Active = false;
        CompletedThisFrame = true;
    }

    public void CompletePresentation()
    {
        Pending = false;
        Active = false;
        ImpactResolved = false;
        Effect = RangedAttackEffect.None;
        elapsed = 0;
        duration = 0;
        CompletedThisFrame = false;
        ImpactStartedThisFrame = false;
    }
}
