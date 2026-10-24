namespace FailedOverride.Battle;

public enum TerrainKind { Floor, Wall, Pillar, LowCover, Elevated, Objective }

public readonly record struct TerrainCell(TerrainKind Kind)
{
    public bool Walkable => Kind is TerrainKind.Floor or TerrainKind.Elevated or TerrainKind.Objective;
    public bool BlocksSight => Kind is TerrainKind.Wall or TerrainKind.Pillar;
    public int Elevation => Kind == TerrainKind.Elevated ? 1 : 0;
}
