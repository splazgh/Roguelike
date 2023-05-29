
using static System.ConsoleKey;

namespace Roguelike;

internal static class Player
{
    public const int ViewRange = 5;

    // Player placement
    public static int Depth = 0;
    public static int X, Y;

    public static (int dX, int dY) Offset { get; set; } = (0, 0);

    // Character properties
    public static Stats Stats { get; private set; } = new();
    public static Equipment Equip { get; private set; } = new();
    public static uint Resurrections { get; private set; } = 0;

    public static readonly List<ITimedEvent> timed_events = new();

    internal static string GetState()
    {
        string result = $"Resurrections: {Resurrections}    ";

        if (Equip.TwoHanded)
        {
            result += $"Hands: {Equip.LeftHand?.Damage ?? Dice.Zero}  ";
        }
        else
        {
            result += $"Left Hand: {Equip.LeftHand?.Damage ?? Dice.Zero}  ";
            result += $"Right Hand: {Equip.RightHand?.Damage ?? Dice.Zero}  ";
        }

        result += $"    Depth: {Depth * 50} m  ";

        return result;
    }

    public static void AddTimedEvent(ITimedEvent tool)
    {
        if (tool.IsStopped)
            return;

        timed_events.Add(tool);
    }

    private static int aging_timesync = 0;

    public static void RunTimedEvents(int elapsed)
    {
        timed_events.ForEach(t => t.OnTimedAction(elapsed));

        for (int i = timed_events.Count - 1; i >= 0; i--)
        {
            if (timed_events[i].IsStopped)
                timed_events.RemoveAt(i);
        }

        if ((aging_timesync += elapsed) >= 50)
        {
            Levels.Map.AgeScents(aging_timesync / 50);
            aging_timesync %= 50;
        }
    }

    public static bool StopTimedEvents()
    {
        bool result = timed_events.Any(t => !t.IsStopped);

        timed_events.Clear();

        return result;
    }

    public static char ProcessKey(ConsoleKeyInfo key_info, char prefix)
    {
        bool successful_action = true;

        switch (key_info.Key)
        {
            case Spacebar or Enter:
                StopTimedEvents();
                break;

            case LeftArrow or RightArrow or UpArrow or DownArrow or Home or PageUp or End or PageDown
                or NumPad1 or NumPad2 or NumPad3 or NumPad4 or NumPad6 or NumPad7 or NumPad8 or NumPad9:

                if (StopTimedEvents())
                    break;

                successful_action = false;

                switch (key_info.Key)
                {
                    case LeftArrow or NumPad4:
                        successful_action = ActionAt(X - 1, Y, prefix);
                        break;

                    case RightArrow or NumPad6:
                        successful_action = ActionAt(X + 1, Y, prefix);
                        break;

                    case UpArrow or NumPad8:
                        successful_action = ActionAt(X, Y - 1, prefix);
                        break;

                    case DownArrow or NumPad2:
                        successful_action = ActionAt(X, Y + 1, prefix);
                        break;

                    case Home or NumPad7:
                        successful_action = ActionAt(X - 1, Y - 1, prefix);
                        break;

                    case PageUp or NumPad9:
                        successful_action = ActionAt(X + 1, Y - 1, prefix);
                        break;

                    case End or NumPad1:
                        successful_action = ActionAt(X - 1, Y + 1, prefix);
                        break;

                    case PageDown or NumPad3:
                        successful_action = ActionAt(X + 1, Y + 1, prefix);
                        break;
                }

                break;

            case D:
                Journal.Log.AddNormal("You don't have a pick.");
                break;

            case OemPeriod:
                Journal.Log.AddNormal("Which direction?");
                return '.';

            case Oem2:
                Journal.Log.AddNormal("Which direction?");
                return '/';

            default:
                break;
        }

        if (!successful_action)
        {
            RunTimedEvents(50);   // constant speed
        }

        return '\0';
    }

    private static readonly char[] vowels = new char[] { 'a', 'e', 'y', 'u', 'i', 'o' };

    public static bool ActionAt(int x, int y, char type = '\0')
    {
        bool result = false;

        var view = ScreenCap.View;
        var map = Levels.Data[Depth];

        try
        {
            if (!map.FullMap.Contains(x, y))
                return false;

            if (map.TryGetMonster(x, y, out _))
            {
                Journal.Log.AddNormal("You can't pass through the foe.");
                return false;
            }

            if (map.TryGetObject(x, y, out MapObject? obj))
            {
                if (type is not '\0'
                    && type == obj.KeyAction)
                {
                    switch (obj.Type)
                    {
                        case '+':
                            obj.Update('\\');
                            obj.DrawTo(view);

                            Journal.Log.AddNormal("You open the door.");
                            return result = true;

                        case '\\':
                            obj.Update('+');
                            obj.DrawTo(view);

                            Journal.Log.AddNormal("You close the door.");
                            return result = true;
                    }

                    return false;
                }
            }
            else
                obj = null;

            if (obj?.CanPass is false)
            {
                if (obj.CanSwim)
                    Journal.Log.AddNormal($"You don't know how to swim.");
                else
                    Journal.Log.AddNormal($"You stuck at the {obj.Name}.");

                return false;
            }

            if (type == '.')
            {
                AddTimedEvent(new RunTool((x - X, y - Y), 50)); // constant speed
                return result = true;
            }

            map.AddFootprint(X, Y, 40, (x - X, y - Y)); // footprint with constant weight
            map.AddScent(X, Y, 40); // constant scent tail

            map.DrawMapPoint(view, X, Y);
            (X, Y) = (x, y);

            DrawTo(view);

            if (obj?.CanPass is true)
            {
                string article = obj.Name.IndexOfAny(vowels) == 0
                    ? "an"
                    : "a";

                Journal.Log.AddNormal($"You see {article} {obj.Name}.");
                return result = true;
            }

            return result = true;
        }
        finally
        {
            if (result)
                map.OpenFogOfWar(X, Y, ViewRange);
        }
    }

    public static void DrawTo(Region view)
    {
        Map map = Levels.Data[Depth];

        // check map coordinates
        if (!map.FullMap.Contains(X, Y))
            return;

        // calc and check view coordinates
        int viewX = X - Offset.dX, viewY = Y - Offset.dY;

        if (!view.Contains(viewX, viewY))
            return;

        // draw player
        view.WriteChar(viewX, viewY, '@', ConsoleColor.White);
    }
}
