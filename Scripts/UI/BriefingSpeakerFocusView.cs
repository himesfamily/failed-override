using Godot;
namespace FailedOverride.UI;
public partial class BriefingSpeakerFocusView : Control
{
 const float FadeSeconds=.18f;readonly BriefingSpeakerPortraitTransitionState state=new();readonly TextureRect[] layers=new TextureRect[2];Tween? focusTween;TextureRect hologramOutline=null!;
 public override void _Ready(){MouseFilter=MouseFilterEnum.Ignore;ZIndex=BriefingSpeakerFocusLayout.FocusZIndex;Position=Vector2.Zero;Size=new(1400,1070);for(var i=0;i<layers.Length;i++){layers[i]=new(){ExpandMode=TextureRect.ExpandModeEnum.IgnoreSize,StretchMode=TextureRect.StretchModeEnum.KeepAspectCentered,MouseFilter=MouseFilterEnum.Ignore,Modulate=new Color(1,1,1,0),Visible=false};AddChild(layers[i]);}hologramOutline=new(){Texture=GD.Load<Texture2D>(BriefingSpeakerFocusLayout.HologramOutlineResource),Position=new(520,125),Size=new(360,360),ExpandMode=TextureRect.ExpandModeEnum.IgnoreSize,StretchMode=TextureRect.StretchModeEnum.KeepAspectCentered,MouseFilter=MouseFilterEnum.Ignore,Visible=false};AddChild(hologramOutline);}
 static readonly Shader PortraitGlow = new()
 {
     Code = """
         shader_type canvas_item;
         varying vec4 tint;
         const float padding = 0.026;
         void vertex() {
             tint = COLOR;
             VERTEX += (UV * 2.0 - 1.0) * vec2(11.0, 14.0);
         }
         vec4 portrait_sample(sampler2D image, vec2 uv) {
             vec2 inside = step(vec2(0.0), uv) * step(uv, vec2(1.0));
             return texture(image, clamp(uv, vec2(0.0), vec2(1.0))) * inside.x * inside.y;
         }
         void fragment() {
             vec2 uv = (UV - 0.5) * (1.0 + 2.0 * padding) + 0.5;
             vec4 portrait = portrait_sample(TEXTURE, uv);
             float rim = 0.0;
             float halo = 0.0;
             for (int i = 0; i < 12; i++) {
                 float angle = TAU * float(i) / 12.0;
                 vec2 direction = vec2(cos(angle), sin(angle));
                 rim = max(rim, portrait_sample(TEXTURE, uv + direction * 0.008).a);
                 halo += portrait_sample(TEXTURE, uv + direction * 0.022).a / 12.0;
             }
             float glow = max(rim * 0.92, halo * 0.8) * (1.0 - portrait.a);
             float alpha = portrait.a + glow;
             vec3 color = (portrait.rgb * portrait.a + vec3(0.32, 0.88, 1.0) * glow) / max(alpha, 0.001);
             COLOR = vec4(color, alpha) * tint;
         }
         """
 };
 ShaderMaterial? portraitGlowMaterial;
 public void ShowSpeaker(string portraitKey,TextureRect? hologram){var transition=state.Select(portraitKey);focusTween?.Kill();focusTween=null;Sync();if(transition.HologramFocused){hologramOutline.Visible=hologram?.Visible==true;return;}hologramOutline.Visible=false;if(transition.CurrentKey is null||transition.CurrentLayer is not int currentIndex)return;if(transition.ReusesCurrentLayer){layers[currentIndex].Visible=true;layers[currentIndex].Modulate=Colors.White;return;}var portrait=BriefingSpeakerPortraitLayout.For(transition.CurrentKey);var frame=BriefingSpeakerPortraitLayout.DisplayFrameFor(transition.CurrentKey);if(portrait is null||frame is null)return;var next=layers[currentIndex];next.Texture=GD.Load<Texture2D>(portrait.Resource);next.Material=transition.CurrentKey=="chrandy"?null:portraitGlowMaterial??=new ShaderMaterial{Shader=PortraitGlow};next.Position=new(frame.Value.X,frame.Value.Y);next.Size=new(frame.Value.Width,frame.Value.Height);next.Visible=true;next.Modulate=new Color(1,1,1,0);var previous=transition.PreviousLayer is int previousIndex?layers[previousIndex]:null;focusTween=CreateTween().SetParallel();if(previous?.Visible==true)focusTween.TweenProperty(previous,"modulate:a",0f,FadeSeconds);focusTween.TweenProperty(next,"modulate:a",1f,FadeSeconds);focusTween.Chain().TweenCallback(Callable.From(()=>{state.Complete(transition);Sync();}));}
 public void Clear(TextureRect? hologram=null){state.Clear();focusTween?.Kill();focusTween=null;foreach(var layer in layers){if(layer is null)continue;layer.Visible=false;layer.Modulate=new Color(1,1,1,0);}hologramOutline.Visible=false;}
 void Sync(){for(var i=0;i<layers.Length;i++){layers[i].Visible=state.LayerVisible(i);if(!layers[i].Visible)layers[i].Modulate=new Color(1,1,1,0);}}
}
