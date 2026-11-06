namespace Leaflet;

using System;
using System.IO;
using Avalonia.Controls;
using AvaloniaEdit; // Required for AvaloniaEdit

public partial class MainWindow : Window
{
    private VaultStore _vault;
    private TextEditor _textEditor; // Declare the editor
    private AutosaveManager _autosaveManager; 
    private string _vaultPath; 

    public MainWindow()
    {
        InitializeComponent();

        // Set up Vault path and store first
        _vaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LeafletVault");
        Directory.CreateDirectory(_vaultPath);
        _vault = new VaultStore(_vaultPath);
        // Scan the folder for markdown files on startup
        _vault.ScanVault();


        // Initialize TextEditor FIRST so it is guaranteed non-null
        _textEditor = new TextEditor
        {
            ShowLineNumbers = true,
            FontFamily = new Avalonia.Media.FontFamily("Consolas"),
            FontSize = 14,
            Text = "# Welcome to Leaflet\n\nStart typing your Markdown notes here..."
        };
        // Set the TextEditor as the content of the Window
        Content = _textEditor; 

        // Now initilize AutosaveManager safely with the guaranteed text editor
        _autosaveManager = new AutosaveManager(_textEditor, _vaultPath);

        // Check if previous session ended in a crash
        if (_autosaveManager.CheckForCrash(out string recoveredText))
        {
            _textEditor.Text = recoveredText;
            // Optional: You could show a subtle notification banner that the draft was restored
        }

        _autosaveManager.AcquireLock(); 


        // Clean up lock and draft cleanly when window closes
        Closed += (sender, e) =>
        {
            _autosaveManager.ReleaseLock(); 
        };


        // Register your transformers, keyboard shortcuts, and element generators below...
        _textEditor.TextArea.TextView.LineTransformers.Add(new MarkdownHighlightTransformer(_textEditor.TextArea)); 

        _textEditor.TextArea.Caret.PositionChanged += (sender, e) =>
        {
            _textEditor.TextArea.TextView.Redraw();
        };

        _textEditor.TextArea.TextView.ElementGenerators.Add(new CheckboxElementGenerator());      

        _textEditor.KeyDown += (sender, e) =>
        {
            var textArea = _textEditor.TextArea;

            // Ctrl + B for Bold
            if (e.KeyModifiers == Avalonia.Input.KeyModifiers.Control && e.Key == Avalonia.Input.Key.B)
            {
                if (textArea.Selection.IsEmpty)
                {
                    int offset = textArea.Caret.Offset;
                    textArea.Document.Insert(offset, "****");
                    textArea.Caret.Offset = offset + 2; // Place cursor between astrisks
                }
                else
                {
                    string selectedText = textArea.Selection.GetText();
                    int startOffset = textArea.Selection.SurroundingSegment.Offset;
                    int length = textArea.Selection.SurroundingSegment.Length;
                    textArea.Document.Replace(startOffset, length, $"**{selectedText}**");
                }
                e.Handled = true; 
            }
            // Ctrl + I for Italic
            else if (e.KeyModifiers == Avalonia.Input.KeyModifiers.Control && e.Key == Avalonia.Input.Key.I)
            {
                if(textArea.Selection.IsEmpty)
                {
                    int offset = textArea.Caret.Offset;
                    textArea.Document.Insert(offset, "**");
                    textArea.Caret.Offset = offset + 1; // Place cursor between asterisks
                }
                else
                {
                    string selectedText = textArea.Selection.GetText();
                    int startOffset = textArea.Selection.SurroundingSegment.Offset;
                    int length = textArea.Selection.SurroundingSegment.Length;
                    textArea.Document.Replace(startOffset, length, $"*{selectedText}*");
                }
                e.Handled = true;
            }
            // Ctrl + P for Headless PDF Export
            else if (e.KeyModifiers == Avalonia.Input.KeyModifiers.Control && e.Key == Avalonia.Input.Key.P)
            {
                string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"); 
                Directory.CreateDirectory(downloadsPath); // Ensure folder exists
                string outputPath = Path.Combine(downloadsPath, "Leaflet_Export.pdf");

                PdfExportService.ExportMarkdownToPdf(_textEditor.Text, outputPath);
                e.Handled = true; 
            }
        }; 
    }
}