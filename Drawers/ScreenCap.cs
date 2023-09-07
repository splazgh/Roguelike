
using System.Diagnostics;
using System.Timers;

namespace Roguelike;

[DebuggerDisplay("{width}:{height}")]
public static partial class ScreenCap
{
    private static int width, height;
    private static readonly System.Timers.Timer resizeTimer;

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

    static ScreenCap()
    {
        (messages, map_view, player_stats, status_row) = CalcRegions();

        messages.Clear();
        player_stats.Clear();
        map_view.Clear();
        status_row.Clear();

        resizeTimer = new(100)
        {
            AutoReset = true,
            Enabled = true
        };

        resizeTimer.Elapsed += OnResize;
    }

    private static void OnResize(object? sender, ElapsedEventArgs e)
    {
        if (!IsResized)
            return;

        lock (status_row)
        {
            (messages, map_view, player_stats, status_row) = CalcRegions();
            
            ((IConsoleDrawer)Journal.Log).OnResize(messages);

            ((IConsoleDrawer)Player.Stats).OnResize(player_stats);
            ((IConsoleDrawer)Player.Map).OnResize(map_view);

            StatusRow.OnResize(status_row);
        }
    }

    private static (Region, Region, Region, Region) CalcRegions()
    {
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Gray;

        Console.Clear();

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
