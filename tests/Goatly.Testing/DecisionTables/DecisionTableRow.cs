namespace Goatly.Testing.DecisionTables;

public sealed record DecisionTableRow<TInput>(
    string Id,
    string Name,
    IReadOnlyList<string> Decisions,
    string When,
    string Then,
    TInput Input);
