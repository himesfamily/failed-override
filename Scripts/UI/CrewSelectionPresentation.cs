using FailedOverride.Campaign;

namespace FailedOverride.UI;

public static class CrewSelectionPresentation
{
    public const string Title = "SELECT YOUR CREW";
    public const string Instruction = "Choose 4 crew members for this mission.";
    public const string SelectedLabel = "✓ SELECTED";
    public const string RequiredLabel = "REQUIRED";
    public const string MaximumMessage = "Four crew members selected. Remove one optional crew member to choose another.";
    public const string ContinueLabel = "Continue to Preparation";
    public static string TitleFor(MissionRunMode mode) => mode == MissionRunMode.TrainingSimulation ? "SELECT YOUR SIMULATION CREW" : Title;
    public static string InstructionFor(MissionRunMode mode) => mode == MissionRunMode.TrainingSimulation
        ? "Choose 4 unlocked crew members. Deployed crew earn permanent XP and available Class Progressions."
        : Instruction;

    public static string CardStatus(bool selected, bool required) => required
        ? RequiredLabel
        : selected ? SelectedLabel : "";

    public static string SelectionMessage(CrewSelectionState state) => state.SelectedIds.Count == 4
        ? MaximumMessage
        : "";
}
