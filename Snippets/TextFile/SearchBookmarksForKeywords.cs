/* Search Bookmarks for keywords
Version: v0.1

Searches the bookmarks folder for bookmar items containing keywords in random order (not necessarily tags). It parses the tags and searches tags in random order.

This is used on KMD style bookmarks, examples:

* (cantonese) zh.wikipedia.org/zh-hans/粵語注音符號
* (Car, #Wanted) https//google.com/Porche911 Also don't forget German/Europe delivery option😆
* (Car, Race Car) "Toyota GR Supra" Pure white (exterior) is super nice. Use some cool brown or black accent interior to brighten it up.
* (Car) https://www.tuxmat.ca/

Notice at the moment this doesn't search files in subfolders.

TODO:

- [ ] Configure searching either only in tags, or full text
- [ ] Configure search in subfolders

To search full text from specific file or specific folder, one can use other snippets available under TextFile catagory.
*/

#region Helpers
(string Line, string Source)[] FindLinesInFiles(IEnumerable<string> filePaths, string[] keywords, bool caseSensitive = false)
{
	string[] mustIncludeKeywords = keywords
		.Where(k => !k.StartsWith("!"))
		.Select(k => caseSensitive ? k : k.ToLower())
		.ToArray();
	string[] mustExcludeKeywords = keywords
		.Where(k => k.StartsWith("!"))
		.Select(k => k.TrimStart('!'))
		.Select(k => caseSensitive ? k : k.ToLower())
		.ToArray();

    // Find texts in file containing keywords (random order)
	(string Line, string Source)[] found = filePaths
		.SelectMany(file => {
			string fileName = Path.GetFileNameWithoutExtension(file);
			return File
				.ReadAllLines(file)
				.Select((line, i) => (Line: $"@{i} {line}", Source: fileName));
		})
		.Where(pair => {
			string modifedLine = caseSensitive ? pair.Line : pair.Line.ToLower();
			return mustIncludeKeywords.All(k => modifedLine.Contains(k)) 
				&& mustExcludeKeywords.All(k => !modifedLine.Contains(k));
		})
		.ToArray();
	return found;
}
#endregion

// Entry point
void Search(string folder, string keywords, char delimiter = ' ', bool caseSensitive = false)
{
    if (!Directory.Exists(folder))
    {
        WriteLine($"Folder {folder} doesn't exist.");
        return;
    }

    var found = FindLinesInFiles(Directory.EnumerateFiles(folder),
        // Use "!" in front of the word to denote "not include" 
        keywords.Split(delimiter, StringSplitOptions.RemoveEmptyEntries), caseSensitive);
    foreach((string Line, string Source) in found)
        WriteLine($"{Line} (Source: <{Source}>)");
}
void Search(string folder, bool caseSensitive, params string[] keywords)
{
    if (!Directory.Exists(folder))
    {
        WriteLine($"Folder {folder} doesn't exist.");
        return;
    }

    var found = FindLinesInFiles(Directory.EnumerateFiles(folder),
        // Use "!" in front of the word to denote "not include" 
        keywords, caseSensitive);
    foreach((string Line, string Source) in found)
        WriteLine($"{Line} (Source: <{Source}>)");
}

// Doc
WriteLine("""
Method:
  void Search(string folder, string keywords, char delimiter = ' ', bool caseSensitive = false)
  void Search(string folder, bool caseSensitive, params string[] keywords)
""");