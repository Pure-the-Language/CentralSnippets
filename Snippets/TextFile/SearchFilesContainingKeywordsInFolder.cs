/* Search Files Containing Keywords in Folder
Version: v0.2

Find texts in file containing keywords (random order).

Also see: SearchTextLinesContainingKeywords.cs

## Changelog

* v0.1: Initial setup
* v0.2: Update to latest Pure 2; Explicitly use static System.Console.
*/
using System.Text;
using static System.Console;

void Search(string folder, params string[] keywords)
{
    if (!Directory.Exists(folder))
    {
        WriteLine($"Folder {folder} doesn't exist.");
        return;
    }
    
    (string Line, string Source)[] found = Directory.EnumerateFiles(folder)
        .SelectMany(file => {
            string fileName = Path.GetFileNameWithoutExtension(file);
            return File
                .ReadAllLines(file)
                .Select((line, i) => (Line: $"@{i} {line}", Source: Path.GetFileName(file)));
        })
        .Where(p => keywords.All(k => p.Line.Contains(k)))
        .ToArray();
    
    StringBuilder result = new();
    foreach(var group in found
        .GroupBy(f => f.Source))
    {
        result.AppendLine($"(Source: {group.Key})");
        foreach((string Line, string Source) in group)
            result.AppendLine($"{Line}");
        result.AppendLine();
    }
    WriteLine(result.ToString().TrimEnd());
}

// Doc
WriteLine("""
Method:
  void Search(string folder, params string[] keywords)
""");