using Godot; using System.Text.Json;
namespace FailedOverride.Campaign;
public static class CampaignStorage {
 const string Path="user://failed_override_campaign.json";
 public static bool Exists()=>Godot.FileAccess.FileExists(Path);
 public static void Save(CampaignState state){using var f=Godot.FileAccess.Open(Path,Godot.FileAccess.ModeFlags.Write);f.StoreString(JsonSerializer.Serialize(CampaignSaveData.From(state),new JsonSerializerOptions{WriteIndented=true}));}
 public static CampaignState? Load(){if(!Exists())return null;using var f=Godot.FileAccess.Open(Path,Godot.FileAccess.ModeFlags.Read);try{return JsonSerializer.Deserialize<CampaignSaveData>(f.GetAsText())?.ToState();}catch{return null;}}
 public static void Delete(){if(Exists())DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(Path));}
}
