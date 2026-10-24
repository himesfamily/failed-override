namespace FailedOverride.Battle;

public readonly record struct DamageBreakdown(int FinalDamage,int HighGroundBonus,int HighGroundProtection,int LowCoverProtection)
{
    public string ModifierSummary => string.Join(" · ", new[]
    {
        HighGroundBonus > 0 ? $"High ground +{HighGroundBonus}" : "",
        HighGroundProtection > 0 ? $"Target above -{HighGroundProtection}" : "",
        LowCoverProtection > 0 ? $"Low cover -{LowCoverProtection}" : ""
    }.Where(x => x.Length > 0));
}
