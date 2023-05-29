
namespace Roguelike;

internal partial class Map
{
    private readonly Dictionary<(int, int), Footprint> footprints = new(40);

    public void AddFootprint(int x, int y, int power, (int, int) direction)
    {
        (int, int) coord = (x, y);

        if (!FullMap.Contains(coord))
            return;

        footprints[coord] = new Footprint(power, direction);
    }

    public void FootprintOverlay(int x, int y, int power)
    {
        (int, int) coord = (x, y);

        if (!footprints.Remove(coord, out var footprint))
            return;

        if (footprint.Power > power)
            footprints.Add(coord, new(footprint.Power - power, footprint.Direction));
    }

    private record Footprint(int Power, (int, int) Direction);
}
