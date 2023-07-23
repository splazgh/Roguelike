
using System.Diagnostics;

namespace Roguelike;

[DebuggerDisplay("{Left,Top,Right,Bottom} : {cX},{cY}")]
public record Region(int X1, int Y1, int X2, int Y2)
{
    private readonly string empty_line = new(' ', Math.Max(0, X2 - X1));

    private int cX = 0, cY = 0;

    internal readonly int Left = X1, Top = Y1, Right = X2, Bottom = Y2;

    public bool PendingUpdate = false;

    public int Width => Math.Max(0, X2 - X1);
    public int Height => Math.Max(0, Y2 - Y1);

    public char[] Line => empty_line.ToCharArray();

    public bool WriteLock => cY >= Height
        || (cY + 1 == Height && cX >= Width);

    public bool Contains(int X, int Y)
    {
        return X >= 0 && X < Width && Y >= 0 && Y < Height;
    }

    public bool Contains((int X, int Y) t)
    {
        return t.X >= 0 && t.X < Width && t.Y >= 0 && t.Y < Height;
    }

    internal void WriteChar(int x, int y, char c)
    {
        if (!Contains(x, y))
            return;

        Console.SetCursorPosition(Left + x, Top + y);
        Console.Write(c);
    }

    internal void WriteChar(int x, int y, char c, ConsoleColor fg)
    {
        if (!Contains(x, y))
            return;

        var backup_color = Console.ForegroundColor;
        Console.ForegroundColor = fg;

        Console.SetCursorPosition(Left + x, Top + y);
        Console.Write(c);

        Console.ForegroundColor = backup_color;
    }

    public void SetCursorPosition(int X, int Y)
    {
        cX = X;
        cY = Y;
    }

    public void Write(string line)
    {
        if (WriteLock)
            return;

        Console.SetCursorPosition(Left + cX, Top + cY);
        Console.Write(line);

        cX = Math.Min(Width, cX + line.Length);
    }

    public void Write(string line, ConsoleColor fg)
    {
        if (WriteLock)
            return;

        var backup_color = Console.ForegroundColor;
        Console.ForegroundColor = fg;

        Console.SetCursorPosition(Left + cX, Top + cY);
        Console.Write(line);

        Console.ForegroundColor = backup_color;

        cX = Math.Min(Width, cX + line.Length);
    }

    public void WriteLine(string line)
    {
        if (WriteLock)
            return;

        Console.SetCursorPosition(Left + cX, Top + cY);
        Console.Write(line);

        cX = 0;
        cY++;
    }

    internal void MoreTipText()
    {
        if (!WriteLock)
            return;

        Console.SetCursorPosition(Right - 7, Bottom - 1);
        Console.Write("(space)");
    }

    internal void WriteTipText(string text)
    {
        Console.SetCursorPosition(Right - text.Length - 1, Bottom - 1);
        Console.Write(text);
    }

    public void Clear()
    {
        for (int i = Top; i <= Bottom; i++)
        {
            Console.SetCursorPosition(Left, i);
            Console.Write(empty_line);
        }

        cX = 0; cY = 0;
    }
}
