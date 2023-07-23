
using System.Diagnostics;

namespace Roguelike;

[DebuggerDisplay("Turns: {Turns}")]
internal static class ActionsCore
{
    internal static long Turns = 0L;

    public static void Run()
    {
        ManualResetEventSlim event_timer = new(false);
        char prefix = '\0';

        int active_level = -1;

        while (true)
        {
            // global screen update action
            if (active_level != Player.Depth)
            {
                ScreenCap.MessageLog.PendingUpdate = true;

                Journal.Log.DrawTo(ScreenCap.MessageLog);
                Player.Map.DrawTo(ScreenCap.View);
                Player.Stats.DrawTo(ScreenCap.Stats);
                StatusRow.DrawTo(ScreenCap.Status);

                active_level = Player.Depth;
            }

            bool wasDepleted = Journal.Log.Depleted;

            // wait for key pressed actions
            if (Console.KeyAvailable || !wasDepleted)
            {
                if (!wasDepleted)
                    prefix = '\0';

                var key_info = Console.ReadKey(true);

                // process global actions
                switch (StatusRow.ProcessKey(key_info))
                {
                    // finish running
                    case StatusRow.GlobalAction.Exit:
                        return;

                    default:
                        break;
                }

                // process message log
                if (Journal.ProcessKey(key_info)
                    || wasDepleted)
                {
                    ScreenCap.MessageLog.Clear();
                }

                // process player actions
                if (wasDepleted)
                {
                    prefix = Player.ProcessKey(key_info, prefix);
                }
            }

            // wait for timed actions
            else
            {
                event_timer.Wait(50);        // constant speed
                Player.RunTimedEvents(50);   // constant speed
            }

            // show partial message log
            if (!Journal.Log.Depleted)
                Journal.Log.DrawTo(ScreenCap.MessageLog);
        }
    }
}
