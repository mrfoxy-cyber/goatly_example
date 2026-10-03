namespace Goatly.UseCases;

public enum UseCaseDocumentKind
{
    UseCase = 1,
    TestUse = 2
}

public sealed class AcceptanceCriterionDefinition
{
    public AcceptanceCriterionDefinition(
        string id,
        string text,
        string fingerprint,
        int line,
        string status)
    {
        Id = id;
        Text = text;
        Fingerprint = fingerprint;
        Line = line;
        Status = status;
    }

    public string Id { get; }
    public string Text { get; }
    public string Fingerprint { get; }
    public int Line { get; }
    public string Status { get; }
}

public sealed class UseCaseDocument
{
    public UseCaseDocument(
        string id,
        string status,
        string path,
        UseCaseDocumentKind kind,
        string fingerprint,
        int titleLine,
        IReadOnlyList<AcceptanceCriterionDefinition> criteria)
    {
        Id = id;
        Status = status;
        Path = path;
        Kind = kind;
        Fingerprint = fingerprint;
        TitleLine = titleLine;
        Criteria = criteria;
    }

    public string Id { get; }
    public string Status { get; }
    public string Path { get; }
    public UseCaseDocumentKind Kind { get; }
    public string Fingerprint { get; }
    public int TitleLine { get; }
    public IReadOnlyList<AcceptanceCriterionDefinition> Criteria { get; }
}
