namespace FailedOverride.Campaign;

public enum TrainingPath { Unassigned, Health, Armor, Movement, Attack }

public sealed class CharacterProgression
{
    public int Level { get; set; } = 1;
    public int Xp { get; set; }
    public TrainingPath TrainingPath { get; set; }
    public int HealthBonus { get; set; }
    public int ArmorBonus { get; set; }
    public int MovementBonus { get; set; }
    public int AttackBonus { get; set; }
    public int PendingClassProgressions { get; set; }
    public HashSet<string> ClassProgressions { get; set; } = [];
    public HashSet<string> CompletedMissionIds { get; set; } = [];

    public CharacterProgression Clone() => new()
    {
        Level = Level, Xp = Xp, TrainingPath = TrainingPath,
        HealthBonus = HealthBonus, ArmorBonus = ArmorBonus,
        MovementBonus = MovementBonus, AttackBonus = AttackBonus,
        PendingClassProgressions = PendingClassProgressions,
        ClassProgressions = [.. ClassProgressions],
        CompletedMissionIds = [.. CompletedMissionIds]
    };
}

public readonly record struct LevelUpAward(int Level, TrainingPath Path);
