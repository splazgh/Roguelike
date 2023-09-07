
using System.Diagnostics;

namespace Roguelike;

internal class Stats : IConsoleDrawer
{
    private static readonly List<Stat> stats = new()
    {
        new() { Name = "Strength", ALPHA2 = "ST", ALPHA3 = "STR" },
        new() { Name = "Dexterity", ALPHA2 = "DX", ALPHA3 = "DEX" },
        new() { Name = "Constitution", ALPHA2 = "CN", ALPHA3 = "CON" },
        new() { Name = "Intellect", ALPHA2 = "IN", ALPHA3 = "INT" },
        new() { Name = "Wisdom", ALPHA2 = "WS", ALPHA3 = "WIS" },
        new() { Name = "Charisma", ALPHA2 = "CH", ALPHA3 = "CHA" }
    };

    private readonly Dictionary<string, Stat> stats_indexer = new();
    private readonly Dictionary<string, DynamicStats> dynamics = new();

    public DynamicStats this[string Key]
    {
        get => dynamics[Key];

        set
        {
            foreach (var item in dynamics.Values)
                item.Bottom++;

            value.Bottom = 1;

            dynamics[Key] = value;
        }
    }

    // About player
    public string Name { get; set; } = string.Empty;
    public string Race { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;

    // Character stats
    public Stats()
    {
        foreach (var item in stats)
        {
            stats_indexer[item.ALPHA2] = item;
            stats_indexer[item.ALPHA3] = item;
        }
    }

    public void DrawTo(Region region)
    {
        if (ScreenCap.IsResized)
            return;

        region.SetCursorPosition(0, 1);

        region.WriteLine(Name);

        region.SetCursorPosition(0, 3);

        region.WriteLine(Race);
        region.WriteLine(Class);

        Stat.ShortStat = region.Width < 10;

        region.SetCursorPosition(0, 6);

        foreach (var item in stats)
        {
            var full_row = region.Line;

            var stat = item.ToString().ToCharArray();

            if (stat.Length > full_row.Length)
                stat = stat[..full_row.Length];

            stat.CopyTo(full_row, 0);

            region.WriteLine(new(full_row));
        }

        foreach (var d in dynamics.Values)
            d.DrawTo(region);
    }

    [DebuggerDisplay("{Name} : {Value}")]
    internal record Stat
    {
        public static bool? ShortStat = false;

        public required string Name { get; init; }
        public required string ALPHA2 { get; init; }
        public required string ALPHA3 { get; init; }

        private int _value = 1;

        public int Value
        {
            get => _value;

            set
            {
                int actual_value = Math.Max(1, value);

                if (_value == actual_value)
                    return;

                _value = actual_value;

                Player.Stats.DrawTo(ScreenCap.Stats);
            }
        }

        public override string ToString()
        {
            return ShortStat is false
                ? $"{ALPHA3} : {Value}"
                : $"{ALPHA2} {Value}";
        }
    }
}
