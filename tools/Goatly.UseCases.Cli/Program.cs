using System.Text;
using System.Text.RegularExpressions;
using Goatly.UseCases;

return GoatlyCli.Run(args);

internal static class GoatlyCli
{
    private static readonly Regex TypedCoverPattern = new(
        @"Covers\s*\(\s*typeof\s*\(\s*Criteria\.(UC_[A-Z]+_\d+)\.(AC_\d+)\s*\)\s*\)",
        RegexOptions.Compiled | RegexOptions.Multiline);

    public static int Run(string[] args)
    {
        var root = FindRoot(Directory.GetCurrentDirectory());
        if (root is null)
        {
            Console.Error.WriteLine("Could not find a Goatly.Backend directory containing docs/use-cases.");
            return 2;
        }

        var state = Load(root);
        var command = args.Length == 0 ? "help" : args[0].ToLowerInvariant();

        return command switch
        {
            "help" or "--help" or "-h" => PrintHelp(),
            "check" => Check(state),
            "missing" => PrintMissing(state),
            "changed" => PrintChanged(state),
            "report" => PrintReport(state),
            "usecase" when args.Length == 2 => PrintUseCase(state, args[1]),
            "criterion" when args.Length == 2 => PrintCriterion(state, args[1]),
            "scaffold" when args.Length == 2 => ScaffoldTestUse(state, args[1]),
            "snapshot" => PrintSnapshot(state),
            "acknowledge" when args.Length == 2 => Acknowledge(state, args[1]),
            _ => UnknownCommand(args)
        };
    }

    private static WorkspaceState Load(string root)
    {
        var documents = Directory
            .EnumerateFiles(Path.Combine(root, "docs"), "*.md", SearchOption.AllDirectories)
            .Where(path => IsUseCaseDocument(path))
            .Select(path => MarkdownUseCaseParser.Parse(path, File.ReadAllText(path)))
            .Where(document => document is not null)
            .Cast<UseCaseDocument>()
            .ToArray();
        var catalog = UseCaseCatalog.Create(documents);
        var lockPath = Path.Combine(root, "docs", "testing", "criteria.lock");
        var criteriaLock = CriteriaLock.Parse(
            File.Exists(lockPath) ? File.ReadAllText(lockPath) : null);
        var testReferences = FindTestReferences(root);

        return new WorkspaceState(root, lockPath, catalog, criteriaLock, testReferences);
    }

    private static int Check(WorkspaceState state)
    {
        PrintIssues(state.Catalog.Issues);
        var changed = ChangedDefinitions(state).ToArray();
        var missing = MissingCriteria(state).ToArray();

        foreach (var item in changed)
        {
            Console.WriteLine($"[CHANGED] {item.Key}");
            Console.WriteLine($"  {Clickable(item.Path, item.Line)}");
        }

        foreach (var item in missing)
        {
            Console.WriteLine($"[UNMAPPED] {item.UseCase.Id}/{item.Criterion.Id}");
            Console.WriteLine($"  {Clickable(item.UseCase.Path, item.Criterion.Line)}");
        }

        var failed = !state.Catalog.IsValid || changed.Length > 0 || missing.Length > 0;
        Console.WriteLine(failed
            ? "Goatly use-case check found work requiring attention."
            : "Goatly use-case check passed.");
        return failed ? 1 : 0;
    }

