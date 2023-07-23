
using System.Diagnostics;

namespace Roguelike;

[DebuggerDisplay("{Count}d{Rank}")]
internal class Dice(int count, int rank)
{
    private static readonly Random RNG = new((int)(DateTime.Now.Ticks % (65536 * 2)));

    public static Dice Zero = Create("1d1");

    public readonly int Count = count;
    public readonly int Rank = rank;

    public static Dice Create(string template)
    {
        int dice_count = 1, dice_rank = 1;

        string[] values = template.Split('d', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (values.Length is 2
            && int.TryParse(values[0], out int count_value))
        {
            dice_count = Math.Max(1, Math.Min(count_value, 8));
        }

        if (values.Length is 1 or 2
            && int.TryParse(values[^1], out int rank_value))
        {
            dice_rank = Math.Max(1, Math.Min(rank_value, 27));
        }

        return new Dice(dice_count, dice_rank);
    }

    public int Throw()
    {
        int result = 0;

        for (int i = Count; i > 0; i--)
        {
            result += 1 + RNG.Next(Rank);
        }

        return result;
    }

    public override string ToString()
    {
        return $"{Count}d{Rank}";
    }
}
