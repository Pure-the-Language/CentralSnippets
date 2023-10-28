/* Search Text Lines Containing Keywords in Any Order
Version: v0.1

This script searches lines of text file that contains keywords in random order. This script saves us effort having to write a regular expression.

Also see: SearchFilesContainingKeywordsInFolder.cs
*/

void Search(string filePath, string keywordsPhrase, char splitter = ' ')
{
    string[] keywords = keywordsPhrase.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
    foreach(var result in File
        .ReadLines(filePath)
        .Select((line, index) => (Line: line, Index: index))
        .Where(p => keywords.All(kw => p.Line.Contains(kw))))
        WriteLine($"@{result.Index}: {result.Line}");
}

// Doc
WriteLine("""
Method:
  void Search(string filePath, string keywordsPhrase, char splitter = ' ')
""");