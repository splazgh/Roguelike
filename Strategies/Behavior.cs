
using static Roguelike.Behavior;
using static Roguelike.Behavior.Mood;

namespace Roguelike;

internal class Behavior(Mood state)
{
    private static readonly Random RNG = new(DateTime.Now.DayOfYear + (int)DateTime.Now.TimeOfDay.Ticks);

    public (int x, int y)? Safepoint { get; protected set; }
    public (int x, int y)? Target { get; protected set; }

    public int MoodTimer { get; set; } = 40;
    public Mood State { get; protected set; } = state;

    public void OnPlayerVisible((int, int) point)
    {
        switch (State)
        {
            case Calm or Sleep or RunAway:
                return;

            case Rest:
                State = HangingAround;
                break;

            case HangingAround
            when MoodTimer > 20:
                break;

            case Coward or ShotDistance or StrikeBack or KeepFlag:
                break;

            default:
                Target = point;
                State = Offence;
                break;
        };

        MoodTimer = 40;
    }

    public void OnLoudSound((int, int) point)
    {
        switch (State)
        {
            case Sleep:
                State = RunAway;
                break;

            case Rest:
                Target = point;
                State = StrikeBack;
                break;

            default:
                return;
        }

        MoodTimer = 40;
    }

    public (int x, int y) GetMove(Monster subject)
    {
        var map = Levels.Data[subject.Depth];
        bool[,] directions = map.TestDirections(subject.X, subject.Y);

        List<(int dX, int dY)> near_sites = new();

        for (int j = 0; j < 3; j++)
            for (int i = 0; i < 3; i++)
            {
                if (!directions[i, j])
                    continue;

                near_sites.Add((i - 1, j - 1));
            }

        switch (State)
        {
            // pointless
            case HangingAround
            when near_sites.Any():
                return near_sites[RNG.Next(near_sites.Count)];

            case Tracking
            when near_sites.Any():

                break;

            // two-point routes
            case Pathtrail or RunAway or StrikeBack:
                break;

            // passive
            default:
            case Sleep or Rest or Calm:
                break;
        }

        return (0, 0);
    }

    public enum Mood
    {
        // passive moods
        Sleep,
        Rest,
        Calm,

        // pointless moods
        HangingAround,
        Tracking,

        // two-point routes
        Pathtrail,
        RunAway,
        StrikeBack,

        // one-point tensions
        Coward,
        ShotDistance,
        KeepFlag,
        Freelance,

        // subordinated
        FollowLeader,
        Offence
    }
}
