
namespace Roguelike;

internal class MonsterTactic : Behavior, ITimedEvent
{
    public Monster Subject { get; set; }
    public Monster? Leader { get; set; }

    // ITimedEvent interface
    public int TickSize { get; private set; }
    public int TimeCounter { get; set; }

    public bool IsStopped { get; set; }

    public MonsterTactic(int timeStep, Monster subject, Monster? leader = null)
        : base(leader is null
            ? Monsters.Initiate(subject.Char, subject.Kind)
            : Mood.FollowLeader)
    {
        TickSize = timeStep;
        Subject = subject;
        Leader = leader;
    }

    public void OnTick()
    {
        State = this.Update(Subject.Char, Subject.Kind);

        if (State == Mood.Sleep)
        {
            IsStopped = true;
            return;
        }

        var map = Levels.Data[Subject.Level];

        var directions = map.TestDirections(Subject.X, Subject.Y);
        directions[1, 1] = false;

        (int dx, int dy) = GetMove(Subject);

        if (!directions[1 + dx, 1 + dy])
            return;

        map.DrawMapPoint(ScreenCap.View, Subject.X, Subject.Y);
        
        Subject.X += dx;
        Subject.Y += dy;

        Subject.DrawTo(ScreenCap.View);
    }

    public void ReplaceTarget((int x, int y) target)
    {
        Target = target;
        Safepoint = null;
    }
}
