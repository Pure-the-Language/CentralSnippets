/* Rename Files in Folder using Regular Expressions 
Version: v0.1

Replaces https://github.com/chaojian-zhang/RegRenameWPF
*/

using System.Text.RegularExpressions;

void Rename(string folder, string pattern, string replacement)
{
    // Rename files in folder
    foreach(string file in Directory.EnumerateFiles(folder))
    {
        string originalFolder = Path.GetDirectoryName(file);
        string originalFileName = Path.GetFileName(file);
        string newFileName = Regex.Replace(originalFileName, pattern, replacement);
        string newPath = Path.Combine(originalFolder, newFileName);
        
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
        string originalFolder = Path.GetDirectoryName(file);
        string originalFileName = Path.GetFileName(file);
        string newFileName = Regex.Replace(originalFileName, pattern, replacement);
        string newPath = Path.Combine(originalFolder, newFileName);
       
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