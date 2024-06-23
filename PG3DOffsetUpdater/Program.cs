using System.Text;
using PG3DOffsetUpdater;
using Quickenshtein;

Offset N(string name, int old)
{
    return new Offset(name, old, "", 0, false, false, Int32.MaxValue, -1);
}

List<Offset> offsets = new List<Offset>()
{
    // N("Player_move_c.Update", 0x1B85180),
};

string[] offsetLines = File.ReadAllLines("offsets.csv");
foreach (var offsetLine in offsetLines)
{
    var parts = offsetLine.Split(",");
    offsets.Add(N(parts[0], Convert.ToInt32(parts[1], 16)));
}

string[] oldLines = File.ReadAllLines("old_dump.cs");
string[] newLines = File.ReadAllLines("latest_dump.cs");


// Go Through each Line
for (var i = 0; i < oldLines.Length; i++)
{
    string line = oldLines[i];
    foreach (var offset in offsets)
    {
        if (offset.TextFound) continue;
        // Match Each Offset
        string match = $"// RVA: 0x{offset.OldOffset:x} VA:";
        if (line.Contains(match))
        {
            // Found old Offset
            if (oldLines.Length > i + 25)
            {
                var sb = new StringBuilder();
                for (int j = 1; j < 25; j++)
                {
                    sb.Append(oldLines[i + j]);
                }
                offset.Text = sb.ToString();
                offset.TextFound = true;
                break;
            }
        }
    }
}

Console.WriteLine($"Found {offsets.Count(o => o.TextFound)}/{offsets.Count} Old Offsets!");

// Find Matches
for (var i = 0; i < newLines.Length; i++)
{
    if (newLines.Length > i + 25)
    {
        var sb = new StringBuilder();
        for (int j = 1; j < 25; j++)
        {
            sb.Append(newLines[i + j]);
        }
        var match = sb.ToString();

        Parallel.ForEach(offsets, new ParallelOptions() {MaxDegreeOfParallelism = 8}, offset =>
        {
            if (!offset.TextFound) return;
            int dist = Levenshtein.GetDistance(match, offset.Text);
            if (dist < offset.Minimum)
            {
                offset.Minimum = dist;
                offset.NewLine = i;
            }
        });
    }

    if (i % 10000 == 0)
    {
        Console.WriteLine($"Progress: {i}/{newLines.Length}");
    }
}

Console.WriteLine($"Found {offsets.Count(o => o.NewLine != -1)}/{offsets.Count} Matches!");

foreach (var offset in offsets)
{
    if (offset.NewLine == -1) continue;
    string line = newLines[offset.NewLine];
    bool flag = false;
    string offsetString = "";
    foreach (var s in line.Split(" "))
    {
        if (flag)
        {
            offsetString = s;
            break;
        }
        if (s.Contains("RVA")) flag = true;
    }

    if (offsetString != "")
    {
        offset.NewOffset = Convert.ToInt32(offsetString, 16);
        offset.NewFound = true;
    }
}

Console.WriteLine($"Found {offsets.Count(o => o.NewFound)}/{offsets.Count} Offsets!");

Console.WriteLine();
var stringBuilder = new StringBuilder();
foreach (var offset in offsets)
{
    if (!offset.NewFound) continue;
    Console.WriteLine($"{offset.Name} -> 0x{offset.NewOffset:X}");
    stringBuilder.Append($"{offset.Name},0x{offset.NewOffset:X}\n");
}
File.WriteAllText("offsets_new.csv", stringBuilder.ToString());

