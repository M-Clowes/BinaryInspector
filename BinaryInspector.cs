using System.Text;

namespace BinaryTools;

public static class BinaryInspector
{
    private static readonly char[] _hexLookup = CreateHexLookup();
    private static readonly char[] _asciiLookup = CreateAsciiLookup();

    public static void PrintAllBytes(string fileName)
    {
        if (!File.Exists(fileName))
            throw new FileNotFoundException($"Unable to find file at: { fileName }");
        long fileSize = new FileInfo(fileName).Length;
        long maxPageCount    = (fileSize + PAGE_SIZE - 1) / PAGE_SIZE;
        long pageNum         = 0;
        int maxDigits        = maxPageCount == 0 ? 1 : (int)Math.Log10(maxPageCount) + 1;

        using var stream = File.OpenRead(fileName); 

        ConsoleKeyInfo input;
        do
        {
            Console.Clear();

            Console.WriteLine($"{ TABLE_COLOR }\u250c{ offsetBar }\u252c{ bytesBar }\u252c{ asciiBar }\u2510");
            Console.WriteLine(
                $"{ straightBar } "                                      +
                $"{ OFFSET_COLOR }{ "Offset".PadRight(OFFSET_LENGTH) } " +
                $"{ straightBar } "                                      +
                $"{ BYTES_COLOR }{ "Bytes".PadRight(BYTES_LENGTH) } "    +
                $"{ straightBar } "                                      +
                $"{ ASCII_COLOR }{ "ASCII".PadRight(ASCII_LENGTH) } "    +
                $"{ straightBar }"
            );
            Console.WriteLine($"{ TABLE_COLOR }\u251c{ offsetBar }\u253c{ bytesBar }\u253c{ asciiBar }\u2524");
        
            PrintBytesPage(stream, pageNum);

            Console.WriteLine($"{ TABLE_COLOR }\u2514{ offsetBar }\u2534{ bytesBar }\u2534{ asciiBar }\u2518");
            Console.WriteLine($"{ RESET_COLOR }<= { NULL_BYTES_COLOR }Page { pageNum + 1 } / { maxPageCount }{ RESET_COLOR } =>");
            Console.WriteLine(
                $"{ NULL_BYTES_COLOR }Press "             +
                $"{ RESET_COLOR }LeftArrow "              +
                $"{ NULL_BYTES_COLOR }to go back, "       +
                $"{ RESET_COLOR }RightArrow "             +
                $"{ NULL_BYTES_COLOR }to go forward or " +
                $"{ RESET_COLOR }ESC "                    +
                $"{ NULL_BYTES_COLOR }to quit."
            );
            Console.Write($"Seek page:{ RESET_COLOR } ");

            var buf = new char[maxDigits];
            int len = 0;
            while (true)
            {
                input = Console.ReadKey(true);

                switch (input.Key)
                {
                    case ConsoleKey.Escape:
                        break;
                    
                    case ConsoleKey.Backspace:
                        if (len > 0)
                        {
                            Console.Write("\b \b");
                            buf[--len] = ' ';
                        }
                        continue;

                    case ConsoleKey.Enter:
                        var page = 0;
                        for (var i = 0; i < len; ++i)
                            page = page * 10 + (buf[i] - '0');
                        pageNum = Math.Clamp(page - 1, 0, maxPageCount - 1);
                        break;

                    case ConsoleKey.LeftArrow:
                        pageNum = Math.Max(pageNum - 1, 0);
                        break;

                    case ConsoleKey.RightArrow:
                        pageNum = Math.Min(pageNum + 1, maxPageCount - 1);
                        break;

                    default:
                        if (char.IsDigit(input.KeyChar) && len < buf.Length)
                        {
                            buf[len++] = input.KeyChar;
                            Console.Write(input.KeyChar);
                        }
                        continue;
                }

                break;
            }
        } while (input.Key != ConsoleKey.Escape);
    }

    private static void PrintBytesPage(FileStream stream, long pageNum)
    {
        var fileData = new byte[PAGE_SIZE];

        stream.Seek(pageNum * PAGE_SIZE, SeekOrigin.Begin);
        int bytesRead = stream.Read(fileData, 0, fileData.Length);

        var ascii = new char[ASCII_LENGTH];
        var sb    = new StringBuilder();

        Span<char> buf = stackalloc char[8];
        for (var i = 0; i < bytesRead; i += ASCII_LENGTH)
        {
            i.TryFormat(buf, out _, "X8");
            sb.Append($"{ straightBar } { OFFSET_COLOR }{ buf } { straightBar } ");

            for (var j = 0; j < ASCII_LENGTH; ++j)
            {
                var idx = i + j;

                if (idx < fileData.Length)
                {
                    sb.Append(fileData[idx] != 0x00 ? BYTES_COLOR : NULL_BYTES_COLOR);
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

            sb.Append($"{ straightBar } { ASCII_COLOR }{ ascii.AsSpan() } ");
            sb.AppendLine(straightBar);

            if (sb.Length >= MAX_BUF)
            {
                Console.Write(sb.ToString());
                sb.Clear();
            }
        }
        
        if (sb.Length > 0)
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

    private const string RESET_COLOR      = "\x1b[0m";
    private const string OFFSET_COLOR     = "\x1b[36m";
    private const string BYTES_COLOR      = "\x1b[97m";
    private const string NULL_BYTES_COLOR = "\x1b[90m";
    private const string ASCII_COLOR      = "\x1b[32m";
    private const string TABLE_COLOR      = "\x1b[34m";

    private const int OFFSET_LENGTH = 8;
    private const int BYTES_LENGTH  = 48;
    private const int ASCII_LENGTH  = 16;

    private const long MAX_BUF = 300_000;
    private const long PAGE_SIZE = 32_000;

    private static readonly string offsetBar   = new('\u2500', OFFSET_LENGTH + 2);
    private static readonly string bytesBar    = new('\u2500', BYTES_LENGTH + 2);
    private static readonly string asciiBar    = new('\u2500', ASCII_LENGTH + 2);
    private static readonly string straightBar = $"{ TABLE_COLOR }\u2502";
}