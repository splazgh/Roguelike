
namespace Roguelike;

internal class DigTool((int x, int y) direction, int timeStep) : ITimedEvent
{
    public (int X, int Y) Direction { get; } = direction;

    public int TimeStep { get; private set; } = timeStep;
    public int TimeCounter { get; set; } = 0;

    public bool IsStopped { get; set; } = false;

    public void TickTime()
    {

    }
}
