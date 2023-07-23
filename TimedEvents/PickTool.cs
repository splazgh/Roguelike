
namespace Roguelike;

internal class DigTool((int x, int y) direction, int tick) : ITimedEvent
{
    public (int X, int Y) Direction { get; } = direction;

    public int TickSize { get; private set; } = tick;
    public int TimeCounter { get; set; } = 0;

    public bool IsStopped { get; set; } = false;

    public void OnTick()
    {

    }
}
