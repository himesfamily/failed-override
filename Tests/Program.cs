using FailedOverride.Battle;
using FailedOverride.Units;

static class Test
{
    static int failures;

    public static void Run(string name, Action test)
    {
        try { test(); Console.WriteLine($"PASS {name}"); }
        catch (Exception ex) { failures++; Console.WriteLine($"FAIL {name}: {ex.Message}"); }
    }

    public static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new Exception($"expected {expected}, got {actual}");
    }

    public static void True(bool value, string message)
    {
        if (!value) throw new Exception(message);
    }

    public static int ExitCode => failures == 0 ? 0 : 1;
}

static class Program
{
    public static int Main()
    {
        Test.Run("battle_constructs_all_three_missions", () =>
        {
            for (var mission = 1; mission <= 3; mission++)
            {
                var battle = new BattleState(mission);
                Test.Equal(12, battle.Map.Length);
                Test.True(battle.Map.All(row => row.Length == 12), $"mission {mission} width");
                Test.True(battle.Units.Any(unit => unit.Team == Team.Player), $"mission {mission} player spawn");
            }
        });

        return Test.ExitCode;
    }
}
