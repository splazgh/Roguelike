
using System.Diagnostics;

namespace Roguelike;

[DebuggerDisplay("{(IsNamed ? Kind + \" named \"\"\" + Name + \"\"\"\" : Kind)} LVL: {Level}")]
internal class Monster(Monsters.Info info, int x, int y) : IConsoleDrawer
{
    // draw factor
    public readonly char Char = info.Char;
    public ConsoleColor Color = info.Color;

    // naming
    public string Kind = info.Kind;
    public string Name = info.Name;

    // placement
    public int Depth = Player.Depth;
    public int X = x, Y = y;

    // active properties
    public int HP = info.HP;
    public int MP = info.MP;
    public int XP = info.XP;
    public int AC = info.AC;

    public int Level = info.IsNamed ? 1 : 2;

    // timings
    public int TimeOffset = 0;
    public int Speed = info.Speed;

    // abilities
    public bool CanSee = true;
    public bool CanHear = true;
    public bool CanSpeak = info.IsNameCapable;
    public bool CanSwim = false;
    public bool CanFly = false;

    // items
    public Equipment Equip { get; init; } = new();

    public readonly Monsters.Info Info = info;

    public void DrawTo(Region view)
    {
        if (Depth != Player.Depth)
            return;

        Map map = Levels.Map;

        // check map coordinates
        if (!map.FullMap.Contains(X, Y))
            return;

        // calc and check view coordinates
        int viewX = X - map.Offset.dX, viewY = Y - map.Offset.dY;

        if (!view.Contains(viewX, viewY))
            return;

        // draw monster
        view.WriteChar(viewX, viewY, Char, Color);
    }
}
