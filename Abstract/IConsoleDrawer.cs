
namespace Roguelike;

internal interface IConsoleDrawer
{
    public void OnResize(Region region)
    {
        region.PendingUpdate = true;
        DrawTo(region);
    }

    public abstract void DrawTo(Region region);
}
