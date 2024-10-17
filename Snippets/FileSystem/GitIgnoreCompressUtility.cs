using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Setup
{
    /// <summary>
    /// Utility class for compressing files not ignored by .gitignore and .customignore rules
    /// </summary>
    public class GitIgnoreCompressUtility
    {
        private readonly string _rootFolder;
        /// <summary>
        /// List of GitIgnore rules
        /// </summary>
        private readonly List<GitIgnoreRule> _ignoreRules = [];
        /// <summary>
        /// Initializes a new instance of the GitIgnoreCompressUtility class
        /// </summary>
        public GitIgnoreCompressUtility(string folderPath) => _rootFolder = Path.GetFullPath(folderPath);

        /// <summary>
        /// Loads ignore patterns from .gitignore and .customignore files
        /// </summary>
        public void LoadIgnorePatterns()
        {
            Console.WriteLine("Loading ignore patterns...");
            LoadIgnorePatternsRecursively(_rootFolder);
            Console.WriteLine($"Total ignore patterns loaded: {_ignoreRules.Count}\n");
        }

        /// <summary>
        /// Recursively loads ignore patterns from .gitignore and .customignore files
        /// </summary>
        private void LoadIgnorePatternsRecursively(string currentFolder)
        {
            string relativePath = Path.GetRelativePath(_rootFolder, currentFolder);

            // Process .gitignore files
            string[] gitIgnoreFiles = Directory.GetFiles(currentFolder, ".gitignore", SearchOption.TopDirectoryOnly);
            foreach (string gitIgnoreFile in gitIgnoreFiles)
            {
                string relativeGitIgnorePath = Path.GetRelativePath(_rootFolder, gitIgnoreFile);
                Console.WriteLine($"Processing .gitignore file: {relativeGitIgnorePath}");
                List<GitIgnoreRule> rules = ParseIgnoreFile(gitIgnoreFile, currentFolder);
                _ignoreRules.AddRange(rules);
            }

            // Process .customignore files
            string[] customIgnoreFiles = Directory.GetFiles(currentFolder, ".customignore", SearchOption.TopDirectoryOnly);
            foreach (string customIgnoreFile in customIgnoreFiles)
            {
                string relativeCustomIgnorePath = Path.GetRelativePath(_rootFolder, customIgnoreFile);
                Console.WriteLine($"Processing .customignore file: {relativeCustomIgnorePath}");
                List<GitIgnoreRule> rules = ParseIgnoreFile(customIgnoreFile, currentFolder);
                // Set high priority for custom ignore rules
                foreach (GitIgnoreRule rule in rules)
                    rule.Sequence = -1;
                _ignoreRules.AddRange(rules);
            }

            // Recursively process subdirectories
            foreach (string dir in Directory.GetDirectories(currentFolder))
                LoadIgnorePatternsRecursively(dir);
        }

        /// <summary>
        /// Parses an ignore file and creates GitIgnoreRule objects
        /// </summary>
        private List<GitIgnoreRule> ParseIgnoreFile(string filePath, string basePath)
        {
            List<GitIgnoreRule> rules = [];
            string[] lines = File.ReadAllLines(filePath);
            for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
            {
                string line = lines[lineNumber];
                string trimmedLine = line.Trim();
                // Skip empty lines and comments
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                bool isNegation = trimmedLine.StartsWith("!");
                if (isNegation)
                    trimmedLine = trimmedLine[1..];

                // Convert gitignore pattern to regex
                string pattern = ConvertGitIgnorePatternToRegex(trimmedLine, basePath);
                Regex regex = new(pattern, RegexOptions.Compiled);

                // Create and configure the GitIgnoreRule
                GitIgnoreRule rule = new()
                {
                    Pattern = regex,
                    IsNegation = isNegation,
                    BasePath = basePath,
                    OriginalPattern = (isNegation ? "!" : "") + trimmedLine,
                    Sequence = _ignoreRules.Count + rules.Count
                };

                // Set high priority for custom ignore rules
                if (filePath.EndsWith(".customignore", StringComparison.OrdinalIgnoreCase))
                    rule.Sequence = -1;

                rules.Add(rule);
                string relativeFilePath = Path.GetRelativePath(_rootFolder, filePath);
                Console.WriteLine($"Loaded pattern: '{(isNegation ? "!" : "")}{trimmedLine}' as regex '{regex}' from file '{relativeFilePath}'");
            }
            return rules;
        }

        /// <summary>
        /// Converts a gitignore pattern to a regular expression
        /// </summary>
        private string ConvertGitIgnorePatternToRegex(string pattern, string basePath)
        {
            bool matchFromRoot = pattern.StartsWith("/");
            if (matchFromRoot)
                pattern = pattern[1..];
            pattern = Regex.Escape(pattern)
                .Replace(@"\[", "[")
                .Replace(@"\]", "]")
                .Replace(@"\!", "!")
                .Replace(@"\-", "-")
                .Replace(@"\#", "#")
                .Replace(@"\ ", " ");
            pattern = pattern.Replace(@"\*\*", ".*");
            pattern = pattern.Replace(@"\*", @"[^/]*");
            pattern = pattern.Replace(@"\?", @"[^/]");
            if (pattern.EndsWith("/"))
                pattern = pattern.TrimEnd('/') + "(/.*)?";
            string relativeBase = Path.GetRelativePath(_rootFolder, basePath).Replace(@"\", "/");
            if (relativeBase == ".")
                relativeBase = string.Empty;
            string regexPattern = "^";
            if (!matchFromRoot)
                regexPattern += "(.*?/)?";
            else
            {
                regexPattern += Regex.Escape(relativeBase);
                if (!string.IsNullOrEmpty(relativeBase))
                    regexPattern += "/";
            }
            regexPattern += pattern + "$";
            return regexPattern;
        }

        /// <summary>
        /// Determines if a file should be included based on ignore rules
        /// </summary>
        private bool IsIncluded(string filePath, out string matchedPattern)
        {
            string relativePath = Path.GetRelativePath(_rootFolder, filePath).Replace(@"\", "/");
            matchedPattern = string.Empty;

            // Hardcoded logic to ignore .git folder
            if (relativePath.StartsWith(".git/", StringComparison.OrdinalIgnoreCase) || 
                relativePath.Contains("/.git/", StringComparison.OrdinalIgnoreCase))
            {
                matchedPattern = "Hardcoded rule: .git folder";
                return false;
            }

            bool isIncluded = false;
            bool parentIsIncluded = false;
            string[] pathParts = relativePath.Split('/');

            for (int i = 1; i <= pathParts.Length; i++)
            {
                string subPath = string.Join("/", pathParts.Take(i));
                foreach (GitIgnoreRule rule in _ignoreRules)
                {
                    if (rule.Pattern.IsMatch(subPath))
                    {
                        bool currentIsIncluded = !rule.IsNegation;

                        if (i < pathParts.Length)
                            parentIsIncluded = currentIsIncluded;
                        else
                            isIncluded = currentIsIncluded;
                        matchedPattern = rule.OriginalPattern;
                        int lastMatchSequence = rule.Sequence;
                        if (rule.Sequence == -1)
                        {
                            isIncluded = false;
                            // Check if it's a directory
                            if (Directory.Exists(filePath))
                            {
                                Console.WriteLine($"Directory '{relativePath}' is ignored due to custom ignore rule: {matchedPattern}");
                                // Recursively check files in the ignored directory
                                foreach (string file in Directory.GetFiles(filePath, "*", SearchOption.AllDirectories))
                                {
                                    string relativeFile = Path.GetRelativePath(_rootFolder, file);
                                    Console.WriteLine($"File '{relativeFile}' is ignored due to parent directory being ignored");
                                }
                            }
                            else
                                Console.WriteLine($"File '{relativePath}' is ignored due to custom ignore rule: {matchedPattern}");
                            return isIncluded;
                        }
                    }
                }
                if (parentIsIncluded)
                {
                    isIncluded = true;
                    matchedPattern = $"Parent directory '{subPath}' is included";
                    break;
                }
            }

            // Check for custom ignore rules in parent directories
            if (isIncluded)
            {
                string? currentDir = Path.GetDirectoryName(filePath);
                while (currentDir != null && currentDir != _rootFolder)
                {
                    string customIgnorePath = Path.Combine(currentDir, ".customignore");
                    if (File.Exists(customIgnorePath))
                    {
                        string[] customIgnoreLines = File.ReadAllLines(customIgnorePath);
                        string fileName = Path.GetFileName(filePath);
                        if (customIgnoreLines.Any(line => line.Trim() == fileName))
                        {
                            isIncluded = false;
                            string relativeCustomIgnorePath = Path.GetRelativePath(_rootFolder, customIgnorePath);
                            matchedPattern = $"File name '{fileName}' matched in parent .customignore: {relativeCustomIgnorePath}";
                            Console.WriteLine($"File '{relativePath}' is ignored due to custom ignore rule in parent directory: {matchedPattern}");
                            return isIncluded;
                        }
                    }
                    currentDir = Path.GetDirectoryName(currentDir);
                }
            }

            return isIncluded;
        }

        /// <summary>
        /// Creates a zip archive containing files not ignored by the rules
        /// </summary>
        public void CreateZipArchive(string zipFilePath)
        {
            List<string> filesToInclude = [];
            List<string> pathsIgnored = [];
            Console.WriteLine("Scanning files and directories...");
            
            // Iterate through all files and directories in the root folder and its subdirectories
            foreach (string path in Directory.GetFileSystemEntries(_rootFolder, "*", SearchOption.AllDirectories))
            {
                if (IsIncluded(path, out string matchedPattern))
                {
                    if (File.Exists(path))
                    {
                        filesToInclude.Add(path);
                        string relativePath = Path.GetRelativePath(_rootFolder, path);
                        Console.WriteLine($"Included file: {relativePath}");
                    }
                    // We don't need to explicitly include directories in the zip file
                }
                else
                {
                    string relativePath = Path.GetRelativePath(_rootFolder, path);
                    pathsIgnored.Add(relativePath);
                    Console.WriteLine($"Ignored: {relativePath}");
                }
            }

            Console.WriteLine($"\nCreating archive at {zipFilePath}...");
            // Delete existing archive if it exists
            if (File.Exists(zipFilePath))
            {
                Console.WriteLine($"Deleting existing archive at {zipFilePath}...");
                File.Delete(zipFilePath);
            }

            // Create the zip archive and add included files
            using ZipArchive archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create);
            foreach (string file in filesToInclude)
            {
                string relativePath = Path.GetRelativePath(_rootFolder, file);
                archive.CreateEntryFromFile(file, relativePath);
            }

            // Print summary
            Console.WriteLine($"Archive created successfully at {zipFilePath}\n");
            Console.WriteLine("Summary:");
            Console.WriteLine($"Total files included: {filesToInclude.Count}");
            Console.WriteLine($"Total paths ignored: {pathsIgnored.Count}");
        }

        /// <summary>
        /// Represents a single gitignore rule
        /// </summary>
        private class GitIgnoreRule
        {
            /// <summary>
            /// The compiled regex pattern for matching files
            /// </summary>
            public required Regex Pattern { get; set; }
            /// <summary>
            /// Indicates if this rule is a negation (inclusion) rule
            /// </summary>
            public bool IsNegation { get; set; }
            /// <summary>
            /// The base path where this rule applies
            /// </summary>
            public required string BasePath { get; set; }
            /// <summary>
            /// The original pattern string from the ignore file
            /// </summary>
            public required string OriginalPattern { get; set; }
            /// <summary>
            /// The sequence number for rule priority (lower numbers have higher priority)
            /// </summary>
            public int Sequence { get; set; }
        }
    }

    public static class Program
    {
        /// <summary>
        /// Entry point for the GitIgnoreCompressUtility program
        /// </summary>
        public static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: GitIgnoreCompressUtility <folder_path> <output_zip_path>");
                return;
            }

            string folderPath = args[0];
            string outputZipPath = args[1];

            try
            {
                GitIgnoreCompressUtility utility = new(folderPath);
                utility.LoadIgnorePatterns();
                utility.CreateZipArchive(outputZipPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
