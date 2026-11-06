namespace Leaflet;

using System;
using System.IO;
using Avalonia.Threading;
using AvaloniaEdit;

public class AutosaveManager
{
    private readonly TextEditor _editor;
    private readonly string _draftPath;
    private readonly string _lockPath;
    private readonly DispatcherTimer _debounceTimer; 

    public AutosaveManager(TextEditor editor, string vaultPath)
    {
        _editor = editor;
        _draftPath = Path.Combine(vaultPath, ".leaflet_autosave.md");
        _lockPath = Path.Combine(vaultPath, ".leaflet.lock"); 

        // 1.5-second debounce timer
        _debounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1500)
        };

        _debounceTimer.Tick += (sender, e) =>
        {
            _debounceTimer.Stop();
            SaveDraft(); 
        };

        _editor.TextChanged += (sender, e) =>
        {
            _debounceTimer.Stop();
            _debounceTimer.Start(); 
        }; 
    }

    public void AcquireLock()
    {
        File.WriteAllText(_lockPath, DateTime.UtcNow.ToString("o")); 
    }

    public void ReleaseLock()
    {
        if (File.Exists(_lockPath))
        {
            File.Delete(_lockPath);
        }
        if (File.Exists(_draftPath))
        {
            File.Delete(_draftPath);
        }
    }

    public bool CheckForCrash(out string draftContent)
    {
        draftContent = string.Empty;
        // If lock file exists on startup, the previous session crashed or terminated unexpectedly
        if (File.Exists(_lockPath) && File.Exists(_draftPath))
        {
            draftContent = File.ReadAllText(_draftPath); 
            return true;
        }
        return false; 
    }

    private void SaveDraft()
    {
        try
        {
            File.WriteAllText(_draftPath, _editor.Text);
        }
        catch
        {
            // Suppress IO write exceptions during heavy edits
        }
    }
}