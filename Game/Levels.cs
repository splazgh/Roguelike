
using System.Diagnostics;

namespace Roguelike;

[DebuggerDisplay("Generated maps: {Data.Count}")]
internal static class Levels
{
    private const int maxLevels = 1;

    public static Dictionary<int, Map> Data = new(maxLevels);

    static Levels()
    {
        Map map = Data[Player.Depth] = new(100, 100);

        map.Paste(MapTemplates.SmallRoom, 5, 5);
        map.Paste(MapTemplates.LongWayToEast, 15, 7);
        map.Paste(MapTemplates.LargeRoom, 57, 6);

        Player.Stats["XP"] = new("XP", 0, 250, ConsoleColor.DarkGray);
        Player.Stats["HP"] = new("HP", 17, 18, ConsoleColor.DarkRed);
        Player.Stats["MP"] = new("MP", 4, 4, ConsoleColor.DarkBlue);

        Player.ActionTo(11, 9);

        int max_monsters = Math.Max(5, (int)(Player.Depth * Math.Pow(1.05, Player.Depth + 1)));
        map.PlaceFoes(Player.Depth, max_monsters);
    }
}
