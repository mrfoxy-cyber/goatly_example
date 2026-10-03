using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Goatly.UseCases;

public static class MarkdownUseCaseParser
{
    private static readonly Regex TitlePattern = new(
        @"^#\s+(?:(?:Test Plan|TestUse):\s*)?(UC-[A-Z]+-\d+)\b",
        RegexOptions.Compiled);

    private static readonly Regex StatusPattern = new(
        @"^\*\*Status:\*\*\s*([^\s]+)",
        RegexOptions.Compiled);

    private static readonly Regex BusinessCriterionPattern = new(
        @"^-\s+\*\*(AC-\d+):\*\*\s*(.+?)\s*$",
        RegexOptions.Compiled);

    private static readonly Regex HeadingCriterionPattern = new(
        @"^#{3,6}\s+(AC-\d+)\b\s*[-:—]?\s*(.*)$",
        RegexOptions.Compiled);

    private static readonly Regex TableCriterionPattern = new(
        @"(?:\[(AC-\d+)\]\([^)]+\)|(AC-\d+))",
        RegexOptions.Compiled);

    public static UseCaseDocument? Parse(string path, string text)
    {
        var normalizedPath = path.Replace('\\', '/');
        var kind = normalizedPath.IndexOf(
            "/docs/testing/use-cases/",
            StringComparison.OrdinalIgnoreCase) >= 0
            ? UseCaseDocumentKind.TestUse
            : UseCaseDocumentKind.UseCase;

        var normalizedText = text.Replace("\r\n", "\n").Replace('\r', '\n');
        var lines = normalizedText.Split('\n');

        string? id = null;
        var status = kind == UseCaseDocumentKind.TestUse ? "Unspecified" : "Draft";
        var titleLine = 1;
        var criteria = new List<AcceptanceCriterionDefinition>();
        var isTestUseCriteriaSection = false;

        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index];

            if (kind == UseCaseDocumentKind.TestUse && line.StartsWith("## ", StringComparison.Ordinal))
            {
                isTestUseCriteriaSection = line.Trim().Equals(
                    "## Acceptance-criteria coverage",
                    StringComparison.OrdinalIgnoreCase);
            }

            if (id is null)
            {
                var titleMatch = TitlePattern.Match(line);
                if (titleMatch.Success)
                {
                    id = titleMatch.Groups[1].Value;
                    titleLine = index + 1;
                }
            }

            var statusMatch = StatusPattern.Match(line);
            if (statusMatch.Success)
            {
                status = statusMatch.Groups[1].Value.Trim();
            }

            if (kind == UseCaseDocumentKind.UseCase)
            {
                var criterionMatch = BusinessCriterionPattern.Match(line);
                if (criterionMatch.Success)
                {
                    AddCriterion(
                        criteria,
                        criterionMatch.Groups[1].Value,
                        criterionMatch.Groups[2].Value,
                        index + 1,
                        "Required");
                    continue;
                }

                var headingMatch = HeadingCriterionPattern.Match(line);
                if (headingMatch.Success)
                {
                    var criterionText = headingMatch.Groups[2].Value;
                    if (string.IsNullOrWhiteSpace(criterionText))
                    {
                        criterionText = ContentAfterHeading(lines, index);
                    }

                    AddCriterion(
                        criteria,
                        headingMatch.Groups[1].Value,
                        criterionText,
                        index + 1,
                        "Required");
                }
            }
            else if (isTestUseCriteriaSection &&
                     line.TrimStart().StartsWith("|", StringComparison.Ordinal))
            {
                var criterionMatch = TableCriterionPattern.Match(line);
                if (!criterionMatch.Success)
                {
                    continue;
                }

                var criterionId = criterionMatch.Groups[1].Success
                    ? criterionMatch.Groups[1].Value
                    : criterionMatch.Groups[2].Value;
                var cells = line
                    .Split('|')
                    .Select(cell => cell.Trim())
                    .Where(cell => cell.Length > 0)
                    .ToArray();
                var criterionStatus = cells.Length == 0 ? "Unspecified" : cells[cells.Length - 1];

                AddCriterion(
                    criteria,
                    criterionId,
                    string.Empty,
                    index + 1,
                    criterionStatus);
            }
        }

        if (id is null)
        {
            return null;
        }

        return new UseCaseDocument(
            id,
            status,
            path,
            kind,
            FingerprintDocument(lines),
            titleLine,
            criteria);
    }

    private static void AddCriterion(
        ICollection<AcceptanceCriterionDefinition> criteria,
        string id,
        string text,
        int line,
        string status)
    {
        criteria.Add(new AcceptanceCriterionDefinition(
            id,
            text.Trim(),
            Fingerprint(text),
            line,
            status.Trim()));
    }

    private static string FingerprintDocument(IEnumerable<string> lines)
    {
        var semanticLines = lines
            .Where(line => !StatusPattern.IsMatch(line))
            .Select(Normalize)
            .Where(line => line.Length > 0);

        return Fingerprint(string.Join("\n", semanticLines));
    }

    public static string Fingerprint(string value)
    {
        var normalized = Normalize(value);
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(normalized));
        return string.Concat(bytes.Select(item => item.ToString("x2")));
    }

    private static string Normalize(string value) =>
        Regex.Replace(value.Trim(), @"\s+", " ");

    private static string ContentAfterHeading(IReadOnlyList<string> lines, int headingIndex)
    {
        var content = new List<string>();
        for (var index = headingIndex + 1; index < lines.Count; index++)
        {
            var candidate = lines[index].Trim();
            if (candidate.StartsWith("#", StringComparison.Ordinal))
            {
                break;
            }

            if (candidate.Length == 0)
            {
                continue;
            }

            content.Add(candidate);
        }

        return string.Join(" ", content);
    }
}
