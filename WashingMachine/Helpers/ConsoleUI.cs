namespace WashingMachine.Helpers;

/// <summary>
/// Centralised console UI helper — all borders, colors, panels and progress bars
/// live here so Views never duplicate rendering code.
/// Demonstrates Single Responsibility: this class owns ONLY rendering utilities.
/// </summary>
public static class ConsoleUI
{
    // ── Palette ──────────────────────────────────────────────────────────
    public static readonly ConsoleColor Accent    = ConsoleColor.Cyan;
    public static readonly ConsoleColor Success   = ConsoleColor.Green;
    public static readonly ConsoleColor Warning   = ConsoleColor.Yellow;
    public static readonly ConsoleColor Error     = ConsoleColor.Red;
    public static readonly ConsoleColor Muted     = ConsoleColor.DarkGray;
    public static readonly ConsoleColor Highlight = ConsoleColor.White;
    public static readonly ConsoleColor Active    = ConsoleColor.Cyan;

    public const int PanelWidth = 62;

    // ── Low-level helpers ─────────────────────────────────────────────────

    public static void Write(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }

    public static void WriteLine(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    public static void WriteLineCenter(string text, ConsoleColor color, int width = PanelWidth)
    {
        var padded = text.Length >= width ? text : text.PadLeft((width + text.Length) / 2).PadRight(width);
        Console.ForegroundColor = color;
        Console.WriteLine(padded);
        Console.ResetColor();
    }

    // ── Box drawing ───────────────────────────────────────────────────────

    public static void DrawTopBorder()
    {
        Write("╭" + new string('─', PanelWidth) + "╮\n", Accent);
    }

    public static void DrawBottomBorder()
    {
        Write("╰" + new string('─', PanelWidth) + "╯\n", Accent);
    }

    public static void DrawSeparator()
    {
        Write("├" + new string('─', PanelWidth) + "┤\n", Accent);
    }

    /// <summary>Draws a row: "│ {content,-width} │"</summary>
    public static void DrawRow(string content = "")
    {
        Write("│ ", Accent);
        var inner = PanelWidth - 2;
        var line  = content.Length > inner ? content[..inner] : content.PadRight(inner);
        Console.Write(line);
        Write(" │\n", Accent);
    }

    /// <summary>Draws a row with a labeled field: "│  Label  : Value │"</summary>
    public static void DrawField(string label, string value,
        ConsoleColor labelColor  = default,
        ConsoleColor valueColor  = default)
    {
        if (labelColor == default) labelColor  = Muted;
        if (valueColor == default) valueColor  = Highlight;

        Write("│  ", Accent);
        Write($"{label,-14}: ", labelColor);
        var inner   = PanelWidth - 2 - 2 - 14 - 2;  // "│  " + label + ": "
        var valStr  = value.Length > inner ? value[..inner] : value.PadRight(inner);
        Write(valStr, valueColor);
        Write(" │\n", Accent);
    }

    public static void DrawBlankRow() => DrawRow();

    // ── Title banner ──────────────────────────────────────────────────────

    public static void DrawTitle(string line1, string line2 = "")
    {
        DrawTopBorder();
        DrawBlankRow();
        WriteRow(line1, Accent);
        if (!string.IsNullOrWhiteSpace(line2))
            WriteRow(line2, Muted);
        DrawBlankRow();
    }

    private static void WriteRow(string text, ConsoleColor color)
    {
        Write("│", Accent);
        var inner   = PanelWidth;
        var padded  = text.Length >= inner ? text : text.PadLeft((inner + text.Length) / 2).PadRight(inner);
        Write(padded, color);
        Write("│\n", Accent);
    }

    // ── Progress bar ──────────────────────────────────────────────────────

    public static void DrawProgressBar(double percent, int barWidth = 40)
    {
        var filled = (int)(percent / 100.0 * barWidth);
        var empty  = barWidth - filled;
        Write("│  ", Accent);
        Write(new string('█', filled), Accent);
        Write(new string('░', empty),  Muted);
        Write($" {percent,5:F1}%", Highlight);
        // pad remaining
        var totalUsed = 4 + barWidth + 7; // "│  " + bar + " XX.X%"
        var pad = PanelWidth - totalUsed;
        Console.Write(new string(' ', Math.Max(0, pad)));
        Write(" │\n", Accent);
    }

    // ── Message panels ────────────────────────────────────────────────────

    public static void ShowSuccess(string message)
    {
        Console.WriteLine();
        DrawTopBorder();
        DrawRow($"  ✓  {message}");
        DrawBottomBorder();
        Console.WriteLine();
    }

    public static void ShowError(string title, string message)
    {
        Console.WriteLine();
        Write("╭" + new string('─', PanelWidth) + "╮\n", Error);
        Write("│", Error);
        var inner  = PanelWidth;
        var header = $"  ⚠  {title}";
        Write(header.PadRight(inner), Error);
        Write("│\n", Error);
        Write("├" + new string('─', PanelWidth) + "┤\n", Error);

        foreach (var line in message.Split('\n'))
        {
            Write("│  ", Error);
            var l = line.TrimEnd().Length > inner - 3
                ? line.TrimEnd()[..(inner - 3)]
                : line.TrimEnd().PadRight(inner - 3);
            Console.Write(l);
            Write("  │\n", Error);
        }

        DrawBlankRow();
        DrawRow("                        [ Enter ] to continue");
        Write("╰" + new string('─', PanelWidth) + "╯\n", Error);
        Console.WriteLine();
        Console.ReadLine();
    }

    public static void ShowInfo(string title, string message)
    {
        Console.WriteLine();
        Write("╭" + new string('─', PanelWidth) + "╮\n", Warning);
        Write("│  ", Warning);
        Write(("ℹ  " + title).PadRight(PanelWidth - 1), Warning);
        Write("│\n", Warning);
        Write("├" + new string('─', PanelWidth) + "┤\n", Warning);
        DrawRow(message);
        DrawBlankRow();
        DrawRow("                        [ Enter ] to continue");
        Write("╰" + new string('─', PanelWidth) + "╯\n", Warning);
        Console.WriteLine();
        Console.ReadLine();
    }

    // ── Keyboard input ────────────────────────────────────────────────────

    public static void DrawShortcuts(params (string key, string label)[] shortcuts)
    {
        DrawSeparator();
        // Group into two columns
        var items = shortcuts.ToList();
        for (int i = 0; i < items.Count; i += 2)
        {
            Write("│  ", Accent);
            Write($"[ {items[i].key} ]", Highlight);
            Write($" {items[i].label,-20}", Muted);
            if (i + 1 < items.Count)
            {
                Write($"  [ {items[i + 1].key} ]", Highlight);
                var remaining = PanelWidth - 2 - 6 - items[i].label.Length - 6 - items[i+1].key.Length - 2;
                Write($" {items[i + 1].label}", Muted);
            }
            var used = 4 + 5 + 1 + 20 + 4 + (i + 1 < items.Count ? 20 : 0);
            Console.Write(new string(' ', Math.Max(0, PanelWidth - used)));
            Write(" │\n", Accent);
        }
    }

    public static char ReadKey(string prompt = "")
    {
        if (!string.IsNullOrWhiteSpace(prompt))
        {
            Write("│  ", Accent);
            Write(prompt, Muted);
            Console.WriteLine();
        }
        return char.ToUpper(Console.ReadKey(intercept: true).KeyChar);
    }

    public static string ReadLine(string prompt)
    {
        Write("│  ", Accent);
        Write(prompt + " > ", Highlight);
        return Console.ReadLine() ?? string.Empty;
    }

    // ── Arrow-key selection ───────────────────────────────────────────────

    /// <summary>
    /// Interactive arrow-key list picker inside a bordered section.
    /// Returns the index the user selected, or -1 if they pressed Escape.
    /// </summary>
    public static int ArrowSelect(IReadOnlyList<string> options,
        int initialIndex = 0,
        ConsoleColor selectedColor = default)
    {
        if (selectedColor == default) selectedColor = Accent;

        int selected = Math.Clamp(initialIndex, 0, options.Count - 1);
        var top      = Console.CursorTop;

        while (true)
        {
            // Render list
            Console.SetCursorPosition(0, top);
            for (int i = 0; i < options.Count; i++)
            {
                Write("│  ", Accent);
                if (i == selected)
                {
                    Write("▶ ", selectedColor);
                    Write(options[i].PadRight(PanelWidth - 6), selectedColor);
                }
                else
                {
                    Write("  ", Muted);
                    Write(options[i].PadRight(PanelWidth - 6), Muted);
                }
                Write(" │\n", Accent);
            }

            var key = Console.ReadKey(intercept: true).Key;
            switch (key)
            {
                case ConsoleKey.UpArrow:
                    selected = (selected - 1 + options.Count) % options.Count;
                    break;
                case ConsoleKey.DownArrow:
                    selected = (selected + 1) % options.Count;
                    break;
                case ConsoleKey.Enter:
                    return selected;
                case ConsoleKey.Escape:
                    return -1;
            }
        }
    }

    // ── Misc ──────────────────────────────────────────────────────────────

    public static void ClearAndReset()
    {
        Console.Clear();
        Console.ResetColor();
    }

    public static void PressEnterToContinue()
    {
        DrawBlankRow();
        DrawRow("  Press [ Enter ] to continue...");
        DrawBottomBorder();
        Console.ReadLine();
    }

    public static string FormatDuration(TimeSpan ts) =>
        ts.TotalSeconds < 60
            ? $"{(int)ts.TotalSeconds}s"
            : $"{(int)ts.TotalMinutes}m {ts.Seconds:D2}s";

    public static string FormatRemaining(int seconds) =>
        $"{seconds / 60:D2}:{seconds % 60:D2}";

    public static string SpinSpeedLabel(WashingMachine.Enums.SpinSpeed s) => $"{(int)s} RPM";
}
