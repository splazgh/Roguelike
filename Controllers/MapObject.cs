
using static Roguelike.MapObjects;

namespace Roguelike;

internal class MapObject(char type, int x, int y) : IConsoleDrawer
{
    public char Type = type;
    public readonly int X = x, Y = y;

    public char KeyAction = GetKey(type);
    public ConsoleColor Color = GetColor(type);
    public string Name = GetName(type);

    public (int, int) Coordinates => (X, Y);

    public bool CanPass => !IsStoppable(Type);
    public bool CanSwim => IsLiquid(Type);

    public void Update(char c)
    {
        Type = c;

        KeyAction = GetKey(c);
        Color = GetColor(c);
        Name = GetName(c);
    }

    public void DrawTo(Region view)
    {
        Map map = Player.Map;

        // check map coordinates
        if (!map.FullSize.Contains(Coordinates))
            return;

        // calc and check view coordinates
        int viewX = X - map.Offset.dX, viewY = Y - map.Offset.dY;

        if (!view.Contains(viewX, viewY))
            return;

        // draw player
        view.WriteChar(viewX, viewY, Type, Color);
    }
}
