
namespace Roguelike;

public partial class Journal
{
    private const int records_cap = 1000;

    private readonly List<Record> records = new(records_cap);
    private int normal = 0, signified = 0;

    private double prev = 0.0, current = 0.0;

    public static Journal Log { get; } = new();
    public bool Depleted { get; private set; } = true;

    public void AddNormal(string message) => AddRecord(Relevance.Normal, message);
    public void AddSignified(string message) => AddRecord(Relevance.Signify, message);
    public void AddQuest(string message) => AddRecord(Relevance.Quest, message);

    private void AddRecord(Relevance rel, string message)
    {
        records.Insert(0, new(rel, message));

        Depleted = false;

        switch (rel)
        {
            case Relevance.Normal:
                normal++;
                break;

            case Relevance.Signify:
                signified++;
                break;

            case Relevance.Quest:
                break;
        }
    }

    private void CutRecords(int limit)
    {
        if (normal + signified <= records_cap)
            return;

        int keep_normal = records_cap * 7 / 10;
        int keep_signified = records_cap * 3 / 10;

        int idx = records.Count;

        while (--idx > limit)
        {
            switch (records[idx].Level)
            {
                case Relevance.Normal:

                    if (keep_normal > 0)
                        keep_normal--;
                    else
                    {
                        records.RemoveAt(idx);
                        current -= 1.0;
                    }

                    continue;

                case Relevance.Signify
                when keep_normal == 0:

                    if (--keep_signified <= 0)
                        break;

                    records.RemoveAt(idx);
                    current -= 1.0;

                    continue;

                case Relevance.Quest:
                    continue;

                default:
                    break;
            }

            break;
        }
    }

    public bool ProcessKey(ConsoleKeyInfo key_info)
    {
        return key_info.Modifiers == 0
            && key_info.Key is ConsoleKey.Spacebar or ConsoleKey.Enter;
    }

    public void DrawTo(Region region)
    {
        if (region.PendingUpdate)
        {
            current = prev;
            region.PendingUpdate = false;
        }
        else
            prev = current;

        region.Clear();

        int row_index = 0;

        int main_index = (int)current;
        int sub_index = (int)((current - main_index) * 100);

        while (!region.WriteLock && main_index++ < records.Count)
        {
            row_index = records.Count - main_index;

            var rows = records[row_index].SplitRows(region.Width);

            while (!region.WriteLock && sub_index < rows.Count)
            {
                region.WriteLine(rows[sub_index++]);
                current += 0.01;
            }

            Depleted = row_index == 0 && sub_index == rows.Count;

            if (!Depleted)
                region.MoreTipText();

            current = main_index;
            sub_index = 0;
        }

        CutRecords(row_index);
    }

    private readonly struct Record(Relevance level, string message)
    {
        public readonly Relevance Level = level;
        public readonly string Message = message ?? string.Empty;

        public int HeightMeasure(in int width)
        {
            int height = 0, pos = width, prevPos = 0;

            while (pos < Message.Length)
            {
                height++;

                if (RegressiveScan(Message, ' ', pos, prevPos) is int delimiterPos)
                {
                    prevPos = pos;
                    pos = delimiterPos + width;
                }
                else
                {
                    prevPos = pos;
                    pos += width;
                }
            }

            if (prevPos < message.Length)
                height++;

            return height;
        }

        public List<string> SplitRows(in int width)
        {
            List<string> rows = new();

            int pos = width, prevPos = 0;

            while (pos < Message.Length)
            {
                if (RegressiveScan(Message, ' ', pos, prevPos) is int delimiterPos)
                {
                    rows.Add(Message[prevPos..delimiterPos]);

                    prevPos = pos;
                    pos = delimiterPos + width;
                }

                else
                {
                    rows.Add(Message[prevPos..pos]);

                    prevPos = pos;
                    pos += width;
                }
            }

            if (prevPos < message.Length)
                rows.Add(Message[prevPos..]);

            return rows;
        }

        private int? RegressiveScan(string message, char v, int from, int downto)
        {
            if (downto >= message.Length)
                return null;

            int idx = from = Math.Min(from, message.Length);

            while (--idx > downto)
            {
                if (Message[idx] == v)
                    return from;
            }

            return from;
        }

        public override string ToString() => $"{Enum.GetName(Level)}: {Message}";
    }

    public enum Relevance : int
    {
        Normal = 0,
        Signify = 1,
        Quest = 2
    }
}
