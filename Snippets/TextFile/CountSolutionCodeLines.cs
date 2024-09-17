/* Count Solution Code Lines
Version: v0.1
Author: Charles Zhang
Co-Author: ChatGPT o1-preview

Recursively searches for .md and .cs files (ignoring .txt files), counts the total number of lines in these files, and outputs the result.
Ignores obj and bin folder.
*/

// Check if a folder path is provided
if (Arguments.Length == 0)
    WriteLine("Please provide a folder path as an argument.");
else
{    
    string folderPath = Arguments[0];

    // Check if the provided folder exists
    if (!Directory.Exists(folderPath))
        Console.WriteLine("The specified folder does not exist.");
    else
    {
        // Define the recognized file extensions
        var recognizedExtensions = new[] { ".md", ".cs" };
        var ignoreFolders = new[] { "bin", "obj" };

        // Get all files recursively from the folder
        var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
        List<(string Path, int Lines)> sourceFiles = [];

        int totalLines = 0;
        foreach (var file in files)
        {
            string extension = Path.GetExtension(file);
            // Ignore obj and bin folders
            if (ignoreFolders.Any(i => file.Contains($"{Path.DirectorySeparatorChar}{i}{Path.DirectorySeparatorChar}")))
                continue;

            // Check if the file has a recognized extension
            if (recognizedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    // Count the number of lines in the file
                    int lineCount = File.ReadLines(file).Count();
                    totalLines += lineCount;

                    // Add to source files list
                    sourceFiles.Add((file, lineCount));
                }
                catch (Exception ex)
                {
                    WriteLine($"Error reading file {file}: {ex.Message}");
                }
            }
        }

        foreach((string File, int Lines) in sourceFiles)
            WriteLine($"{Path.GetRelativePath(folderPath, File)}: {Lines}");
        WriteLine($"Total number of lines: {totalLines}");
    }
}