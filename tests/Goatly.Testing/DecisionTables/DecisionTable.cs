namespace Goatly.Testing.DecisionTables;

public sealed class DecisionTable<TInput>
{
    private readonly IReadOnlyList<DecisionTableRow<TInput>> _rows;

    public DecisionTable(
        string name,
        string establishedGiven,
        IReadOnlyList<string> decisionColumns,
        IReadOnlyList<DecisionTableRow<TInput>> rows)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(establishedGiven);
        ArgumentNullException.ThrowIfNull(decisionColumns);
        ArgumentNullException.ThrowIfNull(rows);

        if (decisionColumns.Count == 0 || decisionColumns.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException(
                "A decision table needs named decision columns.",
                nameof(decisionColumns));
        }

        if (decisionColumns.Distinct(StringComparer.Ordinal).Count() != decisionColumns.Count)
        {
            throw new ArgumentException(
                "Decision-column names must be unique.",
                nameof(decisionColumns));
        }

        if (rows.Count == 0)
        {
            throw new ArgumentException(
                "A decision table needs at least one row.",
                nameof(rows));
        }

        if (rows.Select(row => row.Id).Distinct(StringComparer.Ordinal).Count() != rows.Count)
        {
            throw new ArgumentException(
                "Decision-table row IDs must be unique.",
                nameof(rows));
        }

        foreach (var row in rows)
        {
            ValidateRow(row, decisionColumns.Count);
        }

        Name = name;
        EstablishedGiven = establishedGiven;
        DecisionColumns = decisionColumns.ToArray();
        _rows = rows.ToArray();
    }

    public string Name { get; }
    public string EstablishedGiven { get; }
    public IReadOnlyList<string> DecisionColumns { get; }
    public IReadOnlyList<DecisionTableRow<TInput>> Rows => _rows;

    public IEnumerable<object[]> AsTheoryData() =>
        _rows.Select(row => new object[] { row });

    private static void ValidateRow(DecisionTableRow<TInput> row, int decisionCount)
    {
        if (string.IsNullOrWhiteSpace(row.Id) ||
            string.IsNullOrWhiteSpace(row.Name) ||
            string.IsNullOrWhiteSpace(row.When) ||
            string.IsNullOrWhiteSpace(row.Then))
        {
            throw new ArgumentException("Decision-table rows must be complete.");
        }

        if (row.Decisions.Count != decisionCount)
        {
            throw new ArgumentException(
                $"Decision-table row '{row.Id}' has {row.Decisions.Count} decisions; " +
                $"expected {decisionCount}.");
        }
    }
}
