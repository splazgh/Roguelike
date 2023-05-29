
using System.Diagnostics;

namespace Roguelike;

[DebuggerDisplay("{Header} {Value}/{MaxValue}")]
internal class DynamicStats(string name, int v, int m, ConsoleColor color) : IConsoleDrawer
{
    public string Header = name!.Trim() + ' ';
    public uint Bottom = 0;

    public int Value = v, MaxValue = m;
    public ConsoleColor Color = color;

    public void DrawTo(Region region)
    {
        if (Bottom == 0)
            return;

        region.SetCursorPosition(0, region.Height - (int)Bottom);

        double actualWidth = Math.Min(MaxValue, region.Width - 2);

        if (actualWidth == 0)
        {
            region.Write(new(region.Line));
            return;
        }

        region.Write(Header);

        char[] line = region.Line[Header.Length..^2];

        for (int i = 0; i < Math.Min(line.Length, actualWidth); i++)
            line[i] = '░';

        double stat_value = 0;
        double density = MaxValue / actualWidth;

        for (int i = 0; i < line.Length; i++)
        {
            if (stat_value >= Value)
                break;

            stat_value += density;

            line[i] = (stat_value <= Value) switch
            {
                true => '█',
                false => '▌'
            };
        }

        region.SetCursorPosition(Header.Length, region.Height - (int)Bottom);
        region.Write(new(line), Color);
    }
}