    private static int PrintUseCase(WorkspaceState state, string id)
    {
        var pair = state.Catalog.Pairs.FirstOrDefault(pair =>
            string.Equals(pair.UseCase.Id, id, StringComparison.OrdinalIgnoreCase));
        if (pair is null)
        {
            Console.Error.WriteLine($"Unknown use case: {id}");
            return 2;
        }

        WriteSectionHeader("Use case");
        WriteField("ID", pair.UseCase.Id);
        WriteField("Title", ReadHeading(pair.UseCase.Path, pair.UseCase.TitleLine));
        WriteField("Status", pair.UseCase.Status);
        WriteField(
            "Evidence",
            $"UseCase document — {Clickable(pair.UseCase.Path, pair.UseCase.TitleLine)}");
        Console.WriteLine();

        WriteSectionHeader("TestUse");
        if (pair.TestUse is null)
        {
            WriteField("Status", "Missing");
        }
        else
        {
            WriteField("Title", ReadHeading(pair.TestUse.Path, pair.TestUse.TitleLine));
            WriteField("Status", pair.TestUse.Status);
            WriteField(
                "Evidence",
                $"TestUse document — {Clickable(pair.TestUse.Path, pair.TestUse.TitleLine)}");
        }

        Console.WriteLine();
        WriteSectionHeader("Acceptance criteria");
        foreach (var criterion in pair.UseCase.Criteria)
        {
            var references = ReferencesFor(state, pair.UseCase.Id, criterion.Id).ToArray();
            var status = CriterionDisplayStatus(state, pair, criterion, references.Length);
            Console.WriteLine($"  {criterion.Id}");
            WriteField("Status", status, 4);
            WriteField("Text", criterion.Text, 4);
            WriteField(
                "Evidence",
                $"UseCase criterion — {Clickable(pair.UseCase.Path, criterion.Line)}",
                4);
        }

        return 0;
    }

    private static int PrintCriterion(WorkspaceState state, string selector)
    {
        if (!TrySplitSelector(selector, out var useCaseId, out var criterionId))
        {
            Console.Error.WriteLine("Use UC-CON-001/AC-002 format.");
            return 2;
        }

        var pair = state.Catalog.Pairs.FirstOrDefault(pair =>
            string.Equals(pair.UseCase.Id, useCaseId, StringComparison.OrdinalIgnoreCase));
        var criterion = pair?.UseCase.Criteria.FirstOrDefault(item =>
            string.Equals(item.Id, criterionId, StringComparison.OrdinalIgnoreCase));
        if (pair is null || criterion is null)
        {
            Console.Error.WriteLine($"Unknown criterion: {selector}");
            return 2;
        }

        var key = $"{pair.UseCase.Id}/{criterion.Id}";
        WriteSectionHeader("Criterion");
        WriteField("Use case", pair.UseCase.Id);
        WriteField("ID", criterion.Id);
        WriteField("Text", criterion.Text);
        WriteField(
            "Revision",
            state.CriteriaLock.IsChanged(key, criterion.Fingerprint)
                ? "CHANGED — mapped tests require review"
                : "Acknowledged");
        WriteField(
            "Evidence",
            $"UseCase criterion — {Clickable(pair.UseCase.Path, criterion.Line)}");

        var planned = pair.TestUse?.Criteria.FirstOrDefault(item => item.Id == criterion.Id);
        Console.WriteLine();
        WriteSectionHeader("TestUse");
        if (planned is null)
        {
            WriteField("Status", "Missing criterion");
        }
        else
        {
            foreach (var field in ReadMarkdownTableFields(pair.TestUse!.Path, planned.Line))
            {
                WriteField(field.Key, field.Value);
            }

            WriteField(
                "Evidence",
                $"TestUse coverage row — {Clickable(pair.TestUse.Path, planned.Line)}");
        }

        var references = ReferencesFor(state, pair.UseCase.Id, criterion.Id).ToArray();
        Console.WriteLine();
        WriteSectionHeader($"Tests ({references.Length})");
        for (var index = 0; index < references.Length; index++)
        {
            var reference = references[index];
            Console.WriteLine($"  {index + 1}. {reference.TestName}");
            WriteField("Covers", ReadLine(reference.Path, reference.Line), 5);
            WriteField(
                "Evidence",
                $"Executable Covers metadata — {Clickable(reference.Path, reference.Line)}",
                5);
        }
        return 0;
    }

    private static int PrintMissing(WorkspaceState state)
    {
        var missing = MissingCriteria(state).ToArray();
        foreach (var item in missing)
        {
            Console.WriteLine($"[UNMAPPED] {item.UseCase.Id}/{item.Criterion.Id} — {item.Criterion.Text}");
            Console.WriteLine($"  {Clickable(item.UseCase.Path, item.Criterion.Line)}");
        }

        Console.WriteLine($"Unmapped required criteria: {missing.Length}");
        return missing.Length == 0 ? 0 : 1;
    }

