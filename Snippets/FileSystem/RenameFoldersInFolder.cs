/* Rename folders in folder using regular expressions 
Version: v0.2
Original Date: 2024-01-09
Updates: 2024-01-09
Last Update: 2025-09-26

Replaces https://github.com/chaojian-zhang/RegRenameWPF

## Changelog

* v0.1: Initial setup
* v0.2: Update to latest Pure 2; Explicitly use static System.Console.
*/

using System.Text.RegularExpressions;
using static System.Console;

public record ItemChange(string OldName, string NewName, string OldPath, string NewPath);
IEnumerable<ItemChange> EnumerateFolderItems(string folder, string pattern, string replacement)
{
    // Enumerate folders in folder
    foreach(string item in Directory.EnumerateDirectories(folder))
    {
        string originalName = Path.GetFileName(item);
        string newName = Regex.Replace(originalName, pattern, replacement);
        string newPath = Path.Combine(folder, newName);
        
        if (Directory.Exists(newPath))
        {
            WriteLine($"Skip {newName} (already exists)");
            continue;   
        }
        
        yield return new ItemChange(originalName, newName, item, newPath);
    }
}

void Rename(string folder, string pattern, string replacement)
{
    foreach (var item in EnumerateFolderItems(folder, pattern, replacement))
    {
        WriteLine($"{item.OldName} -> {item.NewName}");
        Directory.Move(item.OldPath, item.NewPath);
    }
}

void Preview(string folder, string pattern, string replacement)
{
    foreach (var item in EnumerateFolderItems(folder, pattern, replacement))
        WriteLine($"{item.OldName} -> {item.NewName}");
}

// Doc
WriteLine("""
Methods:
  void Rename(string folder, string pattern, string replacement)
  void Preview(string folder, string pattern, string replacement)
""");