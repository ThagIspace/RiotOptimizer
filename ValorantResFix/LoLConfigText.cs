using IniParser.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ValorantResFix
{
    internal static class LoLConfigText
    {
        private static readonly Regex Lines = new Regex(@"[^\r\n]+(?:\r\n|\r|\n)?|\r\n|\r|\n");
        private static readonly Regex NewLine = new Regex(@"\r\n|\r|\n");
        private static readonly Regex Assignment = new Regex(
            @"\A(?<prefix>[^\S\r\n]*(?<key>[\w.]+)[^\S\r\n]*=[^\S\r\n]*)(?<value>[^;\r\n]*?)(?<suffix>[^\S\r\n]*(?:;[^\r\n]*)?(?:\r\n|\r|\n)?)\z");

        internal static void Save(string path, string backup, IniDataParser parser,
            IDictionary<string, IDictionary<string, string>> sections)
        {
            byte[] original = File.ReadAllBytes(path);
            string text;
            Encoding encoding;
            using (var reader = new StreamReader(new MemoryStream(original), Encoding.Default, true))
            {
                text = reader.ReadToEnd();
                encoding = reader.CurrentEncoding;
            }

            byte[] preamble = encoding.GetPreamble();
            int preambleLength = original.Take(preamble.Length).SequenceEqual(preamble) ? preamble.Length : 0;
            if (!original.Skip(preambleLength).SequenceEqual(encoding.GetBytes(text)))
                throw new IOException(Loc.T("Không thể giữ nguyên mã hóa của game.cfg.",
                                           "The original game.cfg encoding could not be preserved."));

            parser.Parse(text);
            var lines = Lines.Matches(text).Cast<Match>().Select(match => match.Value).ToList();
            var firstBreak = NewLine.Match(text);
            string newLine = firstBreak.Success ? firstBreak.Value : Environment.NewLine;
            foreach (var section in sections)
                UpdateSection(lines, section.Key, section.Value, parser.Configuration.SectionRegex, newLine);

            byte[] updated = encoding.GetBytes(string.Concat(lines));
            ConfigFiles.Save(path, backup, temp =>
            {
                using (var output = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    output.Write(original, 0, preambleLength);
                    output.Write(updated, 0, updated.Length);
                }
            });
        }

        private static void UpdateSection(List<string> lines, string name,
            IDictionary<string, string> settings, Regex sectionPattern, string newLine)
        {
            int start = -1;
            int end = lines.Count;
            for (int i = 0; i < lines.Count; i++)
            {
                string header = lines[i].Trim();
                if (!sectionPattern.IsMatch(header)) continue;
                if (start >= 0)
                {
                    end = i;
                    break;
                }
                if (string.Equals(header.Substring(1, header.Length - 2).Trim(), name, StringComparison.Ordinal))
                    start = i + 1;
            }

            var remaining = new HashSet<string>(settings.Keys, StringComparer.Ordinal);
            if (start >= 0)
            {
                // The parser uses the last value of a duplicated key.
                for (int i = end - 1; i >= start; i--)
                {
                    var match = Assignment.Match(lines[i]);
                    if (!match.Success) continue;
                    string key = match.Groups["key"].Value;
                    if (!remaining.Remove(key)) continue;
                    lines[i] = match.Groups["prefix"].Value + settings[key] + match.Groups["suffix"].Value;
                }
            }
            if (remaining.Count == 0) return;

            bool append = end == lines.Count;
            bool hadFinalNewLine = lines.Count > 0 && NewLine.IsMatch(lines[lines.Count - 1]);
            if (append && lines.Count > 0 && !hadFinalNewLine)
                lines[lines.Count - 1] += newLine;

            var added = new List<string>();
            if (start < 0) added.Add("[" + name + "]" + newLine);
            added.AddRange(settings.Where(pair => remaining.Contains(pair.Key))
                .Select(pair => pair.Key + "=" + pair.Value + newLine));
            if (append && !hadFinalNewLine)
                added[added.Count - 1] = added[added.Count - 1].Substring(0, added[added.Count - 1].Length - newLine.Length);
            lines.InsertRange(end, added);
        }
    }
}
