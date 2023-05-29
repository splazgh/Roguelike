
using System.Diagnostics;

using static Roguelike.Monsters.NameGenerator;

namespace Roguelike;

internal static class Monsters
{
    public static readonly Info[] Lib = new Info[]
    {
        new('r', "1d1", "rat",          2,  0,   1,   0, 50, null,   ConsoleColor.DarkYellow, new[] { "1d1" }),
        new('r', "2d1", "gray rat",     5,  0,   4,   1, 60, null,   ConsoleColor.Gray,       new[] { "1d2", "1d2" }),

        new('g', "1d2", "goblin",       5,  0,   2,   0, 90, null,   ConsoleColor.DarkGreen,  new[] { "1d1" }),
        new('h', "3d2", "hobgoblin",   15,  0,   8,   2, 80, Goblin, ConsoleColor.Yellow,     new[] { "1d4", "1d2" }),
        new('o', "3d3", "orc",         23,  0,  20,   4, 50, Orc,    ConsoleColor.DarkYellow, new[] { "2d4", "1d4" }),

        new('h', "5d3", "warrior",     40,  0,  25,   6, 50, Human,  ConsoleColor.Gray,       new[] { "1d8" }),
        new('h', "5d5", "mercenary",   60,  0,  30,  10, 75, Outlaw, ConsoleColor.DarkGray,   new[] { "2d8" })
    };

    private static readonly Random RNG = new((int)DateTime.Now.TimeOfDay.TotalSeconds);

    public static List<Info> Generate(int count, Dice complexity)
    {
        // init set complexity
        int threshold = complexity.Throw();

        // get allowed kinds
        threshold *= 2;
        Info[] allowed = Array.Empty<Info>();
            
        while (allowed.Length < 2)
        {
            threshold = (int)(threshold / 1.25);

            allowed = Lib
                .Where(m => m.Freq.Throw() > threshold)
                .ToArray();
        }

        // generate random set
        List<Info> result = new(count * 20);

        for (int i = 0; i < count * 20; i++)
            result.Add(allowed[RNG.Next(allowed.Length)]);

        // get monsters with less complexity
        var normal = result
            .OrderBy(m => m.Freq.Throw())
            .Take((int)(count * 0.7))
            .ToList();

        int filter = normal.Last().Freq.Throw();

        result = result
            .Where(m => m.Freq.Throw() > filter)
            .Take((int)(count * 0.3))
            .Concat(normal)
            .ToList();

        // naming processed after taking, cause of opponent's unique naming
        // in some cases
        return result
            .Select(m => m.Naming.HasValue
                ? m with { Name = m.Naming.Value.GenerateName(m.XP) }
                : m)
            .ToList();
    }

    public static void PlaceFoes(this Map map, int depth, int max_monsters)
    {
        var free_sites = map.GetFreeSites(true, false);
        int get_count = Math.Min((int)(free_sites.Count * 0.25), max_monsters);

        string complexity = $"{depth % 3}d{depth / 3}";

        var monsters = Generate(get_count, Dice.Create(complexity));

        for (int i = 0; i < monsters.Count; i++)
        {
            int idx = RNG.Next(free_sites.Count);
            var site_place = free_sites[idx];
            free_sites.RemoveAt(idx);

            Monster m = new(monsters[i], site_place.X, site_place.Y);
            map.AddMonster(site_place, m);
        }
    }

    public enum NameGenerator { Goblin, Orc, Human, Outlaw }

    [DebuggerDisplay("{(IsNamed ? Kind + \" named \\\"\" + Name + \"\\\"\" : Kind)}")]
    internal struct Info(char c, string frequency, string kind,
        int hit_points, int mana_points, int experience, int armor_class, int speed,
        NameGenerator? naming, ConsoleColor color, string[] attacks)
    {
        public readonly char Char = c;
        public readonly ConsoleColor Color = color;
        public readonly string Kind = kind;

        public string Name = string.Empty;
        public NameGenerator? Naming = naming;

        public readonly Dice Freq = Dice.Create(frequency);

        public readonly int HP =    Math.Max(0, hit_points);
        public readonly int MP =    Math.Max(0, mana_points);
        public readonly int XP =    Math.Max(0, experience);
        public readonly int AC =    Math.Max(0, armor_class);

        public readonly int Speed = Math.Max(1, speed);

        public readonly Dice[] Attacks = attacks.Select(Dice.Create).ToArray();

        public readonly bool IsNameCapable = naming.HasValue;
        public readonly bool IsNamed => !string.IsNullOrWhiteSpace(Name);
    }
}
