
using System.Diagnostics;

namespace Roguelike;

internal static class Levels
{
    private const int maxLevels = 1;

    public static Dictionary<int, Map> Data = new(maxLevels);

    public static Map Map
    {
        [DebuggerStepThrough]
        get
        {
            return Data[Player.Level];
        }
    }

    static Levels()
    {
        Map first = Data[Player.Level] = new(100, 100);

        first.Paste(MapTemplates.SmallRoom, 5, 5);
        first.Paste(MapTemplates.LongWayToEast, 15, 7);
        first.Paste(MapTemplates.LargeRoom, 57, 6);

        (Player.X, Player.Y) = (11, 9);
    }
}
