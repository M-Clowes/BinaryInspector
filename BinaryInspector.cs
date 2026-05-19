
using Spectre.Console;
using System.Text;

namespace BinaryTools;

public static class BinaryInspector
{
    public static void PrintBytes(string fileName)
    {
        byte[] rawData = File.ReadAllBytes(fileName);

        const int offsetSpacer   = 8;
        const int bytesSpacer    = 48;
        const int asciiSpacer    = 16;

        const ConsoleColor structColor    = ConsoleColor.Blue;
        const ConsoleColor offsetColor    = ConsoleColor.Cyan;
        const ConsoleColor bytesColor     = ConsoleColor.White;
        const ConsoleColor nullBytesColor = ConsoleColor.Gray;
        const ConsoleColor asciiColor     = ConsoleColor.Green;

        AnsiConsole.MarkupLine(
            $"[{ offsetColor }]{ "Offset".PadRight(offsetSpacer) }[/] " +
            $"[{ bytesColor }]{ "Bytes".PadRight(bytesSpacer) }[/] " +
            $"[{ structColor }]| [/]" +
            $"[{ asciiColor }]{ "ASCII".PadRight(asciiSpacer) }[/]"
        );
        AnsiConsole.MarkupLine(
            $"[{ structColor }]{ new string('-', offsetSpacer) } { new string('-', bytesSpacer) } | { new string('-', asciiSpacer) }[/]"
        );

        for (var i = 0; i < rawData.Length; i += 16)
        {
            var offset       = $"[{ offsetColor }]{ i.ToString("X8") }[/]";
            var hexBuilder   = new StringBuilder();
            var asciiBuilder = new StringBuilder();

            for (var j = 0; j < 16; ++j)
            {
                var idx = i + j;

                if (idx < rawData.Length)
                {
                    hexBuilder.Append(
                        $"[{ ((rawData[idx] == 0x00) ? nullBytesColor : bytesColor)}]{ rawData[idx].ToString("X2") }[/]"
                    );
                    hexBuilder.Append(new string(' ', (j == 7) ? 2 : 1));

                    char c = (char)rawData[idx];
                    asciiBuilder.Append((c < 32 || c > 126) ? '.' : c);
                }
                else
                {
                    hexBuilder.Append(new string(' ', (j == 7) ? 4 : 3));
                    asciiBuilder.Append(' ');
                }
            }

            var ascii = Markup.Escape($"{ asciiBuilder }");
            var bytes = $"{ hexBuilder }\b";

            AnsiConsole.MarkupLine($"{ offset } { bytes } [{ structColor }]|[/] [{ asciiColor }]{ ascii }[/]");
        }

        var padding = new string('─', (bytesSpacer + asciiSpacer) / 2);
        AnsiConsole.MarkupLine($"[{ nullBytesColor }]└{ padding } END READ { padding }┘[/]");
    }
}