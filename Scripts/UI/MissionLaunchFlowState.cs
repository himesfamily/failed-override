using FailedOverride.Campaign;
namespace FailedOverride.UI;
public enum MissionLaunchStage { RosterSelection, CrewPreparation, Cinematic, Battle }
public sealed class MissionLaunchFlowState
{
 readonly List<string> draft=[];
 public int Mission{get;} public MissionRunMode Mode{get;} public MissionLaunchStage Stage{get;private set;} public IReadOnlyList<string> DraftRoster=>draft;
 public MissionLaunchFlowState(int mission,IEnumerable<string> initial):this(mission,MissionRunMode.Campaign,initial){}
 public MissionLaunchFlowState(int mission,MissionRunMode mode,IEnumerable<string> initial){Mission=mission;Mode=mode;draft.AddRange(initial.Distinct());Stage=RequiresRosterSelection?MissionLaunchStage.RosterSelection:MissionLaunchStage.CrewPreparation;}
 bool RequiresRosterSelection=>Mode==MissionRunMode.TrainingSimulation||Mission>=3;
 public bool ConfirmRoster(IEnumerable<string> roster){var candidate=roster.Distinct().ToList();if(RequiresRosterSelection&&candidate.Count!=4)return false;draft.Clear();draft.AddRange(candidate);Stage=MissionLaunchStage.CrewPreparation;return true;}
 public void BackToRoster(){if(RequiresRosterSelection)Stage=MissionLaunchStage.RosterSelection;}
 public bool BeginCinematic(){if(RequiresRosterSelection&&draft.Count!=4)return false;Stage=MissionLaunchStage.Cinematic;return true;}
 public void BeginBattle()=>Stage=MissionLaunchStage.Battle;
}
