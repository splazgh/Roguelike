
using static System.ConsoleKey;

namespace Roguelike;

internal static class Player
{
    // Player placement
    public static int Depth = 0;
    public static int X, Y;

    public static (int dX, int dY) Offset { get; set; } = (0, 0);

    // Character properties
    public static Stats Stats { get; private set; } = new();
    public static Equipment Equip { get; private set; } = new();
    public static uint Resurrections { get; private set; } = 0;

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

    public static void RunTimedEvents(int elapsed)
    {
    }

    public static bool StopTimedEvents()
    {
        return false;
    }

    public static char ProcessKey(ConsoleKeyInfo key_info, char prefix)
    {
        switch (key_info.Key)
        {
            case Spacebar or Enter:
                StopTimedEvents();
                break;

            case LeftArrow or RightArrow or UpArrow or DownArrow or Home or PageUp or End or PageDown
                or NumPad1 or NumPad2 or NumPad3 or NumPad4 or NumPad6 or NumPad7 or NumPad8 or NumPad9:

                if (StopTimedEvents())
                    break;

                switch (key_info.Key)
                {
                    case LeftArrow or NumPad4:
                        Action(X - 1, Y, prefix);
                        break;

                    case RightArrow or NumPad6:
                        Action(X + 1, Y, prefix);
                        break;

                    case UpArrow or NumPad8:
                        Action(X, Y - 1, prefix);
                        break;

                    case DownArrow or NumPad2:
                        Action(X, Y + 1, prefix);
                        break;

                    case Home or NumPad7:
                        Action(X - 1, Y - 1, prefix);
                        break;

                    case PageUp or NumPad9:
                        Action(X + 1, Y - 1, prefix);
                        break;

                    case End or NumPad1:
                        Action(X - 1, Y + 1, prefix);
                        break;

                    case PageDown or NumPad3:
                        Action(X + 1, Y + 1, prefix);
                        break;
                }

                break;

            case D:
                Journal.Log.AddSignified("You don't have a pick.");
                break;

            case Oem2:
                Journal.Log.AddSignified("Which direction?");
                return '/';

            default:
                break;
        }

        return '\0';
    }

    private static readonly char[] vowels = new char[] { 'a', 'e', 'y', 'u', 'i', 'o' };

    private static void Action(int x, int y, char type)
    {
        var view = ScreenCap.View;
        var map = Levels.Data[Depth];

        if (!map.FullMap.Contains(x, y))
            return;

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
                        break;

                    case '\\':
                        obj.Update('+');
                        obj.DrawTo(view);

                        Journal.Log.AddNormal("You close the door.");
                        break;
                }

                return;
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

            return;
        }

        map.DrawMapPoint(view, X, Y);
        (X, Y) = (x, y);

        DrawTo(view);

        if (obj?.CanPass is true)
        {
            string article = obj.Name.IndexOfAny(vowels) == 0
                ? "an"
                : "a";

            Journal.Log.AddNormal($"You see {article} {obj.Name}.");
            return;
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