    private static int PrintChanged(WorkspaceState state)
    {
        var changed = ChangedDefinitions(state).ToArray();
        foreach (var item in changed)
        {
            Console.WriteLine($"[CHANGED] {item.Key}");
            Console.WriteLine($"  Definition: {Clickable(item.Path, item.Line)}");
            foreach (var reference in item.CriterionId is null
                         ? state.TestReferences.Where(reference => reference.UseCaseId == item.UseCaseId)
                         : ReferencesFor(state, item.UseCaseId, item.CriterionId))
            {
                Console.WriteLine($"  Test: {Clickable(reference.Path, reference.Line)} {reference.TestName}");
            }
        }

        Console.WriteLine($"Changed definitions awaiting review: {changed.Length}");
        return changed.Length == 0 ? 0 : 1;
    }

    private static int PrintReport(WorkspaceState state)
    {
        PrintIssues(state.Catalog.Issues);
        foreach (var pair in state.Catalog.Pairs)
        {
            PrintUseCase(state, pair.UseCase.Id);
            Console.WriteLine();
        }

        return state.Catalog.IsValid ? 0 : 1;
    }

    private static int PrintSnapshot(WorkspaceState state)
    {
        Console.Write(CriteriaLock.CreateSnapshot(state.Catalog.Pairs));
        return state.Catalog.IsValid ? 0 : 1;
    }

    private static int ScaffoldTestUse(WorkspaceState state, string id)
    {
        var pair = state.Catalog.Pairs.FirstOrDefault(item =>
            string.Equals(item.UseCase.Id, id, StringComparison.OrdinalIgnoreCase));
        if (pair is null)
        {
            Console.Error.WriteLine($"Unknown use case: {id}");
            Console.Error.WriteLine("Create the business UseCase document first, then scaffold its TestUse.");
            return 2;
        }

        if (pair.TestUse is not null)
        {
            Console.Error.WriteLine($"TestUse already exists: {pair.TestUse.Path}");
            return 1;
        }

        if (pair.UseCase.Criteria.Count == 0)
        {
            Console.Error.WriteLine($"{pair.UseCase.Id} has no numbered acceptance criteria.");
            Console.Error.WriteLine("Add headings such as '### AC-001' before scaffolding its TestUse.");
            return 1;
        }

        var useCaseRoot = Path.Combine(state.Root, "docs", "use-cases");
        var testUseRoot = Path.Combine(state.Root, "docs", "testing", "use-cases");
        var relativeUseCasePath = Path.GetRelativePath(useCaseRoot, pair.UseCase.Path);
        var targetPath = Path.Combine(testUseRoot, relativeUseCasePath);
        if (File.Exists(targetPath))
        {
            Console.Error.WriteLine($"Refusing to overwrite: {targetPath}");
            return 1;
        }

        var targetDirectory = Path.GetDirectoryName(targetPath)!;
        var useCaseLink = Path
            .GetRelativePath(targetDirectory, pair.UseCase.Path)
            .Replace('\\', '/');
        var heading = ReadHeading(pair.UseCase.Path, pair.UseCase.TitleLine);
        var title = Regex.Replace(
            heading,
            $"^{Regex.Escape(pair.UseCase.Id)}\\s*:?\\s*",
            string.Empty,
            RegexOptions.IgnoreCase);
        var markdown = BuildTestUseScaffold(pair.UseCase, title, useCaseLink);

        Directory.CreateDirectory(targetDirectory);
        File.WriteAllText(targetPath, markdown, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        WriteSectionHeader("TestUse scaffolded");
        WriteField("Use case", pair.UseCase.Id);
        WriteField("Criteria", pair.UseCase.Criteria.Count.ToString());
        WriteField("Created", targetPath);
        WriteField(
            "Next",
            $"Fill the TODO fields, then run .\\goatly criterion {pair.UseCase.Id}/AC-001");
        WriteField(
            "Evidence",
            $"Generated from UseCase document — {Clickable(pair.UseCase.Path, pair.UseCase.TitleLine)}");
        return 0;
    }

    private static string BuildTestUseScaffold(
        UseCaseDocument useCase,
        string title,
        string useCaseLink)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# Test Plan: {useCase.Id} {title}".TrimEnd());
        builder.AppendLine();
        builder.AppendLine("**Status:** Draft  ");
        builder.AppendLine($"**Use case:** [{useCase.Id}: {title}]({useCaseLink})");
        builder.AppendLine();
        builder.AppendLine("## Purpose");
        builder.AppendLine();
        builder.AppendLine("TODO: State what evidence this TestUse must provide.");
        builder.AppendLine();
        builder.AppendLine("## Acceptance-criteria coverage");
        builder.AppendLine();
        builder.AppendLine("| Claim | What is selected | Type | Technique | Why the technique fits | Status |");
        builder.AppendLine("|---|---|---|---|---|---|");

        foreach (var criterion in useCase.Criteria.OrderBy(item => item.Id, StringComparer.Ordinal))
        {
            var anchor = criterion.Id.ToLowerInvariant();
            builder.AppendLine(
                $"| [{criterion.Id}]({useCaseLink}#{anchor}) | TODO | TODO | TODO | TODO | Planned |");
        }

        builder.AppendLine();
        builder.AppendLine("## Notes");
        builder.AppendLine();
        builder.AppendLine("- TODO: Add decision tables or other evidence design only when useful.");
        return builder.ToString();
    }

