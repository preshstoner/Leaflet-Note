namespace Leaflet;

using System.Text.RegularExpressions;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;

public class MarkdownHighlightTransformer : DocumentColorizingTransformer
{
    private readonly TextArea _textArea;
    private static readonly Regex BoldRegex = new(@"\*\*(.*?)\*\*", RegexOptions.Compiled);
    private static readonly Regex ItalicRegex = new(@"\*(.*?)\*", RegexOptions.Compiled); 

    public MarkdownHighlightTransformer(TextArea textArea)
    {
        _textArea = textArea;
    }

    protected override void ColorizeLine(DocumentLine line)
    {
        int lineNum = line.LineNumber;
        int activeCaretLine = _textArea.Caret.Line; 

        // If the cursor is on this line, keep it raw and unstyled for easy editing
        if (lineNum == activeCaretLine)
        {
            return;
        }

        // On inactive lines, parse and apply live-preview styling
        string text = CurrentContext.Document.GetText(line);
        int lineStartOffset = line.Offset; 

        // Handle Headers
        if (text.StartsWith("#"))
        {
            int hashCount = 0;
            while (hashCount < text.Length && text[hashCount] == '#')
            {
                hashCount++;
            }

            // Dim or hide the '#' markdown symbols on inactive lines
            ChangeLinePart(lineStartOffset, lineStartOffset + hashCount, element =>
            {
                // Set to a subtle gray (or Brushes.Transparent to completely hide them)
                element.TextRunProperties.SetForegroundBrush(Brushes.DimGray);
            }); 


            // Style H1 headers (larger appearance, distinct color)
            if(lineStartOffset + hashCount < line.EndOffset)
            {
                ChangeLinePart(lineStartOffset + hashCount, line.EndOffset, element =>
                {
                    element.TextRunProperties.SetForegroundBrush(Brushes.DeepSkyBlue);

                    var tf = element.TextRunProperties.Typeface;
                    element.TextRunProperties.SetTypeface(new Typeface(tf.FontFamily, tf.Style, FontWeight.Bold, tf.Stretch));
                });
            }
            return;
        }

        // Handles Inline Bold (**text**)
        foreach (Match match in BoldRegex.Matches(text))
        {
            int matchStart = lineStartOffset + match.Index;
            int matchEnd = matchStart + match.Length;

            // Dim opening '**'
            ChangeLinePart(matchStart, matchStart + 2, element =>
            {
                element.TextRunProperties.SetForegroundBrush(Brushes.DimGray);
            });

            // Style inner bold content
            ChangeLinePart(matchStart + 2, matchEnd - 2, element =>
            {
                var tf = element.TextRunProperties.Typeface;
                element.TextRunProperties.SetTypeface(new Typeface(tf.FontFamily, tf.Style, FontWeight.Bold, tf.Stretch)); 
            });

            // Dim closing '**'
            ChangeLinePart(matchEnd - 2, matchEnd, element =>
            {
                element.TextRunProperties.SetForegroundBrush(Brushes.DimGray); 
            });
        }

        // Handle Inline Italic ("text*)
        foreach (Match match in ItalicRegex.Matches(text))
        {
            if (match.Index > 0 && text[match.Index - 1] == '*') continue;
            if (match.Index + match.Length < text.Length && text[match.Index + match.Length] == '*') continue; 

            int matchStart = lineStartOffset + match.Index;
            int matchEnd = matchStart + match.Length;

            // Dim opening '*'
            ChangeLinePart(matchStart, matchStart + 1, element =>
            {
                element.TextRunProperties.SetForegroundBrush(Brushes.DimGray); 
            }); 

            // Style inner italic content
            ChangeLinePart(matchStart + 1, matchEnd - 1, element =>
            {
                var tf = element.TextRunProperties.Typeface;
                element.TextRunProperties.SetTypeface(new Typeface(tf.FontFamily, FontStyle.Italic, tf.Weight, tf.Stretch));
            });

            // Dim closing '*'
            ChangeLinePart(matchEnd - 1, matchEnd, element =>
            {
                element.TextRunProperties.SetForegroundBrush(Brushes.DimGray); 
            }); 
        }
    }
}