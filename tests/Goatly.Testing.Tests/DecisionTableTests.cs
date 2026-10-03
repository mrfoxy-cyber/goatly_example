using Goatly.Testing.DecisionTables;

namespace Goatly.Testing.Tests;

public sealed class DecisionTableTests
{
    [Fact]
    public void Every_row_becomes_separate_theory_data()
    {
        var table = new DecisionTable<string>(
            "Required input",
            "a creation request",
            ["Key supplied"],
            [
                new("key-present", "Key is present", ["Yes"], "creation is attempted", "creation continues", "berry"),
                new("key-missing", "Key is missing", ["No"], "creation is attempted", "validation fails", "")
            ]);

        var rows = table.AsTheoryData().ToArray();

        Assert.Equal(2, rows.Length);
        Assert.Equal("key-present", Assert.IsType<DecisionTableRow<string>>(rows[0][0]).Id);
        Assert.Equal("key-missing", Assert.IsType<DecisionTableRow<string>>(rows[1][0]).Id);
    }

    [Fact]
    public void Duplicate_row_ids_are_rejected()
    {
        var rows = new[]
        {
            new DecisionTableRow<string>("same", "First", ["Yes"], "creation is attempted", "it succeeds", "berry"),
            new DecisionTableRow<string>("same", "Second", ["No"], "creation is attempted", "it fails", "")
        };

        var error = Assert.Throws<ArgumentException>(() =>
            new DecisionTable<string>("Duplicate rows", "a request", ["Valid"], rows));

        Assert.Contains("row IDs must be unique", error.Message);
    }
}
