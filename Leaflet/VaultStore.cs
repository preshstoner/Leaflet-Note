namespace Leaflet;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


public class VaultStore
{
	private readonly string _rootPath;
	private readonly Dictionary<string, MarkdownNote> _notes = new(StringComparer.OrdinalIgnoreCase); 

	public VaultStore(string rootPath)
	{
		_rootPath = rootPath;
	}

	public void ScanVault()
	{
		// Get all .md files, ignoring folders starting with '.' (like .git, .obsidian)
		var mdFiles = Directory.EnumerateFiles(_rootPath, "*.md", SearchOption.AllDirectories)
			.Where(file => !Path.GetRelativePath(_rootPath, file).Split(Path.DirectorySeparatorChar).Any(dir => dir.StartsWith('.')));

		foreach (var filePath in mdFiles)
		{
			LoadNote(filePath);
		}
	}

	// ✅ Atomic save method placed directly inside VaultStore
	public static void SafeSaveNote(string targetFilePath, string newContent)
	{
		string directory = Path.GetDirectoryName(targetFilePath)!;
		string tempFilePath = Path.Combine(directory, $"{Guid.NewGuid()}.tmp");

		// 1. Write to temp file first
		File.WriteAllText(tempFilePath, newContent);

		// 2. Atomically replace the original file
		File.Move(tempFilePath, targetFilePath, overwrite: true);
	}

	public void InitializeWatcher()
	{
		var watcher = new FileSystemWatcher(_rootPath)
		{
			Filter = "*.md",
			IncludeSubdirectories = true,
			EnableRaisingEvents = true
		};

		watcher.Changed += (s, e) => { /* Handle external modifications */ };
		watcher.Renamed += (s, e) => { /* Handle file renames & update [[links]] */};
	}

	private void LoadNote(string filePath)
	{
		string content = File.ReadAllText(filePath);
		string relativeName = Path.GetRelativePath(_rootPath, filePath);

		// Parse headers, [[wikilinks]], and tags here using Regex or a lightweight parser
	}
}

// Placeholder model for individual notes
public class MarkdownNote
{
	public string FilePath { get; set; } = string.Empty;
	public string Content { get; set; } = string.Empty;
}
