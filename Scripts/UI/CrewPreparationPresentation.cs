using FailedOverride.Campaign;

namespace FailedOverride.UI;

public static class CrewPreparationPresentation
{
    public const string TrainingPathExplanation =
        "Characters earn XP by attacking, healing, and completing useful actions. At 100 XP, they Level Up immediately. " +
        "Every Level Up, including Level 2, immediately improves that character's selected Training Path. " +
        "You may change this path between missions. Earlier Level Ups do not change. A reserve character without a Training Path may receive a random bonus from catch-up XP.";

    public const string TrainingPathInstruction = "Choose one Training Path for each deployed character before beginning the mission.";

    public const string ClassProgressionExplanation =
        "A Class Progression is a permanent reward for completing a mission with this character for the first time. " +
        "It is a larger ability choice, separate from automatic Level Ups. Choose and confirm one reward for each pending Class Progression. " +
        "A Tier II ability requires its matching Tier I ability first.";

    public const string ClassProgressionInstruction = "Choose one Class Progression, then confirm this character's choices.";

    public static string PathLabel(TrainingPath path, bool selected)
    {
        var choice = path switch
        {
            TrainingPath.Health => "Health\n+4 Health",
            TrainingPath.Armor => "Armor\n+1 Armor",
            TrainingPath.Movement => "Movement\n+1 Movement",
            TrainingPath.Attack => "Attack\n+1 damage",
            _ => path.ToString()
        };
        return selected ? $"✓ SELECTED\n{choice}" : choice;
    }

    public static string ClassProgressionLabel(ClassProgressionDefinition progression, bool selected)
    {
        var choice = $"{progression.Name}\n{progression.Description}";
        return selected ? $"✓ SELECTED\n{choice}" : choice;
    }
}
