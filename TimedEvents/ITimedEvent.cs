
namespace Roguelike;

internal interface ITimedEvent
{
    public int TimeStep { get; }
    public int TimeCounter { get; protected set; }

    public bool IsStopped { get; protected set; }

    public void TickTime();

    public void OnTimedAction(int timeTick)
    {
        if ((TimeCounter -= timeTick) > 0)
            return;

        while (TimeCounter <= 0)
        {
            TickTime();

            TimeCounter += TimeStep;
        }
    }

    public void OnHostileAction()
    {
        IsStopped = true;
    }
}
