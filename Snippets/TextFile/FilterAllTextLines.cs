/* Filter All Text lines
Version: v0.1

This script provides snippet/template for quickly search through all text files in all subfolders using arbitrary criteria.
*/

public static readonly string AllFiles = "*.*";
IEnumerable<string> Filter(string folderPath, string fileNameFilter, Func<string, bool> lineFilter)
{
	// Doesn't deal with UnauthorizedAccessException; If we want to handle it, we might want to implement our own routine to get all files
	string[] allfiles = Directory.GetFiles(folderPath, AllFiles, SearchOption.AllDirectories);
	return allfiles
		.SelectMany(f => File.ReadLines(f).Where(lineFilter));
}

// Doc
WriteLine("""
Method:
  IEnumerable<string> Filter(string folderPath, Func<string, bool> fileNameFilter, Func<string, bool> lineFilter): Returns lines.
""");