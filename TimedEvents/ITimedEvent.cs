
namespace Roguelike;

internal interface ITimedEvent
{
    public int TickSize { get; }
    public int TimeCounter { get; protected set; }

    public bool IsStopped { get; protected set; }

    public void OnTick();

    public void OnTimedAction(int elapsed)
    {
        if ((TimeCounter -= elapsed) > 0)
            return;

        while (TimeCounter <= 0)
        {
            OnTick();

            TimeCounter += TickSize;
        }
    }

    public void OnHostileAction()
    {
        IsStopped = true;
    }
}
