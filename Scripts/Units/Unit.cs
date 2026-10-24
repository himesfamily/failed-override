namespace FailedOverride.Units;

public enum Team { Player, Enemy, Civilian }
public enum Role
{
    Scout, Shapeshifter, Tech, Robot, Droid, HeavyDroid, Officer, Medic,
    Twin, Dog, OmpokProxy, VaurBrute, KheltStalker, SynapticEngineer
}

public readonly record struct Cell(int X, int Y)
{
    public int Distance(Cell other) => Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
    public Cell[] Neighbors() => [new(X + 1, Y), new(X - 1, Y), new(X, Y + 1), new(X, Y - 1)];
}

public sealed record Weapon(string Name, int Power, int Range, bool Stun = false, bool StunOnly = false);

public sealed class Unit
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Token { get; init; }
    public Team Team { get; set; }
    public Role Role { get; init; }
    public Cell Position { get; set; }
    public int BaseHp { get; set; }
    public int Hp { get; set; }
    public int BaseArmor { get; set; }
    public int BaseMove { get; set; }
    public int Level { get; set; } = 1;
    public int Xp { get; set; }
    public bool Bear { get; set; }
    public bool Captured { get; set; }
    public bool Hidden { get; set; }
    public bool Moved { get; set; }
    public bool Acted { get; set; }
    public int Stunned { get; set; }
    public bool SkippedByStun { get; set; }
    public int MedicalSupplies { get; set; }

    public bool Alive => Hp > 0;
    public bool Small => Role is Role.Twin or Role.Dog;
    public bool Hero => Role is Role.Scout or Role.Shapeshifter or Role.Tech or Role.Medic;
    public int MaxHp => BaseHp + (Bear ? 6 : 0);
    public int Armor => BaseArmor + (Bear ? 2 : 0);
    public int Move => BaseMove - (Bear ? 1 : 0);

    public IReadOnlyList<Weapon> Weapons => Role switch
    {
        Role.Scout => [new("Plasma pistol", 6, 3), new("Vibro-axe", 8, 1)],
        Role.Shapeshifter when Bear => [new("Bear claws", 9, 1), new("Maul", 7, 1, true)],
        Role.Shapeshifter => [new("Pulse palm", 5, 2)],
        Role.Tech => [new("Arc tool", 5, 2)],
        Role.Robot => [new("Robot laser", 4, 2)],
        Role.Droid => [new("Stun ray", 4, 3, true), new("Saw-blade arm", 6, 1)],
        Role.HeavyDroid => [new("Heavy cannon", 8, 2), new("Crushing arm", 7, 1)],
        Role.Officer => [new("Officer pistol", 4, 3)],
        Role.Medic => [new("Stun pistol", 0, 3, true, true)],
        Role.OmpokProxy => [new("Neural cannon", 8, 3), new("Crushing limb", 10, 1)],
        Role.VaurBrute => [new("Shock club", 7, 1)],
        Role.KheltStalker => [new("Needle caster", 5, 2)],
        Role.SynapticEngineer => [new("Arc probe", 4, 2)],
        _ => []
    };
}
