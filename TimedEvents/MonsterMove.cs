
namespace Roguelike;

internal class MonsterMove((int x, int y) target, int timeStep) : ITimedEvent
{
    public (int x, int y) Target { get; private set; } = target;

    public int TickSize { get; private set; } = timeStep;
    public int TimeCounter { get; set; } = 0;

    public bool IsStopped { get; set; } = false;

    public void OnTick()
    {

    }

    public void ReplaceTarget((int x, int y) target)
    {
        Target = target;
    }
}
