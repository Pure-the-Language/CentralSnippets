void Print(string folder)
{
	if (Directory.Exists(folder))
	{
		WriteLine(Path.GetFileNameWithoutExtension(folder));
		foreach(string sub in Directory.EnumerateDirectories(folder).OrderBy(f => 
			Path.GetFileNameWithoutExtension(f)))
		{
			string name = Path.GetFileNameWithoutExtension(sub);
			int fileCount = Directory.EnumerateFiles(sub).Count();
			
			if (fileCount == 0) continue;
			WriteLine($" - {name} (Count: {fileCount})");
		}
	}
}
WriteLine("""
Methods:
  Print(string folder): Print first level folders with file counts
""");