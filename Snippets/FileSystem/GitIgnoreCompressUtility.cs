using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
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
        /// <param name="currentFolder">The current folder being processed</param>
        private void LoadIgnorePatternsRecursively(string currentFolder)
        {
            // Process .gitignore files
            string[] gitIgnoreFiles = Directory.GetFiles(currentFolder, ".gitignore", SearchOption.TopDirectoryOnly);
            foreach (string gitIgnoreFile in gitIgnoreFiles)
            {
                Console.WriteLine($"Processing .gitignore file: {gitIgnoreFile}");
                List<GitIgnoreRule> rules = ParseIgnoreFile(gitIgnoreFile, currentFolder);
                _ignoreRules.AddRange(rules);
            }

            // Process .customignore files
            string[] customIgnoreFiles = Directory.GetFiles(currentFolder, ".customignore", SearchOption.TopDirectoryOnly);
            foreach (string customIgnoreFile in customIgnoreFiles)
            {
                Console.WriteLine($"Processing .customignore file: {customIgnoreFile}");
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
        /// <param name="filePath">Path to the ignore file</param>
        /// <param name="basePath">Base path for the ignore rules</param>
        /// <returns>List of parsed GitIgnoreRule objects</returns>
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

                // Check for negation patterns
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
                Console.WriteLine($"Loaded pattern: '{(isNegation ? "!" : "")}{trimmedLine}' as regex '{regex}' from file '{filePath}'");
            }
            return rules;
        }

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

        private bool IsIgnored(string filePath, out string matchedPattern)
        {
            string relativePath = Path.GetRelativePath(_rootFolder, filePath).Replace(@"\", "/");
            matchedPattern = string.Empty;
            bool isIncluded = false;
            bool parentIsIgnored = false;
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
                            parentIsIgnored = currentIsIncluded;
                        else
                            isIncluded = currentIsIncluded;
                        matchedPattern = rule.OriginalPattern;
                        int lastMatchSequence = rule.Sequence;
                        if (rule.Sequence == -1)
                        {
                            isIncluded = false;
                            Console.WriteLine($"File '{filePath}' is ignored due to custom ignore rule: {matchedPattern}");
                        }
                    }
                }
                if (parentIsIgnored)
                {
                    isIncluded = true;
                    matchedPattern = $"Parent directory '{subPath}' is ignored";
                    Console.WriteLine($"File '{filePath}' is ignored because parent directory is ignored: {matchedPattern}");
                    break;
                }
            }

            return !isIncluded;
        }

        /// <summary>
        /// Creates a zip archive containing files not ignored by the rules
        /// </summary>
        /// <param name="zipFilePath">Path where the zip archive will be created</param>
        public void CreateZipArchive(string zipFilePath)
        {
            List<string> filesToInclude = [];
            List<string> filesIgnored = [];
            Console.WriteLine("Scanning files...");
            
            // Iterate through all files in the root folder and its subdirectories
            foreach (string file in Directory.GetFiles(_rootFolder, "*", SearchOption.AllDirectories))
            {
                if (IsIgnored(file, out string matchedPattern))
                {
                    string relativePath = Path.GetRelativePath(_rootFolder, file);
                    filesIgnored.Add(relativePath);
                    Console.WriteLine($"Ignored: {relativePath} (Matched pattern: {matchedPattern})");
                }
                else
                {
                    filesToInclude.Add(file);
                    string relativePath = Path.GetRelativePath(_rootFolder, file);
                    Console.WriteLine($"Included: {relativePath}");
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
            Console.WriteLine($"Total files ignored: {filesIgnored.Count}");
        }

        /// <summary>
        /// Represents a single gitignore rule
        /// </summary>
        private class GitIgnoreRule
        {
            // The compiled regex pattern for matching files
            public required Regex Pattern { get; set; }
            // Indicates if this rule is a negation (inclusion) rule
            public bool IsNegation { get; set; }
            // The base path where this rule applies
            public required string BasePath { get; set; }
            // The original pattern string from the ignore file
            public required string OriginalPattern { get; set; }
            // The sequence number for rule priority (lower numbers have higher priority)
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
