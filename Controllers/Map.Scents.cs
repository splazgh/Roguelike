
namespace Roguelike;

internal partial class Map
{
    private readonly Dictionary<(int, int), int> scent_tails = new(40);
    private readonly List<(int, int)> reversive_scent = new(40);

    public void AddScent(int x, int y, int power)
    {
        (int, int) coord = (x, y);

        if (!FullSize.Contains(coord))
            return;

        if (scent_tails.Remove(coord, out _))
            reversive_scent.Remove(coord);

        if (power > 0)
        {
            scent_tails.Add(coord, power);
            reversive_scent.Add(coord);
        }
    }

    public void ScentOverlay(int x, int y, int power)
    {
        (int, int) coord = (x, y);

        if (!scent_tails.Remove(coord, out var scent_power))
            return;

        if (scent_power > power)
            scent_tails.Add(coord, scent_power - power);
        else
        {
            int scent_idx = reversive_scent.IndexOf(coord);

            if (scent_idx >= 0)
                reversive_scent.RemoveAt(scent_idx);
        }
    }

    public void AgeScents(int power)
    {
        for (int i = reversive_scent.Count -1; i >= 0; i--)
        {
            var coord = reversive_scent[i];

            if (!scent_tails.TryGetValue(coord, out int scent_power))
            {
                reversive_scent.RemoveAt(i);
                continue;
            }

            if (--scent_power > power)
                scent_tails[coord] = scent_power;
            else
                scent_tails.Remove(coord);
        }
    }
}
