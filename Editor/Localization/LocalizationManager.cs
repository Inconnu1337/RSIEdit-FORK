using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;

namespace Editor.Localization;
// WIP. SHIT.
public static class LocalizationManager
{
    public const string DefaultLocale = "en-US";

    private const string SentinelKey = "app-locale";

    public static void LoadLanguage(string locale)
    {
        var app = Application.Current;
        if (app is null) return;

        var uri = new Uri($"avares://Editor/Assets/Localization/{locale}.ftl");
        string content;
        using (var stream = AssetLoader.Open(uri))
        using (var reader = new StreamReader(stream))
            content = reader.ReadToEnd();

        var strings = ParseFtl(content);

        var dict = new ResourceDictionary();
        foreach (var (key, value) in strings)
            dict[key] = value;

        var merged = app.Resources.MergedDictionaries;
        for (var i = merged.Count - 1; i >= 0; i--)
        {
            if (merged[i].TryGetResource(SentinelKey, null, out _))
            {
                merged.RemoveAt(i);
                break;
            }
        }

        merged.Add(dict);
    }

    private static Dictionary<string, string> ParseFtl(string content)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var rawLine in content.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');

            if (line.Length == 0 || line[0] == '#')
                continue;

            if (line[0] == ' ' || line[0] == '\t')
                continue;

            var eq = line.IndexOf('=');
            if (eq <= 0)
                continue;

            var key = line[..eq].TrimEnd();
            var value = line[(eq + 1)..].TrimStart();

            if (key.Length > 0)
                result[key] = value;
        }

        return result;
    }
}
