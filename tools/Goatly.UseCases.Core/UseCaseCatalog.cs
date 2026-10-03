namespace Goatly.UseCases;

public enum CatalogIssueSeverity
{
    Information = 1,
    Warning = 2,
    Error = 3
}

public sealed class CatalogIssue
{
    public CatalogIssue(
        string code,
        string message,
        CatalogIssueSeverity severity,
        string path,
        int line)
    {
        Code = code;
        Message = message;
        Severity = severity;
        Path = path;
        Line = line;
    }

    public string Code { get; }
    public string Message { get; }
    public CatalogIssueSeverity Severity { get; }
    public string Path { get; }
    public int Line { get; }
}

public sealed class UseCasePair
{
    public UseCasePair(UseCaseDocument useCase, UseCaseDocument? testUse)
    {
        UseCase = useCase;
        TestUse = testUse;
    }

    public UseCaseDocument UseCase { get; }
    public UseCaseDocument? TestUse { get; }
}

public sealed class UseCaseCatalog
{
    private UseCaseCatalog(
        IReadOnlyList<UseCasePair> pairs,
        IReadOnlyList<CatalogIssue> issues)
    {
        Pairs = pairs;
        Issues = issues;
    }

    public IReadOnlyList<UseCasePair> Pairs { get; }
    public IReadOnlyList<CatalogIssue> Issues { get; }
    public bool IsValid => Issues.All(issue => issue.Severity != CatalogIssueSeverity.Error);

    public static UseCaseCatalog Create(IEnumerable<UseCaseDocument> documents)
    {
        var materialized = documents.ToArray();
        var issues = new List<CatalogIssue>();

        AddDuplicateDocumentIssues(materialized, issues);
        AddDuplicateCriterionIssues(materialized, issues);

        var useCases = materialized
            .Where(document => document.Kind == UseCaseDocumentKind.UseCase)
            .GroupBy(document => document.Id, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var testUses = materialized
            .Where(document => document.Kind == UseCaseDocumentKind.TestUse)
            .GroupBy(document => document.Id, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        var pairs = new List<UseCasePair>();
        foreach (var useCase in useCases.Values.OrderBy(document => document.Id, StringComparer.Ordinal))
        {
            testUses.TryGetValue(useCase.Id, out var testUse);
            pairs.Add(new UseCasePair(useCase, testUse));

            if (testUse is null)
            {
                if (!string.Equals(useCase.Status, "Draft", StringComparison.OrdinalIgnoreCase))
                {
                    issues.Add(new CatalogIssue(
                        "GTLY002",
                        $"{useCase.Id} is {useCase.Status} and requires a matching TestUse document.",
                        CatalogIssueSeverity.Error,
                        useCase.Path,
                        useCase.TitleLine));
                }

                continue;
            }

            ValidateCriterionParity(useCase, testUse, issues);
        }

        foreach (var testUse in testUses.Values.Where(document => !useCases.ContainsKey(document.Id)))
        {
            issues.Add(new CatalogIssue(
                "GTLY001",
                $"TestUse {testUse.Id} has no matching business UseCase document.",
                CatalogIssueSeverity.Error,
                testUse.Path,
                testUse.TitleLine));
        }

        return new UseCaseCatalog(pairs, issues);
    }

    private static void ValidateCriterionParity(
        UseCaseDocument useCase,
        UseCaseDocument testUse,
        ICollection<CatalogIssue> issues)
    {
        var businessCriteria = useCase.Criteria
            .GroupBy(criterion => criterion.Id, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var plannedCriteria = testUse.Criteria
            .GroupBy(criterion => criterion.Id, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        foreach (var criterion in businessCriteria.Values.Where(item => !plannedCriteria.ContainsKey(item.Id)))
        {
            issues.Add(new CatalogIssue(
                "GTLY005",
                $"{useCase.Id}/{criterion.Id} is missing from its TestUse document.",
                CatalogIssueSeverity.Error,
                useCase.Path,
                criterion.Line));
        }

        foreach (var criterion in plannedCriteria.Values.Where(item => !businessCriteria.ContainsKey(item.Id)))
        {
            issues.Add(new CatalogIssue(
                "GTLY006",
                $"TestUse references unknown criterion {useCase.Id}/{criterion.Id}.",
                CatalogIssueSeverity.Error,
                testUse.Path,
                criterion.Line));
        }
    }

    private static void AddDuplicateDocumentIssues(
        IEnumerable<UseCaseDocument> documents,
        ICollection<CatalogIssue> issues)
    {
        foreach (var group in documents.GroupBy(
                     document => new { document.Kind, document.Id }))
        {
            foreach (var duplicate in group.Skip(1))
            {
                issues.Add(new CatalogIssue(
                    "GTLY003",
                    $"Duplicate {duplicate.Kind} document ID {duplicate.Id}.",
                    CatalogIssueSeverity.Error,
                    duplicate.Path,
                    duplicate.TitleLine));
            }
        }
    }

    private static void AddDuplicateCriterionIssues(
        IEnumerable<UseCaseDocument> documents,
        ICollection<CatalogIssue> issues)
    {
        foreach (var document in documents)
        {
            foreach (var group in document.Criteria.GroupBy(criterion => criterion.Id, StringComparer.Ordinal))
            {
                foreach (var duplicate in group.Skip(1))
                {
                    issues.Add(new CatalogIssue(
                        "GTLY004",
                        $"Duplicate criterion {document.Id}/{duplicate.Id}.",
                        CatalogIssueSeverity.Error,
                        document.Path,
                        duplicate.Line));
                }
            }
        }
    }
}
