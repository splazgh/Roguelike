
namespace Roguelike;

internal class RunTool((int x, int y) direction, int timeStep) : ITimedEvent
{
    private bool isJustRan = true;
    private bool[,] backside_disable_mask = new bool[3, 3];

    public (int X, int Y) Direction { get; private set; } = direction;

    public int TimeStep { get; private set; } = timeStep;
    public int TimeCounter { get; set; } = 0;

    public bool IsStopped { get; set; } = false;

    public void TickTime()
    {
        if (IsStopped)
            return;

        var map = Levels.Map;

        // process no direction
        if (Direction.X == 0 && Direction.Y == 0)
        {
            if (isJustRan)
                Journal.Log.AddNormal("You jump in place.");

            IsStopped = true;
            return;
        }
        
        // analyze directions
        var environment = map.TestDirections(Player.X + Direction.X, Player.Y + Direction.Y);

        environment[1 - Direction.X, 1 - Direction.Y] = false;    // disable current position
        environment[1, 1] = false;                                // disable next position

        int directions_count = 0;
        (int x, int y) new_direction = Direction;

        if (isJustRan)
            directions_count++;
        else
        {
            for (int j = -1; j < 2; j++)
                for (int i = -1; i < 2; i++)
                {
                    if (!environment[1 + i, 1 + j])
                        continue;

                    if (isJustRan)
                    {
                        directions_count = 1;
                        break;
                    }
                    else if (backside_disable_mask[1 + i, 1 + j])
                        continue;

                    directions_count++;

                    new_direction = (i, j);
                }
        }

        try
        {
            // process failed move action
            if (!Player.ActionAt(Player.X + Direction.X, Player.Y + Direction.Y))
                IsStopped = true;

            // process successful move action
            else
            {
                if (isJustRan)                                              // disable backside running
                {
                    if (Direction.X != 0)
                        for (int j = -1; j < 2; j++)
                            backside_disable_mask[1 - Direction.X, 1 + j] = true;

                    if (Direction.Y != 0)
                        for (int i = -1; i < 2; i++)
                            backside_disable_mask[1 + i, 1 - Direction.Y] = true;

                    isJustRan = false;
                }
                else
                    backside_disable_mask = new bool[3, 3];
            }
        }
        finally
        {
            if (directions_count < 2)
                Direction = new_direction;
            else
                Direction = (0, 0);
        }
    }
}
