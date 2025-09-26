/* Rename Files in Folder using Regular Expressions 
Version: v0.2
Updates: 2024-01-09
Last Update: 2025-09-26

Replaces https://github.com/chaojian-zhang/RegRenameWPF

## Changelog

* v0.1: Initial setup
* v0.2: Update to latest Pure 2; Explicitly use static System.Console.
*/

using System.Text.RegularExpressions;
using static System.Console;

void Rename(string folder, string pattern, string replacement)
{
    // Rename files in folder
    foreach(string file in Directory.EnumerateFiles(folder))
    {
        string originalFileName = Path.GetFileName(file);
        string newFileName = Regex.Replace(originalFileName, pattern, replacement);
        string newPath = Path.Combine(folder, newFileName);
        
        if (File.Exists(newPath))
        {
            WriteLine($"Skip {newFileName} (already exists)");
            continue;   
        }
        WriteLine($"{originalFileName} -> {newFileName}");
        File.Move(file, newPath);
    }
}

void Preview(string folder, string pattern, string replacement)
{
    // Enumerate files in folder
    foreach(string file in Directory.EnumerateFiles(folder))
    {
        string originalFileName = Path.GetFileName(file);
        string newFileName = Regex.Replace(originalFileName, pattern, replacement);
        string newPath = Path.Combine(folder, newFileName);
       
        if (File.Exists(newPath))
        {
            WriteLine($"Skip {newFileName} (already exists)");
            continue;   
        }
        WriteLine($"{originalFileName} -> {newFileName}");
    }
}

// Doc
WriteLine("""
Methods:
  void Rename(string folder, string pattern, string replacement)
  void Preview(string folder, string pattern, string replacement)
""");