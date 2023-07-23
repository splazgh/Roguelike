
namespace Roguelike;

internal static class MapObjects
{
    public static readonly Info[] Lib = new Info[]
    {
        new('>', '>', ConsoleColor.Gray,       "stairs leading down"),
        new('<', '<', ConsoleColor.Gray,       "stairs leading up"),
        new('"', 'p', ConsoleColor.Green,      "herbs"),
        new('≈','\0', ConsoleColor.DarkBlue,   "flowing water"),
        new('+', '/', ConsoleColor.DarkYellow, "closed door"),
        new('\\','/', ConsoleColor.DarkYellow, "open door"),
        new('_', 'u', ConsoleColor.DarkRed,    "altar"),
        new('&', 'u', ConsoleColor.DarkGray,   "forge"),
        new('#', 'd', ConsoleColor.DarkGray,   "rock")
    };

    public static bool Unknown(char c) => GetIndex(c) < 0;

    public static bool IsStoppable(char c)
    {
        return c is '≈' or '+' or '#';
    }

    public static bool IsLiquid(char c)
    {
        return c is '≈';
    }

    public static int GetIndex(char c)
    {
        int idx = -1;

        while (++idx < Lib.Length)
        {
            if (Lib[idx].MapChar == c)
                return idx;
        }

        return -1;
    }

    public static char GetKey(char c)
    {
        int idx = -1;

        while (++idx < Lib.Length)
        {
            if (Lib[idx].MapChar == c)
                return Lib[idx].Key;
        }

        return '\0';
    }

    public static ConsoleColor GetColor(char c)
    {
        int idx = -1;

        while (++idx < Lib.Length)
        {
            if (Lib[idx].MapChar == c)
                return Lib[idx].Color;
        }

        return ConsoleColor.Gray;
    }

    public static string GetName(char c)
    {
        int idx = -1;

        while (++idx < Lib.Length)
        {
            if (Lib[idx].MapChar == c)
                return Lib[idx].Name;
        }

        return string.Empty;
    }

    internal readonly struct Info(char mapChar, char key, ConsoleColor color, string name)
    {
        public readonly char MapChar = mapChar;
        public readonly char Key = key;
        public readonly ConsoleColor Color = color;
        public readonly string Name = name;
    }
}
