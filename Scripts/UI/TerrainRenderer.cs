using Godot;
using FailedOverride.Battle;
using FailedOverride.Units;
namespace FailedOverride.UI;
public static class TerrainRenderer {
 public static void Draw(CanvasItem canvas,BattleState battle,Vector2 origin,int size){
  (Color floor,Color alt,Color edge,Color cargo) p=battle.Mission switch{2=>(new Color("46515a"),new Color("3d4851"),new Color("1b242b"),new Color("a9652f")),3=>(new Color("334657"),new Color("2c3d4d"),new Color("111c28"),new Color("8b5a35")),_=>(new Color("536575"),new Color("495c6c"),new Color("18232d"),new Color("a76b36"))};
  for(int y=0;y<12;y++)for(int x=0;x<12;x++){var t=battle.TerrainAt(new Cell(x,y));var o=origin+new Vector2(x*size,y*size);var r=new Rect2(o,new Vector2(size-2,size-2));canvas.DrawRect(r,(x+y)%2==0?p.floor:p.alt);canvas.DrawRect(r,new Color(1,1,1,.08f),false,1);
   if(t.Kind==TerrainKind.Wall){canvas.DrawRect(new Rect2(o+new Vector2(4,9),new Vector2(size-7,size-7)),new Color(0,0,0,.5f));canvas.DrawRect(r.Grow(-4),p.edge);canvas.DrawLine(o+new Vector2(5,6),o+new Vector2(size-7,6),new Color("607181"),4);}
   else if(t.Kind==TerrainKind.Pillar){canvas.DrawCircle(o+new Vector2(size/2+3,size/2+5),size*.34f,new Color(0,0,0,.5f));canvas.DrawCircle(o+new Vector2(size/2,size/2),size*.34f,new Color("26343e"));canvas.DrawCircle(o+new Vector2(size/2-5,size/2-6),size*.13f,new Color("71818c"));}
   else if(t.Kind==TerrainKind.LowCover){canvas.DrawRect(new Rect2(o+new Vector2(7,23),new Vector2(size-12,30)),new Color(0,0,0,.45f));canvas.DrawRect(new Rect2(o+new Vector2(6,17),new Vector2(size-12,31)),p.cargo.Darkened(.28f));canvas.DrawLine(o+new Vector2(7,15),o+new Vector2(size-7,15),p.cargo.Lightened(.25f),8);}
   else if(t.Kind==TerrainKind.Elevated){canvas.DrawRect(new Rect2(o+new Vector2(7,13),new Vector2(size-10,size-7)),new Color(0,0,0,.48f));canvas.DrawRect(new Rect2(o+new Vector2(4,8),new Vector2(size-8,size-12)),p.cargo.Darkened(.2f));canvas.DrawRect(new Rect2(o+new Vector2(4,4),new Vector2(size-8,size-18)),p.cargo.Lightened(.22f));canvas.DrawString(ThemeDB.FallbackFont,o+new Vector2(22,38),"▲",fontSize:22,modulate:new Color("fff0bd"));}
  }
 }
}
