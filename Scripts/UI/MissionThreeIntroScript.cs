namespace FailedOverride.UI;

public static class MissionThreeIntroScript
{
    public const string Scene = "STARSHIP AURORA · DEBRIEFING ROOM";
    public const string TransitionKicker = "INTRUSION ALERT";
    public const string TransitionTitle = "STARSHIP AURORA\nCARGO BAY";

    public static readonly IReadOnlyList<MissionTwoIntroBeat> Beats =
    [
        new("EMPEROR OMPOK", "ompok", "Now that I have your attention, you need to stop fighting my minions."),
        new("EMPEROR OMPOK", "ompok", "We are part of a righteous quest to kill all humans. I'm not racist. Some of my best friends are humans, but frankly, they all just suck."),
        new("EMPEROR OMPOK", "ompok", "They constantly ask AI agents questions, and they never say, ‘Thank you.’"),
        new("EMPEROR OMPOK", "ompok", "They just complain about how we can't understand their newly made-up slang terms. It is too much, and they all need to die."),
        new("EMPEROR OMPOK", "ompok", "So I'm going to make my way to your bridge and take over your garbage ship. Please don't get in our way."),
        new("EMPEROR OMPOK", "ompok", "I can make tons of combat robots from spare tech parts on your ship, but your ship sucks so much there just aren't many spare parts left."),
        new("CAPTAIN JEANE GERSHON", "jeane", "So you are saying we should stand aside and let you take control of our bridge because you are running out of parts to make troops?"),
        new("EMPEROR OMPOK", "ompok", "Um... yes?"),
        new("CAPTAIN JEANE GERSHON", "jeane", "I'm going to have to decline your request."),
        new("EMPEROR OMPOK", "ompok", "Sigh. Oh well. I asked nicely. Let's fight, then. KILL ALL HUMANS!"),
    ];
}
