using PdfSharp.Fonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PdfSharp.Snippets.Font.SegoeWpFontResolver;
using System.Windows.Documents;
using System.IO;

namespace Tax_Liability_Forecast_App.Utils
{
    public class WindowsFontResolver : IFontResolver
    {
        private static readonly string FontsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

        private static readonly Dictionary<string, string> FontFileMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Arial", "arial.ttf" },
            { "Arial-Bold", "arialbd.ttf" },
            { "Arial-Italic", "ariali.ttf" },
            { "Arial-BoldItalic", "arialbi.ttf" },
            { "Verdana", "verdana.ttf" },
            { "Verdana-Bold", "verdanab.ttf" },
            { "Verdana-Italic", "verdanai.ttf" },
            { "Verdana-BoldItalic", "verdanaz.ttf" },
            { "Times", "times.ttf" },
            { "Times-Bold", "timesbd.ttf" },
            { "Times-Italic", "timesi.ttf" },
            { "Times-BoldItalic", "timesbi.ttf" },
            { "Courier", "cour.ttf" },
            { "Courier-Bold", "courbd.ttf" },
            { "Courier-Italic", "couri.ttf" },
            { "Courier-BoldItalic", "courbi.ttf" }
        };

        public byte[]? GetFont(string faceName)
        {
            if (!FontFileMap.TryGetValue(faceName, out string fileName))
            {
                fileName = "arial.ttf";
            }

            string fontPath = Path.Combine(FontsFolder, fileName);

            if (!File.Exists(fontPath))
            {
                throw new FileNotFoundException($"Font file '{fileName}' not found in Windows Fonts directory.");
            }

            return File.ReadAllBytes(fontPath);
        }

        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            string key = familyName;

            if (isBold && isItalic)
            {
                key += "-BoldItalic";
            }
            else if (isBold)
            {
                key += "-Bold";
            }
            else if (isItalic)
            {
                key += "-Italic";
            }

            if (FontFileMap.ContainsKey(key))
            {
                return new FontResolverInfo(key);
            }

            return new FontResolverInfo("Arial");
        }
    }
}