    private static int Acknowledge(WorkspaceState state, string selector)
    {
        if (!state.Catalog.IsValid)
        {
            Console.Error.WriteLine("Cannot acknowledge revisions while document validation has errors.");
            PrintIssues(state.Catalog.Issues);
            return 1;
        }

        var entries = new Dictionary<string, string>(state.CriteriaLock.Entries, StringComparer.Ordinal);
        if (string.Equals(selector, "all", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var pair in state.Catalog.Pairs)
            {
                AddCurrent(entries, pair.UseCase);
            }
        }
        else if (TrySplitSelector(selector, out var useCaseId, out var criterionId))
        {
            var pair = state.Catalog.Pairs.FirstOrDefault(item => item.UseCase.Id == useCaseId);
            var criterion = pair?.UseCase.Criteria.FirstOrDefault(item => item.Id == criterionId);
            if (criterion is null)
            {
                Console.Error.WriteLine($"Unknown criterion: {selector}");
                return 2;
            }

            entries[selector] = criterion.Fingerprint;
        }
        else
        {
            var pair = state.Catalog.Pairs.FirstOrDefault(item => item.UseCase.Id == selector);
            if (pair is null)
            {
                Console.Error.WriteLine($"Unknown use case or criterion: {selector}");
                return 2;
            }

            AddCurrent(entries, pair.UseCase);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(state.LockPath)!);
        File.WriteAllText(state.LockPath, SerializeLock(entries));
        Console.WriteLine($"Acknowledged {selector}. Updated {state.LockPath}");
        return 0;
    }

    private static void AddCurrent(IDictionary<string, string> entries, UseCaseDocument useCase)
    {
        entries[useCase.Id] = useCase.Fingerprint;
        foreach (var criterion in useCase.Criteria)
        {
            entries[$"{useCase.Id}/{criterion.Id}"] = criterion.Fingerprint;
        }
    }

    private static string SerializeLock(IEnumerable<KeyValuePair<string, string>> entries)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Generated by Goatly use-case tooling. Review changes before updating.");
        builder.AppendLine("# Format: <use-case-or-criterion-key> <semantic-sha256>");
        builder.AppendLine();
        foreach (var entry in entries.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            builder.AppendLine($"{entry.Key} {entry.Value}");
        }

