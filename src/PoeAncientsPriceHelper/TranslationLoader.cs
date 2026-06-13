using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace PoeAncientsPriceHelper;

/// <summary>
/// Loads a German-to-English item name translation table from translations.json.
/// The OCR engine reads German item names from the screen; this translates them
/// to English before looking up prices on poe.ninja.
/// When a German name is NOT in the map, the original text is used as-is.
/// </summary>
internal static class TranslationLoader
{
    private static IReadOnlyDictionary<string, string> _map =
        new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

    private static readonly string FileName = "translations.json";

    public static IReadOnlyDictionary<string, string> Map => _map;

    public static void Load()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, FileName);
            if (!File.Exists(path))
            {
                Console.WriteLine($"[Translations] {FileName} not found — German→English translation disabled.");
                return;
            }

            var json = File.ReadAllText(path);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            if (dict is null)
            {
                Console.Error.WriteLine($"[Translations] {FileName} is empty or invalid.");
                return;
            }

            // Normalize all keys (lowercase, strip punctuation/extra whitespace)
            var normalized = new Dictionary<string, string>(dict.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var (deKey, enValue) in dict)
            {
                var key = PriceRepository.NormalizeName(deKey);
                if (!string.IsNullOrEmpty(key))
                    normalized[key] = enValue;
            }

            _map = new ReadOnlyDictionary<string, string>(normalized);
            Console.WriteLine($"[Translations] Loaded {_map.Count} German→English translations.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Translations] Failed to load {FileName}: {ex.Message}");
        }
    }

    /// <summary>
    /// Translates a normalized German item name to the English equivalent.
    /// Returns the original name if no translation is found (fallback).
    /// </summary>
    public static string Translate(string normalizedGerman)
    {
        if (_map.TryGetValue(normalizedGerman, out var english))
            return PriceRepository.NormalizeName(english);
        // Fuzzy-match against translation keys if exact lookup fails
        var fuzzy = FuzzyTranslate(normalizedGerman);
        return fuzzy ?? normalizedGerman;
    }

    /// <summary>
    /// Tries to find a close match among the loaded translation keys using Levenshtein
    /// similarity. Returns the translated value if a match clears the threshold, null otherwise.
    /// Only considers keys whose length differs by ≤3 from the input.
    /// </summary>
    public static string? FuzzyTranslate(string normalizedGerman)
    {
        const double threshold = 0.84;
        string? best = null;
        double bestScore = threshold;
        foreach (var (deKey, enValue) in _map)
        {
            if (Math.Abs(deKey.Length - normalizedGerman.Length) > 3)
                continue;
            int dist = ScanEngine.Levenshtein(normalizedGerman, deKey);
            double score = 1.0 - (double)dist / Math.Max(normalizedGerman.Length, deKey.Length);
            if (score > bestScore) { bestScore = score; best = enValue; }
        }
        return best is not null ? PriceRepository.NormalizeName(best) : null;
    }
}
