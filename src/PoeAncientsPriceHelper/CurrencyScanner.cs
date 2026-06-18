using System.Drawing;
using System.Text.RegularExpressions;

namespace PoeAncientsPriceHelper;

/// <summary>
/// One-shot currency-grid scanner (F6).  Takes a calibrated region, slices it into vertical
/// columns, OCRs each column as an independent list, then resolves prices for every row.
/// Results are shown as a compact list in a separate overlay window.
/// </summary>
internal sealed class CurrencyScanner : IDisposable
{
    private readonly AppConfig _config;
    private readonly PriceRepository _prices;
    private readonly IconCache _icons;
    private readonly Action<string>? _log;

    public CurrencyScanner(AppConfig config, PriceRepository prices, IconCache icons, Action<string>? log = null)
    {
        _config = config;
        _prices = prices;
        _icons = icons;
        _log = log;
    }

    private void Log(string msg)
    {
        var line = $"[{DateTime.Now:HH:mm:ss.fff}] [Currency] {msg}";
        try { File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "currency_log.txt"), line + "\n"); } catch { }
        if (App.DebugMode) Console.WriteLine(line);
        _log?.Invoke(line);
    }

    /// <summary>
    /// Scans the configured region once, splits it into <paramref name="columnCount"/> vertical
    /// columns, runs OCR on each, merges and deduplicates the results, resolves prices, and
    /// returns the priced rows.
    /// </summary>
    public IReadOnlyList<PriceRow> ScanOnce(int columnCount = 3)
    {
        var region = _config.RegionRect;
        if (region.Width <= 0 || region.Height <= 0)
        {
            Log("ERROR Region not calibrated");
            return [];
        }

        Log($"SCAN region={region} columns={columnCount}");

        var tessdataDir = Path.Combine(AppContext.BaseDirectory, "tessdata");
        if (!Directory.Exists(tessdataDir))
        {
            Log($"ERROR tessdata not found at {tessdataDir}");
            return [];
        }

        using var scanner = new OcrScanner(tessdataDir, Log, App.DebugMode);
        var allRows = new List<OcrRow>();

        int colW = region.Width / columnCount;
        for (int i = 0; i < columnCount; i++)
        {
            int x = region.X + i * colW;
            int w = (i == columnCount - 1) ? (region.Right - x) : colW; // last col gets remainder
            var colRect = new Rectangle(x, region.Y, w, region.Height);

            try
            {
                using var bmp = ScreenCapture.CaptureRegion(colRect);
                var rows = scanner.Scan(bmp);
                Log($"  col {i}: {rows.Count} rows");
                allRows.AddRange(rows);
            }
            catch (Exception ex)
            {
                Log($"  col {i} ERROR: {ex.Message}");
            }
        }

        // Deduplicate: same normalized name within 20px vertical distance → keep best
        var deduped = Deduplicate(allRows);
        Log($"DEDUP {allRows.Count} → {deduped.Count} unique");

        var priced = BuildPriceRows(deduped);
        Log($"PRICED {priced.Count(r => r.HasPrice)}/{priced.Count}");
        return priced;
    }

    private static List<OcrRow> Deduplicate(List<OcrRow> rows)
    {
        const int yTol = 30;
        var result = new List<OcrRow>(rows.Count);
        foreach (var r in rows.OrderBy(r => r.CenterY))
        {
            bool merged = false;
            for (int i = 0; i < result.Count; i++)
            {
                var existing = result[i];
                if (Math.Abs(existing.CenterY - r.CenterY) <= yTol &&
                    existing.NormalizedName.Equals(r.NormalizedName, StringComparison.OrdinalIgnoreCase))
                {
                    // Keep the one with more letters
                    if (CountLetters(r.NormalizedName) > CountLetters(existing.NormalizedName))
                        result[i] = r;
                    merged = true;
                    break;
                }
            }
            if (!merged) result.Add(r);
        }
        return result;
    }

    private static int CountLetters(string s)
    {
        int c = 0;
        foreach (var ch in s) if (char.IsLetter(ch)) c++;
        return c;
    }

    // ── Price resolution (adapted from ScanEngine.BuildPriceRows) ──

    private IReadOnlyList<PriceRow> BuildPriceRows(IReadOnlyList<OcrRow> ocrRows)
    {
        var snapshot = _prices.Prices;
        var rows = new List<PriceRow>(ocrRows.Count);
        foreach (var row in ocrRows)
        {
            if (row.NormalizedName.Contains("runeshape"))
                continue;

            int stableY = row.CenterY;

            // Uncut gems: exact type + level only
            if (ScanEngine.TryResolveGemKey(row.NormalizedName, out var gemKey))
            {
                if (gemKey is not null && snapshot.TryGetValue(gemKey, out var gemEntry))
                    rows.Add(new PriceRow(stableY, row.RawText, gemEntry.DivineValue, gemEntry.ExaltedValue,
                        true, row.Multiplier, gemKey, true));
                else
                    rows.Add(new PriceRow(stableY, row.RawText, 0m, 0m, false, row.Multiplier, row.NormalizedName));
                continue;
            }

            // Translate German → English
            string translatedName = TranslationLoader.Translate(row.NormalizedName);
            PriceEntry? entry = null;
            string matchedKey = translatedName;
            bool exact = false;
            if (snapshot.TryGetValue(translatedName, out entry))
            {
                exact = true;
            }
            else if (row.NormalizedName.Length >= 10 &&
                     snapshot.Keys.Where(k => k.StartsWith(row.NormalizedName, StringComparison.Ordinal))
                                  .MinBy(k => k.Length) is { } prefixKey)
            {
                entry = snapshot[prefixKey];
                matchedKey = prefixKey;
            }
            else if (row.NormalizedName.Length >= 6 &&
                     ScanEngine.BestFuzzy(snapshot, row.NormalizedName) is { } fuzzy)
            {
                entry = snapshot[fuzzy];
                matchedKey = fuzzy;
            }

            if (entry != null)
                rows.Add(new PriceRow(stableY, row.RawText, entry.DivineValue, entry.ExaltedValue,
                    true, row.Multiplier, matchedKey, exact));
            else
                rows.Add(new PriceRow(stableY, row.RawText, 0m, 0m, false, row.Multiplier, row.NormalizedName));
        }
        return rows;
    }

    public void Dispose()
    {
        // nothing owned permanently
    }
}
