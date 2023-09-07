
namespace Roguelike;

internal static class StatusRow
{
    private const string key_tips_template = "  CTRL+X  Exit      F1  Key Tip";

    internal static void OnResize(Region region)
    {
        region.PendingUpdate = true;
        DrawTo(region);
    }

    public static void DrawTo(Region region)
    {
        if (ScreenCap.IsResized)
            return;

        var full_row = region.Line;
        key_tips_template.ToString().CopyTo(full_row);

        region.SetCursorPosition(0, region.Height - 1);
        region.Write(new(full_row));

        region.WriteTipText(Player.GetState());
    }

    public static GlobalAction ProcessKey(ConsoleKeyInfo key_info)
    {
        if (key_info.Modifiers == ConsoleModifiers.Control
            && key_info.Key == ConsoleKey.X)
        {
            return GlobalAction.Exit;
        }

        else if (key_info.Modifiers == 0
            && key_info.Key == ConsoleKey.F1)
        {
            return GlobalAction.KeyHelp;
        }

        return GlobalAction.None;
    }

    public enum GlobalAction : int
    {
        None = 0,
        KeyHelp = 1,
        Exit = 2
    }
}
