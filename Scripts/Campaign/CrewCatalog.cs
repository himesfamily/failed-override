using FailedOverride.Units;

namespace FailedOverride.Campaign;

public sealed record CrewProfile(
    string Id,
    string Name,
    string ClassName,
    Role Role,
    string Token,
    int Health,
    int Armor,
    int Movement,
    string RoleSummary);

public static class CrewCatalog
{
    static readonly IReadOnlyDictionary<string, CrewProfile> Profiles =
        new Dictionary<string, CrewProfile>(StringComparer.Ordinal)
        {
            ["xander"] = new("xander", "Xander McRaven", "Scout", Role.Scout, "X", 18, 1, 6, "Mobile ranged fighter with a powerful melee weapon."),
            ["nance"] = new("nance", "Nance Parker", "Shapeshifter", Role.Shapeshifter, "N", 20, 1, 5, "Transforms into a durable bear for close combat."),
            ["akiko"] = new("akiko", "Akiko Ikeda", "Tech", Role.Tech, "A", 16, 1, 5, "Summons support robots and repairs allied machines."),
            ["issac"] = new("issac", "Issac Wynter", "Science Officer", Role.Officer, "I", 18, 0, 4, "Reprograms damaged enemy robots and repairs machines."),
            ["chrandy"] = new("chrandy", "Dr. Chandry Perra", "Medic", Role.Medic, "C", 17, 1, 5, "Heals organic allies and carries a stun pistol."),
            ["jeane"] = new("jeane", "Captain Jeane Gershon", "Captain", Role.Captain, "J", 24, 2, 6, "Elite combatant who can order an ally to act again."),
        };

    public static CrewProfile Profile(string id) => Profiles.TryGetValue(id, out var profile)
        ? profile
        : throw new ArgumentOutOfRangeException(nameof(id), id, "Unknown crew member.");

    public static IReadOnlyList<string> AvailableIds(int mission) => mission switch
    {
        1 => ["xander", "nance", "akiko"],
        2 => ["xander", "nance", "akiko", "issac"],
        _ => ["xander", "nance", "akiko", "issac", "chrandy", "jeane"],
    };
}
