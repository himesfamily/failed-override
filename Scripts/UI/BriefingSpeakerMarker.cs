using Godot;

namespace FailedOverride.UI;

public partial class BriefingSpeakerMarker : Control
{
    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetProcess(true);
    }

    public override void _Process(double delta) => QueueRedraw();

    public override void _Draw()
    {
        var pulse = .78f + .22f * (float)((Math.Sin(Time.GetTicksMsec() / 260.0) + 1) / 2);
        var color = new Color(.40f, .91f, .96f, pulse);
        var glow = new Color(.40f, .91f, .96f, .20f * pulse);
        const float haloBottomFraction = .96f;
        var haloTopFraction = BriefingSpeakerFocusLayout.MarkerHaloTopFraction;
        var centerYFraction = (haloTopFraction + haloBottomFraction) / 2f;
        var radiusYFraction = (haloBottomFraction - haloTopFraction) / 2f;
        var center = new Vector2(Size.X * .5f, Size.Y * centerYFraction);
        var radius = new Vector2(Size.X * .43f, Size.Y * radiusYFraction);
        var ellipse = new Vector2[65];
        for (var index = 0; index <= 64; index++)
        {
            var angle = Mathf.Tau * index / 64f;
            ellipse[index] = center + new Vector2(Mathf.Cos(angle) * radius.X, Mathf.Sin(angle) * radius.Y);
        }
        DrawPolyline(ellipse, glow, 12f, true);
        DrawPolyline(ellipse, color, 4f, true);

        var tip = new Vector2(center.X, center.Y - radius.Y - 3f);
        Vector2[] pointer =
        [
            tip,
            tip + new Vector2(-13f, -22f),
            tip + new Vector2(13f, -22f),
        ];
        DrawColoredPolygon(pointer, color);
    }
}
