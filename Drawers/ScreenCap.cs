
using System.Diagnostics;

namespace Roguelike;

[DebuggerDisplay("{width}:{height}")]
public static partial class ScreenCap
{
    private static int width, height;

    public static bool IsResized =>
        width != Console.WindowWidth ||
        height != Console.WindowHeight;

    private static Region messages, map_view, player_stats, status_row;

    public static Region MessageLog
    {
        [DebuggerStepThrough] get => messages;
    }

    public static Region View
    {
        [DebuggerStepThrough] get => map_view;
    }

    public static Region Stats
    {
        [DebuggerStepThrough] get => player_stats;
    }

    public static Region Status
    {
        [DebuggerStepThrough] get => status_row;
    }

    [DebuggerStepThrough]
    static ScreenCap()
    {
        (messages, map_view, player_stats, status_row) = CalcRegions();

        messages.Clear();
        player_stats.Clear();
        map_view.Clear();
        status_row.Clear();
    }

    public static void Resize()
    {
        if (IsResized)
            return;

        (messages,map_view,  player_stats, status_row) = CalcRegions();

        messages.PendingUpdate = true;
        player_stats.PendingUpdate = true;
        map_view.PendingUpdate = true;
        status_row.PendingUpdate = true;
    }

    private static (Region, Region, Region, Region) CalcRegions()
    {
        Console.CursorVisible = false;

        width = Console.WindowWidth;
        height = Console.WindowHeight;

        int messages_height = Math.Max(2, height / 10);

        int stats_width = Math.Max(15, width / 8);
        int stats_left = width - stats_width;

        int status_top = height - 2;

        return (/*Log    */ new(0, 0, stats_left - 1, messages_height),
                /*View   */ new(0, messages_height + 1, stats_left - 1, status_top - 1),
                /*Stats  */ new(stats_left, 0, width, status_top - 1),
                /*Status */ new(0, status_top, width - 1, height - 1));
    }
}
