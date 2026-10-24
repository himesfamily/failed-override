using FailedOverride.Units;

namespace FailedOverride.Battle;

public sealed record UnitSpawn(char Token, Cell Position);

public sealed class MissionDefinition
{
    public int Number { get; }
    public TerrainCell[,] Terrain { get; }
    public IReadOnlyList<UnitSpawn> Spawns { get; }
    public string[] UnitRows { get; }
    public Cell? Console { get; }
    public Cell? Shuttle { get; }
    public Cell? Bridge { get; }

    MissionDefinition(int number, string[] terrainRows, string[] unitRows, Cell? console = null, Cell? shuttle = null, Cell? bridge = null)
    {
        Number = number; UnitRows = unitRows; Console = console; Shuttle = shuttle; Bridge = bridge;
        if (terrainRows.Length != 12 || terrainRows.Any(row => row.Length != 12)) throw new ArgumentException("Mission terrain must be 12x12.");
        if (unitRows.Length != 12 || unitRows.Any(row => row.Length != 12)) throw new ArgumentException("Mission spawns must be 12x12.");
        Terrain = new TerrainCell[12, 12];
        var spawns = new List<UnitSpawn>();
        for (var y = 0; y < 12; y++) for (var x = 0; x < 12; x++)
        {
            Terrain[x, y] = new TerrainCell(terrainRows[y][x] switch
            {
                '#' => TerrainKind.Wall, 'P' => TerrainKind.Pillar, 'c' => TerrainKind.LowCover,
                '^' => TerrainKind.Elevated, 'O' => TerrainKind.Objective, _ => TerrainKind.Floor
            });
            if (unitRows[y][x] != '.') spawns.Add(new UnitSpawn(unitRows[y][x], new Cell(x, y)));
        }
        Spawns = spawns;
    }

    public TerrainCell At(Cell cell) => cell.X is >= 0 and < 12 && cell.Y is >= 0 and < 12
        ? Terrain[cell.X, cell.Y] : new TerrainCell(TerrainKind.Wall);

    public static MissionDefinition For(int mission) => mission switch
    {
        1 => new(1,
        ["############","#...#O.....#","#.#.#...P..#","#.#.....#..#","#c##..O.##.#","#....##....#","##.#.^^.#..#","#..#....#..#","#..c..#....#","#.#...#..#.#","#..........#","############"],
        ["............",".....I.D....","............",".....D......","............","............","............",".....D......","............","............",".XNA........","............"], console: new(6,4)),
        2 => new(2,
        ["############","#.........O#","#..PP......#","#....^.....#","#.cc##.ccc.#","#......#...#","#P^.##.#.#.#","#...#......#","#c#.#.###..#","#..........#","#..........#","############"],
        ["............","............","............","...D.H......","............","............","............",".......D....","............",".XNAI.......","............","............"], shuttle: new(10,1)),
        3 => new(3,
        ["############","#........O.#","#..^..#....#","#..c..#....#","#.....###..#","#..........#","#.cc##.cc#.#","#..........#","#..^.......#","#..........#","#..........#","############"],
        ["............","............","............","........K...",".M..........",".......Y....","............",".V..........",".......XNAI.",".....C......","............","............"], bridge: new(9,1)),
        _ => throw new ArgumentOutOfRangeException(nameof(mission))
    };
}
