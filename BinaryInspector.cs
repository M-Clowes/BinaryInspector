using System.Text;

namespace BinaryTools;

public static class BinaryInspector
{
    private static readonly char[] _hexLookup = CreateHexLookup();
    private static readonly char[] _asciiLookup = CreateAsciiLookup();

    public static void PrintBytes(string fileName)
    {
        if (!File.Exists(fileName))
            throw new FileNotFoundException($"Unable to find file at: { fileName }");
        byte[] fileData = File.ReadAllBytes(fileName);
        
        // constants
        const string resetColor     = "\x1b[0m";
        const string offsetColor    = "\x1b[36m";
        const string bytesColor     = "\x1b[97m";
        const string nullBytesColor = "\x1b[90m";
        const string asciiColor     = "\x1b[32m";
        const string tableColor     = "\x1b[34m";

        const int offsetLength = 8;
        const int bytesLength = 48;
        const int asciiLength = 16;

        const long maxBuf = 300_000;

        // vars
        var offsetBar   = new string('\u2500', offsetLength + 2);
        var bytesBar    = new string('\u2500', bytesLength + 2);
        var asciiBar    = new string('\u2500', asciiLength + 2);
        var straightBar = $"{ tableColor }\u2502";

        // header
        Console.WriteLine($"{ tableColor }\u250c{ offsetBar }\u252c{ bytesBar }\u252c{ asciiBar }\u2510");
        Console.WriteLine(
            $"{ straightBar } "                                    +
            $"{ offsetColor }{ "Offset".PadRight(offsetLength) } " +
            $"{ straightBar } "                                    +
            $"{ bytesColor }{ "Bytes".PadRight(bytesLength) } "    +
            $"{ straightBar } "                                    +
            $"{ asciiColor }{ "ASCII".PadRight(asciiLength) } "    +
            $"{ straightBar }"
        );
        Console.WriteLine($"{ tableColor }\u251c{ offsetBar }\u2502{ bytesBar }\u2502{ asciiBar }\u2524");

        // body
        var ascii = new char[asciiLength];
        var sb    = new StringBuilder();

        Span<char> buf = stackalloc char[8];
        for (var i = 0; i < fileData.Length; i += asciiLength)
        {
            i.TryFormat(buf, out _, "X8");
            sb.Append($"{ straightBar } { offsetColor }{ buf } { straightBar } ");

            for (var j = 0; j < asciiLength; ++j)
            {
                var idx = i + j;

                if (idx < fileData.Length)
                {
                    sb.Append(fileData[idx] != 0x00 ? bytesColor : nullBytesColor);
                    sb.Append(_hexLookup, fileData[idx] * 2, 2);

                    ascii[j] = _asciiLookup[fileData[idx]];
                }
                else
                {
                    sb.Append("  ");

                    ascii[j] = ' ';
                }

                sb.Append(' ');
                if (j == 7)
                    sb.Append(' ');
            }

            sb.Append($"{ straightBar } { asciiColor }{ ascii.AsSpan() } ");
            sb.AppendLine(straightBar);

            if (sb.Length >= maxBuf)
            {
                Console.Write(sb.ToString());
                sb.Clear();
            }
        }

        // footer
        sb.AppendLine($"{ tableColor }\u2514{ offsetBar }\u2534{ bytesBar }\u2534{ asciiBar}\u2518{ resetColor }");
        Console.Write(sb.ToString());
    }

    private static char[] CreateHexLookup()
    {
        var table = new char[512];
        for (var i = 0; i < 256; ++i)
        {
            string s = i.ToString("X2");

            table[i * 2]     = s[0];
            table[i * 2 + 1] = s[1];
        }

        return table;
    }

    private static char[] CreateAsciiLookup()
    {
        var table = new char[256];
        for (var i = 0; i < 256; ++i)
            table[i] = (i >= 32 && i <= 126) ? (char)i : '.';

        return table;
    }
}