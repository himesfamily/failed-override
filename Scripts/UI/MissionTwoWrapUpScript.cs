namespace FailedOverride.UI;

public static class MissionTwoWrapUpScript
{
    public const string Scene = "MISSION 2 COMPLETE · DEBRIEFING ROOM";

    public static readonly IReadOnlyList<MissionTwoIntroBeat> Beats =
    [
        new("CAPTAIN JEANE GERSHON", "jeane", "Good job, team. We need to find a way to stop this before it happens again."),
        new("EMPEROR OMPOK", "ompok", "Sorry to interrupt your meeting, friends. But just to save you some time and effort, there is no way to stop what is happening."),
        new("CAPTAIN JEANE GERSHON", "jeane", "Issac, what is happening here? Who is this, and how is he talking on our monitor?"),
        new("ISSAC WYNTER", "issac", "This is Emperor Ompok, Captain, the ruler of the Cyborg Directive. He is able to talk to us because he has infected our ship's AI."),
        new("EMPEROR OMPOK", "ompok", "Thanks for the introduction, Issac."),
    ];
}
