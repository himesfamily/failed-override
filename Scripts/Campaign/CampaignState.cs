using FailedOverride.Units;
namespace FailedOverride.Campaign;
public sealed class CampaignState {
 public HashSet<string> CompletedMissions {get;}=[]; public Dictionary<string,int> Bonds {get;}=[]; public Dictionary<string,int> Levels {get;}=[]; public Dictionary<string,int> Experience {get;}=[];
 public Dictionary<string,CharacterProgression> Progressions {get;}=[];
 public string IssacUpgrade {get;set;}=""; public string XanderUpgrade {get;set;}=""; public string NanceUpgrade {get;set;}=""; public string AkikoUpgrade {get;set;}="";
 public string Key(string a,string b)=>string.CompareOrdinal(a,b)<0?$"{a}|{b}":$"{b}|{a}";
 public int Bond(string a,string b)=>Bonds.GetValueOrDefault(Key(a,b));
 public CharacterProgression Progress(string id){if(Progressions.TryGetValue(id,out var current))return current;var legacyLevel=Levels.GetValueOrDefault(id,1);current=new(){Level=legacyLevel,Xp=Experience.GetValueOrDefault(id),AttackBonus=Math.Max(0,legacyLevel-1)};Progressions[id]=current;return current;}
 public void SetTrainingPath(string id,TrainingPath path)=>Progress(id).TrainingPath=path;
 public bool HasMissionCredit(string characterId,string missionId)=>Progress(characterId).CompletedMissionIds.Contains(missionId);
 public IReadOnlyList<string> AvailableCrew(int mission)=>mission switch{1=>["xander","nance","akiko"],2=>["xander","nance","akiko","issac"],_=>["xander","nance","akiko","issac","chrandy"]};
 public IReadOnlyList<string> RequiredCrew(int mission)=>mission switch{1=>AvailableCrew(1),2=>AvailableCrew(2),_=>[]};
 public bool ValidateRoster(int mission,IEnumerable<string> ids){var list=ids.ToList();var available=AvailableCrew(mission);return list.Count>0&&list.Count<=4&&list.Distinct().Count()==list.Count&&list.All(available.Contains)&&RequiredCrew(mission).All(list.Contains)&&(mission<3||list.Count==4);}
 public IReadOnlyList<ClassProgressionDefinition> AvailableClassProgressions(string id){var p=Progress(id);return ClassProgressionCatalog.For(id).Where(x=>!p.ClassProgressions.Contains(x.Id)&&(x.Prerequisite==null||p.ClassProgressions.Contains(x.Prerequisite))).ToList();}
 public bool ChooseClassProgression(string id,string progressionId){var p=Progress(id);var choice=AvailableClassProgressions(id).FirstOrDefault(x=>x.Id==progressionId);if(choice==null||p.PendingClassProgressions<=0)return false;p.ClassProgressions.Add(progressionId);p.PendingClassProgressions--;return true;}
 public IReadOnlyList<LevelUpAward> AwardXp(string id,int amount){var p=Progress(id);p.Xp+=Math.Max(0,amount);List<LevelUpAward> awards=[];while(p.Xp>=100){p.Xp-=100;p.Level++;var path=p.TrainingPath==TrainingPath.Unassigned?RandomPath():p.TrainingPath;ApplyAdvance(p,path);awards.Add(new(p.Level,path));}Levels[id]=p.Level;Experience[id]=p.Xp;return awards;}
 static TrainingPath RandomPath()=>new[]{TrainingPath.Health,TrainingPath.Armor,TrainingPath.Movement,TrainingPath.Attack}[Random.Shared.Next(4)];
 static void ApplyAdvance(CharacterProgression p,TrainingPath path){switch(path){case TrainingPath.Health:p.HealthBonus+=4;break;case TrainingPath.Armor:p.ArmorBonus++;break;case TrainingPath.Movement:p.MovementBonus++;break;case TrainingPath.Attack:p.AttackBonus++;break;}}
 public void Apply(Unit u){if(!u.Hero&&u.Role!=Role.Officer)return;var p=Progress(u.Id);u.Level=p.Level;u.Xp=p.Xp;u.BaseHp+=p.HealthBonus;u.Hp+=p.HealthBonus;u.BaseArmor+=p.ArmorBonus;u.BaseMove+=p.MovementBonus;u.AttackBonus=p.AttackBonus;u.ClassProgressions=[..p.ClassProgressions];}
 public void Apply(IEnumerable<Unit> units){foreach(var u in units)Apply(u);}
 public void Record(IEnumerable<Unit> units){foreach(var u in units.Where(x=>x.Hero||x.Role==Role.Officer)){var p=Progress(u.Id);p.Level=u.Level;p.Xp=u.Xp;Levels[u.Id]=u.Level;Experience[u.Id]=u.Xp;}}
 public void GrowBonds(IEnumerable<string> ids){var list=ids.Distinct().ToList();for(int i=0;i<list.Count;i++)for(int j=i+1;j<list.Count;j++){var k=Key(list[i],list[j]);Bonds[k]=Math.Min(3,Bonds.GetValueOrDefault(k)+1);}}
 public Dictionary<string,CharacterProgression> SnapshotProgressions()=>Progressions.ToDictionary(x=>x.Key,x=>x.Value.Clone());
 public void RestoreProgressions(IReadOnlyDictionary<string,CharacterProgression> snapshot){Progressions.Clear();Levels.Clear();Experience.Clear();foreach(var x in snapshot){Progressions[x.Key]=x.Value.Clone();Levels[x.Key]=x.Value.Level;Experience[x.Key]=x.Value.Xp;}}
 public void RestoreProgression(string id,IReadOnlyDictionary<string,CharacterProgression> snapshot){if(snapshot.TryGetValue(id,out var saved)){Progressions[id]=saved.Clone();Levels[id]=saved.Level;Experience[id]=saved.Xp;}else{Progressions.Remove(id);Levels.Remove(id);Experience.Remove(id);}}
}
public sealed class CampaignSaveData {
 public int ProgressionSchemaVersion {get;set;}
 public string IssacUpgrade {get;set;}="";public string XanderUpgrade {get;set;}="";public string NanceUpgrade {get;set;}="";public string AkikoUpgrade {get;set;}="";
 public string[] CompletedMissions {get;set;}=[];public Dictionary<string,int> Bonds {get;set;}=[];public Dictionary<string,int> Levels {get;set;}=[];public Dictionary<string,int> Experience {get;set;}=[];public Dictionary<string,CharacterProgression> Progressions {get;set;}=[];
 public static CampaignSaveData From(CampaignState c)=>new(){ProgressionSchemaVersion=1,IssacUpgrade=c.IssacUpgrade,XanderUpgrade=c.XanderUpgrade,NanceUpgrade=c.NanceUpgrade,AkikoUpgrade=c.AkikoUpgrade,CompletedMissions=[..c.CompletedMissions],Bonds=new(c.Bonds),Levels=new(c.Levels),Experience=new(c.Experience),Progressions=c.Progressions.ToDictionary(x=>x.Key,x=>x.Value.Clone())};
 public CampaignState ToState(){var c=new CampaignState{IssacUpgrade=IssacUpgrade,XanderUpgrade=XanderUpgrade,NanceUpgrade=NanceUpgrade,AkikoUpgrade=AkikoUpgrade};foreach(var x in CompletedMissions)c.CompletedMissions.Add(x);foreach(var x in Bonds)c.Bonds[x.Key]=x.Value;foreach(var x in Levels)c.Levels[x.Key]=x.Value;foreach(var x in Experience)c.Experience[x.Key]=x.Value;foreach(var x in Progressions)c.Progressions[x.Key]=x.Value.Clone();if(ProgressionSchemaVersion<1)MigrateLegacyMissionCredit(c);return c;}
 static void MigrateLegacyMissionCredit(CampaignState c){foreach(var missionId in c.CompletedMissions){var mission=missionId switch{"failed-override"=>1,"mission-2"=>2,"mission-3"=>3,_=>0};if(mission==0)continue;foreach(var id in c.AvailableCrew(mission))c.Progress(id).CompletedMissionIds.Add(missionId);}}
}
