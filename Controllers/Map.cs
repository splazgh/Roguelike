
using System.Diagnostics.CodeAnalysis;

namespace Roguelike;

internal class Map(int width, int height)
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

    public void AddMonster((int, int) coordinates, Monster monster)
    {
        monsters[coordinates] = monster;
    }

    public bool TryGetMonster(int x, int y, [NotNullWhen(true)] out Monster? monster)
    {
        if (monsters.TryGetValue((x, y), out monster))
            return true;

        monster = null;
        return false;
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

        if (TryGetObject(X, Y, out var obj))
        {
            obj.DrawTo(view);
            return;
        }

        // draw one char at map point
        view.WriteChar(X, Y, representation[X, Y]);
    }
}
