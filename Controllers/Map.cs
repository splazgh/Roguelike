
using System.Diagnostics.CodeAnalysis;

namespace Roguelike;

internal class Map(int width, int height) : IConsoleDrawer
{
    private readonly bool[,] known_sites = new bool[width, height];
    private readonly char[,] representation = new char[width, height];

    private readonly List<(int, int)> entries = new();
    private readonly Dictionary<(int, int), MapObject> objects = new();
    private readonly Dictionary<(int, int), Monster> monsters = new();

    internal IReadOnlyCollection<(int, int)> Entries => entries;

    public void AddEntry(int x, int y)
    {
        entries.Add((x, y));
    }

    public readonly Region FullMap = new(0, 0, width, height);

    // interactive map support
    public bool TryGetObject(int x, int y, [NotNullWhen(true)] out MapObject? obj)
    {
        if (objects.TryGetValue((x, y), out obj))
            return true;

        obj = null;
        return false;
    }

    public bool TryGetMonster(int x, int y, [NotNullWhen(true)] out Monster? monster)
    {
        if (monsters.TryGetValue((x, y), out monster))
            return true;

        monster = null;
        return false;
    }

    public bool[,] TestDirections(int x, int y)
    {
        bool[,] result = new bool[3, 3];

        for (int j = -1; j < 2; j++)
            for (int i = -1; i < 2; i++)
            {
                var map_c = (x + i, y + j);

                if (!FullMap.Contains(map_c))
                    continue;

                if (monsters.ContainsKey(map_c))
                    continue;

                if (objects.TryGetValue(map_c, out var obj))
                    result[1 + i, 1 + j] = obj?.CanPass is not false;
                else
                    result[1 + i, 1 + j] = true;
            }

        return result;
    }

    // map generator support
    public void Paste(string[] site_template, int offsetX, int offsetY)
    {
        int mapX, mapY;

        for (int j = 0; j < site_template.Length; j++)
        {
            if ((mapY = j + offsetY) >= known_sites.GetLength(1))
                break;

            string site_row = site_template[j];

            for (int i = 0; i < site_row.Length; i++)
            {
                if ((mapX = i + offsetX) >= known_sites.GetLength(0))
                    break;

                char c = site_row[i];

                if (c != ' ' || representation[mapX, mapY] == '\0')
                    representation[mapX, mapY] = c;

                if (!MapObjectsCollection.Unknown(c))
                {
                    MapObject place = new(c, mapX, mapY);
                    objects[place.Coordinates] = place;
                }
            }
        }
    }

    public List<(int X, int Y)> GetFreeSites(bool canPass, bool canSwim)
    {
        List<(int X, int Y)> result = new();

        for (int j = 0; j < representation.GetLength(1); j++)
        
            for (int i = 0; i < representation.GetLength(0); i++)
            {
                char c = representation[i, j];

                if (c == '\0')
                    continue;

                if (Player.X == i && Player.Y == j)
                    continue;

                if (MapObjectsCollection.Unknown(c))
                    result.Add((i, j));
            }

        foreach ((var coordinates, MapObject obj) in objects)
        
            if (obj.CanPass && canPass)
                result.Add(coordinates);

            else if (obj.CanSwim && canSwim)
                result.Add(coordinates);

        return result
            .OrderBy(t => t.Y)
            .ThenBy(t => t.X)
            .ToList();
    }

    public void AddMonster((int, int) coordinates, Monster monster)
    {
        monsters[coordinates] = monster;
    }

    // map view support
    private (int dX, int dY) _offset;

    public (int dX, int dY) Offset
    {
        get => _offset;

        private set
        {
            if (Player.Offset != value)
                DrawMapPoint(ScreenCap.View, Player.X, Player.Y);

            _offset = value;
        }
    }

    public void DrawTo(Region view)
    {
        int mapY, mapX;

        List<MapObject> row_objects = new();
        List<Monster> row_monsters = new();

        // draw map from top to bottom
        int row_index = -1;
        while (++row_index + view.Y1 < view.Y2)
        {
            mapY = Offset.dY + row_index;

            char[] fullRow = view.Line;
            if (mapY < representation.GetLength(1))
            {
                // build map line from left to right
                int col_index = -1;
                while (++col_index + view.X1 < view.X2)
                {
                    mapX = Offset.dX + col_index;

                    if (mapX >= representation.GetLength(0))
                        continue;

                    if (!known_sites[mapX, mapY])
                        continue;

                    if (monsters.TryGetValue((mapX, mapY), out var monster))
                        row_monsters.Add(monster);

                    else if (objects.TryGetValue((mapX, mapY), out var map_object))
                        row_objects.Add(map_object);

                    char c = representation[mapX, mapY];

                    if (c == '\0')
                        continue;

                    fullRow[col_index] = c;
                }
            }

            // draw map line
            view.SetCursorPosition(0, mapY);
            view.WriteLine(new string(fullRow));

            row_objects.ForEach(obj => obj.DrawTo(view));
            row_objects.Clear();

            row_monsters.ForEach(m => m.DrawTo(view));
            row_monsters.Clear();
        }

        // draw player
        Player.DrawTo(view);
    }

    public void DrawMapPoint(Region view, int X, int Y)
    {
        // check map coordinates
        if (!FullMap.Contains(X, Y))
            return;

        // calc and check view coordinates
        int viewX = X - Offset.dX, viewY = Y - Offset.dY;

        if (!view.Contains(viewX, viewY))
            return;

        if (!known_sites[X, Y])
        {
            view.WriteChar(X, Y, ' ');
            return;
        }

        if (TryGetMonster(X, Y, out var m))
        {
            m.DrawTo(view);
            return;
        }

        if (TryGetObject(X, Y, out var obj))
        {
            obj.DrawTo(view);
            return;
        }

        // draw one char at map point
        view.WriteChar(X, Y, representation[X, Y]);
    }

    internal void OpenFogOfWar(int x, int y, int range)
    {
        int square_range = range * range;

        for (int j = -range; j <= range; j++) 
        {
            if (!FullMap.Contains(x, y + j))
                continue;

            for (int i = -range; i <= range; i++)
            {
                if (!FullMap.Contains(x + i, y + j))
                    continue;

                if (i * i + j * j > square_range)
                    continue;

                if (!DirectVisible(x, y, i, j))
                    continue;

                if (!known_sites[x + i, y + j])
                {
                    known_sites[x + i, y + j] = true;
                    DrawMapPoint(ScreenCap.View, x + i, y + j);
                }
            }
        }
    }

    private bool DirectVisible(int x1, int y1, int dx, int dy)
    {
        int x = x1, y = y1;
        int x2 = x1 + dx, y2 = y1 + dy;

        int x_step = Math.Abs(dx);
        int y_step = Math.Abs(dy);

        bool lastVisible = true;
        int x_threshold = 0, y_threshold = 0;

        while (true)
        {
            if (!lastVisible)
                return false;

            if (x == x2 || y == y2)
                break;

            if (objects.TryGetValue((x, y), out var obj))
                lastVisible = obj?.CanPass is not false;

            if (x_threshold <= 0)
                x_threshold += x_step;

            if (y_threshold <= 0)
                y_threshold += y_step;

            // look at the direction
            if (x_threshold >= y_threshold)
            {
                x_threshold -= y_step;

                if (dx < 0)
                    x--;
                else if (dx > 0)
                    x++;
            }

            if (x_threshold <= y_threshold)
            {
                y_threshold -= x_step;

                if (dy < 0)
                    y--;
                else if (dy > 0)
                    y++;
            }
        }

        return true;
    }
}
