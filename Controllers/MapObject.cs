
using static Roguelike.MapObjectsCollection;

namespace Roguelike;

internal class MapObject(char c, int x, int y) : IConsoleDrawer
{
    public char Type = c;
    public readonly int X = x, Y = y;

    public char KeyAction = GetKey(c);
    public ConsoleColor Color = GetColor(c);
    public string Name = GetName(c);

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
        Map map = Levels.Map;

        // check map coordinates
        if (!map.FullMap.Contains(Coordinates))
            return;

        // calc and check view coordinates
        int viewX = X - map.Offset.dX, viewY = Y - map.Offset.dY;

        if (!view.Contains(viewX, viewY))
            return;

        // draw player
        view.WriteChar(viewX, viewY, Type, Color);
    }
}
