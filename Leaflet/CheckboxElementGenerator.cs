namespace Leaflet;

using System.Text.RegularExpressions;
using Avalonia; 
using Avalonia.Controls;
using Avalonia.Layout; 
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

public class CheckboxElementGenerator : VisualLineElementGenerator
{
    // Matches markdown checkboxes like "- [ ]" or "- [x]"
    private static readonly Regex CheckboxRegex = new(@"^\s*-\s*\[([ xX])\]", RegexOptions.Compiled);

    public override int GetFirstInterestedOffset(int startOffset)
    {
        TextDocument document = CurrentContext.Document;
        if (startOffset >= document.TextLength)
            return -1;

        var line = document.GetLineByOffset(startOffset);
        while (line != null)
        {
            string lineText = document.GetText(line);
            var match = CheckboxRegex.Match(lineText);
            if (match.Success)
            {
                int offset = line.Offset + match.Index;
                if (offset >= startOffset)
                    return offset;
            }
            line = line.NextLine;
        }
        return -1; 
    }

    public override VisualLineElement? ConstructElement(int offset)
    {
        TextDocument document = CurrentContext.Document;
        var line = document.GetLineByOffset(offset);
        string lineText = document.GetText(line);
        var match = CheckboxRegex.Match(lineText); 
        
        if (match.Success && line.Offset + match.Index == offset)
        {
            bool isChecked = match.Groups[1].Value != " "; 
            int bracketOffset = offset + match.Groups[1].Index;

            // Create a lightweight, perfectly proportioned custom checkbox using Border
            var checkBox = new Border
            {
                Width = 14,
                Height = 14,
                CornerRadius = new CornerRadius(3),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1.5),
                Background = isChecked ? Brushes.DeepSkyBlue : Brushes.Transparent,
                Child = isChecked ? new TextBlock
                {
                    Text = "✓",
                    Foreground = Brushes.White,
                    FontSize = 10,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                } : null,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 6, 0),
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
            };

            // When clicked, toogle the character in the underlying document
            checkBox.PointerPressed += (sender, args) =>
            {
                bool newState = !isChecked;
                char targetChar = newState ? 'x' : ' '; 

                char docChar = document.GetCharAt(bracketOffset);
                if (docChar != targetChar && docChar != (newState ? 'X' : ' '))
                {
                    document.Replace(bracketOffset, 1, targetChar.ToString());
                }
            };

            // Replace the markdown bracket text with the interactive control element 
            return new InlineObjectElement(match.Length, checkBox); 
        }

        return null; 
    }
}