/* Rename folders in folder using regular expressions 
Version: v0.1
Original Date: 2024-01-09
Last Update Date: 2024-01-09

Replaces https://github.com/chaojian-zhang/RegRenameWPF
*/

using System.Text.RegularExpressions;

public record ItemChange(string OldName, string NewName, string OldPath, string NewPath);
IEnumerable<ItemChange> EnumerateFolderItems(string folder, string pattern, string replacement)
{
    // Enumerate folders in folder
    foreach(string folder in Directory.EnumerateDirectories(folder))
    {
        string originalFolder = Path.GetDirectoryName(folder);
        string originalFolderName = Path.GetFileName(folder);
        string newFolderName = Regex.Replace(originalFolderName, pattern, replacement);
        string newPath = Path.Combine(originalFolder, newFolderName);
        
        if (Directory.Exists(newPath))
        {
            WriteLine($"Skip {newFolderName} (already exists)");
            continue;   
        }
        
        yield new ItemChange(oldFOlderName, newFolderName, folder, newPath);
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