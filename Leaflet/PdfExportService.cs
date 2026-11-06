namespace Leaflet; 

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SkiaSharp; 

public class PdfExportService
{
    // A4 dimenisons in points as 72 DPI: 595.27 x 841.89
    private const float PageWidth = 595.27f;
    private const float PageHeight = 841.89f;
    private const float Margin = 54.0f; // 0.75 inch margin

    public static void ExportMarkdownToPdf(string markdownContent, string outputPath)
    {
        // Use safe system typeface to prevent native SkiaSharp crashes
        using var typeface = SKTypeface.FromFamilyName("Segoe UI") ?? SKTypeface.Default; 
        
        using var stream = new SKFileWStream(outputPath);
        using var document = SKDocument.CreatePdf(stream); 

        using var paint = new SKPaint
        {
            IsAntialias = true,
            Color = SKColors.Black
        };

        using var headerPaint = new SKPaint
        {
            IsAntialias = true,
            Color = SKColors.DeepSkyBlue
        }; 

        // Typography and sizing go to SKFont
        using var font = new SKFont
        {
            Typeface = typeface,
            Size = 12
        };

        using var headerFont = new SKFont
        {
            Typeface = SKTypeface.FromFamilyName(typeface.FamilyName, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright),
            Size = 20
        }; 


        float y = Margin;
        float printableWidth = PageWidth - (Margin * 2);
        float lineHeight = 18f;
        float headerLineHeight = 28f; 


        SKCanvas canvas = document.BeginPage(PageWidth, PageHeight);

        string[] lines = markdownContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None); 

        foreach (var rawLine in lines)
        {
            bool isHeader = rawLine.StartsWith("#");
            string lineText = isHeader ? rawLine.TrimStart('#').Trim() : rawLine;
            var currentFont = isHeader ? headerFont : font;
            var currentPaint = isHeader ? headerPaint : paint;
            float currentLineHeight = isHeader ? headerLineHeight : lineHeight; 

            // Word wrapping logic
            var wrappedLines = WrapText(lineText, currentFont, printableWidth); 

            foreach (var wrappedLine in wrappedLines)
            {
                if (y + currentLineHeight > PageHeight - Margin)
                {
                    document.EndPage();
                    canvas = document.BeginPage(PageWidth, PageHeight);
                    y = Margin; 
                }

                canvas.DrawText(wrappedLine, Margin, y + currentFont.Size, SKTextAlign.Left, currentFont, currentPaint);
                y += currentLineHeight; 
            }
        }

        document.EndPage(); 
        document.Close(); 
    }

    private static List<string> WrapText(string text, SKFont font, float maxWidth)
    {
        var result = new List<string>();
        if (string.IsNullOrEmpty(text))
        {
            result.Add(string.Empty);
            return result;
        }

        string[] words = text.Split(' ');
        StringBuilder currentLine = new StringBuilder(); 
        
        foreach (string word in words)
        {
            string testLine = currentLine.Length == 0 ? word : currentLine + " " + word;
            float width = font.MeasureText(testLine);

            if (width > maxWidth && currentLine.Length > 0)
            {
                result.Add(currentLine.ToString());
                currentLine.Clear();
                currentLine.Append(word); 
            }
            else
            {
                currentLine.Clear();
                currentLine.Append(testLine);
            }
        }

        if (currentLine.Length > 0)
        {
            result.Add(currentLine.ToString());
        }

        return result; 
    }

    // private static byte[] GetLeanSubsetFontBytes()
    // {
    //     string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "sequi.ttf"); 
    //     if (!File.Exists(fontPath))
    //     {
    //         fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf"); 
    //     }
    //     if (!File.Exists(fontPath)) return null;

    //     byte[] fontBytes = File.ReadAllBytes(fontPath);
    //     return SubsetFontTables(fontBytes); 
    // }

    // private static byte[] SubsetFontTables(byte[] fontBytes)
    // {
    //     using var ms = new MemoryStream(fontBytes);
    //     using var reader = new BinaryReader(ms); 

    //     if (fontBytes.Length < 12) return fontBytes;

    //     uint sfntVersion = reader.ReadUInt32();
    //     ushort numTables = reader.ReadUInt16();
    //     ushort searchRange = reader.ReadUInt16();
    //     ushort entrySelector = reader.ReadUInt16(); 
    //     ushort rangeShift = reader.ReadUInt16(); 

    //     var tables = new List<(uint tag, uint checkSum, uint offset, uint length, byte[] data)>(); 

    //     for (int i = 0; i < numTables; i++)
    //     {
    //         uint tag = reader.ReadUInt32();
    //         uint checkSum = reader.ReadUInt32();
    //         uint offset = reader.ReadUInt32();
    //         uint length = reader.ReadUInt32(); 

    //         long originalPos = ms.Position;
    //         ms.Position = offset;
    //         byte[] data = reader.ReadBytes((int)length);
    //         ms.Position = originalPos; 

    //         // Strip heavy layout tables (GSUB, GPOS, DSIG, JSTF) to shrink font size
    //         string tagName = Encoding.ASCII.GetString(BitConverter.GetBytes(tag)).Trim('\0');
    //         if (tagName == "GSUB" || tagName == "GPOS" || tagName == "DSIG" || tagName == "JSTF")
    //         {
    //             continue;
    //         }

    //         tables.Add((tag, checkSum, offset, length, data)); 
    //     }

    //     using var outMs = new MemoryStream();
    //     using var writer = new BinaryWriter(outMs);

    //     writer.Write(sfntVersion);
    //     ushort newNumTables = (ushort)tables.Count; 
    //     writer.Write(newNumTables);

    //     ushort sRange = 1;
    //     ushort eSel = 0;
    //     while (sRange * 2 <= newNumTables)
    //     {
    //         sRange *= 2;
    //         eSel++;
    //     }
    //     sRange *= 16; 
    //     ushort rShift = (ushort)(newNumTables * 16 - sRange); 

    //     writer.Write(sRange); 
    //     writer.Write(eSel);
    //     writer.Write(rShift);

    //     uint currentOffset = (uint)(12 + newNumTables * 16);
    //     foreach (var t in tables)
    //     {
    //         writer.Write(t.tag);
    //         writer.Write(t.checkSum);
    //         writer.Write(currentOffset); 
    //         writer.Write((uint)t.data.Length);
    //         currentOffset += (uint)((t.data.Length + 3) & ~3); 
    //     }

    //     foreach (var t in tables)
    //     {
    //         writer.Write(t.data);
    //         int padding = (int)((4 - (t.data.Length % 4)) % 4);
    //         for (int p = 0; p < padding; p++) writer.Write((byte)0);
    //     }

    //     return outMs.ToArray(); 
    // }
}