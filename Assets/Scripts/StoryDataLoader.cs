using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;

public static class StoryDataLoader
{
    [Serializable]
    public class TriggerData
    {
        public string Name;
        public string Type;
        public string T;
        public string Ts1;
        public string Ts2;
        public string F;
        public string Fs1;
        public string Fs2;
        public Dictionary<string, string> other = new Dictionary<string, string>();
    }

    [Serializable]
    public class ChapterData
    {
        public string Chapter;
        public int Amount = 0;
        public List<TriggerData> triggers = new List<TriggerData>();
    }

    public static string LoadTextFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return null;

        try
        {
            if (File.Exists(fileName))
                return File.ReadAllText(fileName);
        }
        catch { }

        string nameWoExt = Path.GetFileNameWithoutExtension(fileName);
        TextAsset ta = Resources.Load<TextAsset>(nameWoExt);
        if (ta != null)
            return ta.text;

        string streamingPath = Path.Combine(Application.streamingAssetsPath, fileName);
        if (File.Exists(streamingPath))
        {
            try { return File.ReadAllText(streamingPath); }
            catch { }
        }

        Debug.LogWarning($"StoryDataLoader: не удалось найти файл '{fileName}'. " +
            $"Убедитесь, что он лежит в Resources (как TextAsset, без расширения '{nameWoExt}') " +
            $"или в StreamingAssets/{fileName}.");
        return null;
    }

    public static List<ChapterData> Parse(string text)
    {
        var chapters = new List<ChapterData>();
        if (string.IsNullOrEmpty(text)) return chapters;

        var headerRegex = new Regex(@"\[([^\]]+)\]", RegexOptions.Multiline);
        var headerMatches = headerRegex.Matches(text);

        if (headerMatches.Count == 0)
        {
            var single = new ChapterData { Chapter = "Default", Amount = 0 };
            single.triggers = ParseTriggersInBlock(text);
            chapters.Add(single);
            return chapters;
        }

        for (int i = 0; i < headerMatches.Count; i++)
        {
            var match = headerMatches[i];
            int startIndex = match.Index + match.Length;
            int endIndex = (i + 1 < headerMatches.Count) ? headerMatches[i + 1].Index : text.Length;
            string headerContent = match.Groups[1].Value;
            string body = text.Substring(startIndex, endIndex - startIndex);

            var ch = new ChapterData();
            var kvRegex = new Regex(@"(\w+)\s*=\s*(""([^""]*)""|([-+]?\d+))");
            var kvMatches = kvRegex.Matches(headerContent);
            foreach (Match kv in kvMatches)
            {
                string key = kv.Groups[1].Value;
                if (kv.Groups[3].Success)
                {
                    string val = kv.Groups[3].Value;
                    if (key.Equals("Chapter", StringComparison.OrdinalIgnoreCase) || key.Equals("chapter", StringComparison.OrdinalIgnoreCase))
                        ch.Chapter = val;
                    else
                        ch.otherSet(key, val);
                }
                else if (kv.Groups[4].Success)
                {
                    int iv = 0;
                    int.TryParse(kv.Groups[4].Value, out iv);
                    if (key.Equals("Amount", StringComparison.OrdinalIgnoreCase))
                        ch.Amount = iv;
                    else
                        ch.otherSet(key, kv.Groups[4].Value);
                }
            }

            if (string.IsNullOrEmpty(ch.Chapter))
                ch.Chapter = $"Chapter {chapters.Count + 1}";

            ch.triggers = ParseTriggersInBlock(body);
            chapters.Add(ch);
        }

        return chapters;
    }

    private static void otherSet(this ChapterData ch, string key, string val)
    {
        // на будущее
    }

    private static List<TriggerData> ParseTriggersInBlock(string block)
    {
        var list = new List<TriggerData>();
        if (string.IsNullOrEmpty(block)) return list;

        var triggerRegex = new Regex(@"\{([^}]*)\}", RegexOptions.Singleline);
        var trigMatches = triggerRegex.Matches(block);
        foreach (Match tm in trigMatches)
        {
            string content = tm.Groups[1].Value;
            var t = new TriggerData();

            var kvRegex = new Regex(@"(\w+)\s*=\s*(""([^""]*)""|([-+]?\d+))", RegexOptions.Multiline);
            var kvMatches = kvRegex.Matches(content);
            foreach (Match kv in kvMatches)
            {
                string key = kv.Groups[1].Value;
                string val = kv.Groups[3].Success ? kv.Groups[3].Value : kv.Groups[4].Value;

                switch (key.ToLowerInvariant())
                {
                    case "name": t.Name = val; break;
                    case "type": t.Type = val; break;
                    case "t": t.T = val; break;
                    case "ts1": t.Ts1 = val; break;
                    case "ts2": t.Ts2 = val; break;
                    case "f": t.F = val; break;
                    case "fs1": t.Fs1 = val; break;
                    case "fs2": t.Fs2 = val; break;
                    default:
                        if (!string.IsNullOrEmpty(key))
                            t.other[key] = val;
                        break;
                }
            }

            if (string.IsNullOrEmpty(t.Name))
                t.Name = $"trigger_{list.Count + 1}";

            list.Add(t);
        }

        return list;
    }
}
