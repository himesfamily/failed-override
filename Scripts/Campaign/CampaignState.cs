using FailedOverride.Units;
namespace FailedOverride.Campaign;
public sealed class CampaignState {
 public HashSet<string> CompletedMissions {get;}=[]; public Dictionary<string,int> Bonds {get;}=[]; public Dictionary<string,int> Levels {get;}=[]; public Dictionary<string,int> Experience {get;}=[];
 public string IssacUpgrade {get;set;}=""; public string XanderUpgrade {get;set;}=""; public string NanceUpgrade {get;set;}=""; public string AkikoUpgrade {get;set;}="";
 public string Key(string a,string b)=>string.CompareOrdinal(a,b)<0?$"{a}|{b}":$"{b}|{a}";
 public int Bond(string a,string b)=>Bonds.GetValueOrDefault(Key(a,b));
 public void Apply(IEnumerable<Unit> units){foreach(var u in units.Where(x=>x.Hero||x.Role==Role.Officer)){u.Level=Levels.GetValueOrDefault(u.Id,1);u.Xp=Experience.GetValueOrDefault(u.Id);}}
 public void Record(IEnumerable<Unit> units){foreach(var u in units.Where(x=>x.Hero||x.Role==Role.Officer)){Levels[u.Id]=u.Level;Experience[u.Id]=u.Xp;}}
 public void GrowBonds(IEnumerable<string> ids){var list=ids.Distinct().ToList();for(int i=0;i<list.Count;i++)for(int j=i+1;j<list.Count;j++){var k=Key(list[i],list[j]);Bonds[k]=Math.Min(3,Bonds.GetValueOrDefault(k)+1);}}
}
public sealed class CampaignSaveData {
 public string IssacUpgrade {get;set;}="";public string XanderUpgrade {get;set;}="";public string NanceUpgrade {get;set;}="";public string AkikoUpgrade {get;set;}="";
 public string[] CompletedMissions {get;set;}=[];public Dictionary<string,int> Bonds {get;set;}=[];public Dictionary<string,int> Levels {get;set;}=[];public Dictionary<string,int> Experience {get;set;}=[];
 public static CampaignSaveData From(CampaignState c)=>new(){IssacUpgrade=c.IssacUpgrade,XanderUpgrade=c.XanderUpgrade,NanceUpgrade=c.NanceUpgrade,AkikoUpgrade=c.AkikoUpgrade,CompletedMissions=[..c.CompletedMissions],Bonds=new(c.Bonds),Levels=new(c.Levels),Experience=new(c.Experience)};
 public CampaignState ToState(){var c=new CampaignState{IssacUpgrade=IssacUpgrade,XanderUpgrade=XanderUpgrade,NanceUpgrade=NanceUpgrade,AkikoUpgrade=AkikoUpgrade};foreach(var x in CompletedMissions)c.CompletedMissions.Add(x);foreach(var x in Bonds)c.Bonds[x.Key]=x.Value;foreach(var x in Levels)c.Levels[x.Key]=x.Value;foreach(var x in Experience)c.Experience[x.Key]=x.Value;return c;}
}