        return builder.ToString();
    }

    private static IEnumerable<MissingCriterion> MissingCriteria(WorkspaceState state)
    {
        foreach (var pair in state.Catalog.Pairs.Where(pair => pair.TestUse is not null))
        {
            foreach (var criterion in pair.UseCase.Criteria)
            {
                var planned = pair.TestUse!.Criteria.FirstOrDefault(item => item.Id == criterion.Id);
                if (planned is not null && IsNonAutomated(planned.Status))
                {
                    continue;
                }

                if (!ReferencesFor(state, pair.UseCase.Id, criterion.Id).Any())
                {
                    yield return new MissingCriterion(pair.UseCase, criterion);
                }
            }
        }
    }

    private static IEnumerable<ChangedDefinition> ChangedDefinitions(WorkspaceState state)
    {
        foreach (var pair in state.Catalog.Pairs.Where(pair => pair.TestUse is not null))
        {
            if (state.CriteriaLock.IsChanged(pair.UseCase.Id, pair.UseCase.Fingerprint))
            {
                yield return new ChangedDefinition(
                    pair.UseCase.Id,
                    pair.UseCase.Id,
                    null,
                    pair.UseCase.Path,
                    pair.UseCase.TitleLine);
            }

            foreach (var criterion in pair.UseCase.Criteria)
            {
                var key = $"{pair.UseCase.Id}/{criterion.Id}";
                if (state.CriteriaLock.IsChanged(key, criterion.Fingerprint))
                {
                    yield return new ChangedDefinition(
                        key,
                        pair.UseCase.Id,
                        criterion.Id,
                        pair.UseCase.Path,
                        criterion.Line);
                }
            }
        }
    }

    private static IEnumerable<TestReference> ReferencesFor(
        WorkspaceState state,
        string useCaseId,
        string criterionId) => state.TestReferences.Where(reference =>
            reference.UseCaseId == useCaseId && reference.CriterionId == criterionId);

    private static IReadOnlyList<TestReference> FindTestReferences(string root)
    {
        var references = new List<TestReference>();
        var testRoot = Path.Combine(root, "tests");
        if (!Directory.Exists(testRoot))
        {
            return references;
        }

        foreach (var path in Directory.EnumerateFiles(testRoot, "*.cs", SearchOption.AllDirectories)
                     .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                                    !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")))
        {
            var text = File.ReadAllText(path);
            foreach (Match match in TypedCoverPattern.Matches(text))
            {
                var useCaseId = match.Groups[1].Value.Replace('_', '-');
                var criterionId = match.Groups[2].Value.Replace('_', '-');
                var line = 1 + text.Take(match.Index).Count(character => character == '\n');
                var following = text.Substring(match.Index, Math.Min(1200, text.Length - match.Index));
                var method = Regex.Match(
                    following,
                    @"public\s+(?:async\s+)?(?:void|Task(?:<[^>]+>)?)\s+([A-Za-z_][A-Za-z0-9_]*)\s*\(");
                references.Add(new TestReference(
                    useCaseId,
                    criterionId,
                    path,
                    line,
                    method.Success ? method.Groups[1].Value : "test"));
            }
        }

        return references;
    }

    private static bool IsNonAutomated(string status) =>
        status.IndexOf("Deferred", StringComparison.OrdinalIgnoreCase) >= 0 ||
        status.IndexOf("Manual", StringComparison.OrdinalIgnoreCase) >= 0;

    private static string CriterionDisplayStatus(
        WorkspaceState state,
        UseCasePair pair,
        AcceptanceCriterionDefinition criterion,
        int referenceCount)
    {
        var key = $"{pair.UseCase.Id}/{criterion.Id}";
        if (state.CriteriaLock.IsChanged(key, criterion.Fingerprint))
        {
            return "CHANGED";
        }

        var planned = pair.TestUse?.Criteria.FirstOrDefault(item => item.Id == criterion.Id);
        if (planned is not null && IsNonAutomated(planned.Status))
        {
            return planned.Status.IndexOf("Deferred", StringComparison.OrdinalIgnoreCase) >= 0
                ? "DEFERRED"
                : "MANUAL";
        }

        return referenceCount == 0 ? "UNMAPPED" : "MAPPED";
    }

    private static void PrintIssues(IEnumerable<CatalogIssue> issues)
    {
        foreach (var issue in issues)
        {
            Console.WriteLine($"{Clickable(issue.Path, issue.Line)}: {issue.Code}: {issue.Message}");
        }
    }

    private static string Clickable(string path, int line) =>
        $"{Path.GetFullPath(path)}({line},1)";

    private static string ReferenceWithPreview(
        string path,
        int line,
        string? semanticText = null)
    {
        var sourceText = ReadLine(path, line);
        var preview = sourceText.Length == 0 ? semanticText ?? string.Empty : sourceText;
        if (!string.IsNullOrWhiteSpace(semanticText) &&
            preview.IndexOf(semanticText, StringComparison.Ordinal) < 0)
        {
            preview = $"{preview} — {semanticText}".TrimStart(' ', '—');
        }

        return preview.Length == 0
            ? Clickable(path, line)
            : $"{Clickable(path, line)} — {preview}";
    }

    private static string ReadLine(string path, int oneBasedLine)
    {
        if (!File.Exists(path) || oneBasedLine < 1)
        {
            return string.Empty;
        }

        return File.ReadLines(path)
            .Skip(oneBasedLine - 1)
            .FirstOrDefault()?
            .Trim() ?? string.Empty;
    }

    private static string ReadHeading(string path, int oneBasedLine) =>
        ReadLine(path, oneBasedLine).TrimStart('#', ' ');

    private static IReadOnlyList<KeyValuePair<string, string>> ReadMarkdownTableFields(
        string path,
        int oneBasedLine)
    {
        if (!File.Exists(path) || oneBasedLine < 1)
        {
            return [];
        }

        var lines = File.ReadAllLines(path);
        var rowIndex = oneBasedLine - 1;
        if (rowIndex >= lines.Length || !lines[rowIndex].TrimStart().StartsWith("|"))
        {
            return [];
        }

        var tableStart = rowIndex;
        while (tableStart > 0 &&
               lines[tableStart - 1].TrimStart().StartsWith("|"))
        {
            tableStart--;
        }

        var headers = SplitMarkdownTableRow(lines[tableStart]);
        var values = SplitMarkdownTableRow(lines[rowIndex]);
        return headers
            .Take(Math.Min(headers.Count, values.Count))
            .Select((header, index) => new KeyValuePair<string, string>(
                header,
                CleanMarkdownCell(values[index])))
            .ToArray();
    }

    private static IReadOnlyList<string> SplitMarkdownTableRow(string row) =>
        row.Trim()
            .Trim('|')
            .Split('|')
            .Select(cell => cell.Trim())
            .ToArray();

    private static string CleanMarkdownCell(string value) =>
        Regex.Replace(value, @"\[([^\]]+)\]\([^)]+\)", "$1");

    private static void WriteField(string label, string value, int indent = 2)
    {
        var spaces = new string(' ', indent);
        Console.WriteLine($"{spaces}{label,-23}: {value}");
    }

    private static void WriteSectionHeader(string title)
    {
        Console.WriteLine($"── {title} ──");
    }

    private static bool TrySplitSelector(string selector, out string useCaseId, out string criterionId)
    {
        var parts = selector.ToUpperInvariant().Split('/');
        useCaseId = parts.Length > 0 ? parts[0] : string.Empty;
        criterionId = parts.Length > 1 ? parts[1] : string.Empty;
        return parts.Length == 2 && useCaseId.StartsWith("UC-") && criterionId.StartsWith("AC-");
    }

    private static int PrintHelp()
    {
        Console.WriteLine(
            """
            Goatly use-case tooling

              goatly usecase UC-CON-001
              goatly criterion UC-CON-001/AC-002
              goatly scaffold UC-CON-001
              goatly missing
              goatly changed
              goatly check
              goatly report
              goatly acknowledge UC-CON-001/AC-002
              goatly acknowledge UC-CON-001
              goatly acknowledge all
              goatly snapshot
            """);
        return 0;
    }

    private static int UnknownCommand(string[] args)
    {
        Console.Error.WriteLine($"Unknown command: {string.Join(" ", args)}");
        PrintHelp();
        return 2;
    }

    private static bool IsUseCaseDocument(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.IndexOf("/docs/use-cases/", StringComparison.OrdinalIgnoreCase) >= 0 ||
               normalized.IndexOf("/docs/testing/use-cases/", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static string? FindRoot(string startingPath)
    {
        var directory = new DirectoryInfo(startingPath);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "docs", "use-cases")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private sealed record WorkspaceState(
        string Root,
        string LockPath,
        UseCaseCatalog Catalog,
        CriteriaLock CriteriaLock,
        IReadOnlyList<TestReference> TestReferences);

    private sealed record TestReference(
        string UseCaseId,
        string CriterionId,
        string Path,
        int Line,
        string TestName);

    private sealed record MissingCriterion(
        UseCaseDocument UseCase,
        AcceptanceCriterionDefinition Criterion);

    private sealed record ChangedDefinition(
        string Key,
        string UseCaseId,
        string? CriterionId,
        string Path,
        int Line);
}
