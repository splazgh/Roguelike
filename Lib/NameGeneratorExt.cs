
using static Roguelike.Monsters;

namespace Roguelike;

internal static class NameGeneratorExt
{
    private static readonly Random RNG = new((int)DateTime.Now.TimeOfDay.TotalMicroseconds);

    public static string GenerateName(this NameGenerator gen, int naming_threshold)
    {
        if (RNG.Next(100) > naming_threshold)
            return string.Empty;

        return gen switch
        {
            NameGenerator.Goblin => GetGoblin(),
            NameGenerator.Orc => GetOrc(),
            NameGenerator.Human => GetHuman(),
            NameGenerator.Outlaw => GetOutlaw(),
            _ => string.Empty
        };  
    }

    private static readonly string[] goblins =
    {
        "bI", "O", "Au", "Ba", "Ga", "Do", "Ra"
    };

    private static string GetGoblin()
    {
        int n = RNG.Next(goblins.Length);

        return goblins[n];
    }

    private static readonly List<string> orcs = new()
    {
        "Grunt", "Crusher", "Club", "Soul", "Fluffy", "Dick Pig"
    };

    private static string GetOrc()
    {
        if (!orcs.Any())
            return string.Empty;

        int n = RNG.Next(orcs.Count);
        string name = orcs[n];
        orcs.RemoveAt(n);

        return name;
    }

    private static readonly string[] names = new[]
    {
        "Jhon", "Ivan", "Boris", "Federico", "Pedro", "Alexander", "Andrew"
    };

    private static readonly string[] families = new[]
    {
        "Malkovich", "Tobolich", "Kowalsky", "Smith", "Petroff", "Petrovich"
    };

    private static string GetHuman()
    {
        int n = RNG.Next(names.Length);
        int f = RNG.Next(families.Length);

        return $"{names[n]} {families[f]}";
    }

    private static readonly List<string> outlaws = new()
    {
        "Joker", "Judge", "Procrust", "Bunny", "Ankudo", "Theeth", "Mickey"
    };

    private static string GetOutlaw()
    {
        if (!outlaws.Any() || (RNG.Next(6) < 5 & RNG.Next(6) < 5 & RNG.Next(6) < 5))
            return GetHuman();

        int n = RNG.Next(outlaws.Count);
        string name = outlaws[n];
        outlaws.RemoveAt(n);

        return name;
    }
}
