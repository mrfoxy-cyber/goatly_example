using Goatly.Testing.Catalog;
using Goatly.Testing.Metadata;
using Goatly.Testing.Model;

namespace Goatly.Testing.Tests;

public sealed class TestCatalogTests
{
    [Fact]
    public void Complete_metadata_becomes_a_catalog_entry()
    {
        var catalog = TestCatalog.Discover(typeof(CompleteExample));

        var test = Assert.Single(catalog.Tests);
        Assert.Equal("UC-TEST-001", test.UseCaseId);
        Assert.Equal(TestType.Unit, test.Type);
        Assert.IsType<TestBehavior.GivenWhenThen>(test.Behavior);
        var coverage = Assert.IsType<TestCoverageStrategy.HumanVerified>(test.CoverageStrategy);
        var selection = Assert.Single(coverage.Selections);
        Assert.Equal(TestTechnique.ExpertJudgment, selection.Technique);
        Assert.Equal("The normal metadata-discovery path.", selection.What);
        Assert.Equal(
            "A deliberately chosen representative path is sufficient for this framework behavior.",
            selection.Why);
        Assert.Single(test.Covers);
    }

    [Fact]
    public void Missing_test_technique_is_a_catalog_error()
    {
        var catalog = TestCatalog.Discover(typeof(MissingTechniqueExample));

        Assert.False(catalog.IsValid);
        Assert.Empty(catalog.Tests);
        Assert.Contains(catalog.Errors, error => error.Contains("no test-technique selection"));
    }

    [Fact]
    public void Duplicate_test_techniques_are_a_catalog_error()
    {
        var catalog = TestCatalog.Discover(typeof(DuplicateTechniqueExample));

        Assert.False(catalog.IsValid);
        Assert.Empty(catalog.Tests);
        Assert.Contains(catalog.Errors, error => error.Contains("duplicate test techniques"));
    }

    [Theory]
    [InlineData("", "A reason")]
    [InlineData("A selected case", "")]
    public void Technique_metadata_requires_what_and_why(string what, string why)
    {
        Assert.Throws<ArgumentException>(() => new UsesTestTechniqueAttribute(
            TestTechnique.ExpertJudgment,
            what,
            why));
    }

    private sealed class CompleteExample
    {
        [UseCase("UC-TEST-001")]
        [Covers("AC-001", "Complete metadata is discoverable.")]
        [TestType(TestType.Unit)]
        [Behavior(
            "a test method with complete Goatly metadata",
            "the test catalog is discovered",
            "one complete definition is returned")]
        [UsesTestTechnique(
            TestTechnique.ExpertJudgment,
            what: "The normal metadata-discovery path.",
            why: "A deliberately chosen representative path is sufficient for this framework behavior.")]
        public void Complete_metadata_is_discovered()
        {
        }
    }

    private sealed class MissingTechniqueExample
    {
        [UseCase("UC-TEST-002")]
        [TestType(TestType.Unit)]
        [Behavior("a test without a technique", "the catalog is built", "the omission is reported")]
        public void Missing_technique_is_not_silently_accepted()
        {
        }
    }

    private sealed class DuplicateTechniqueExample
    {
        [UseCase("UC-TEST-003")]
        [TestType(TestType.Unit)]
        [Behavior("duplicate technique metadata", "the catalog is built", "the duplication is reported")]
        [UsesTestTechnique(
            TestTechnique.ExpertJudgment,
            what: "The first selection.",
            why: "It represents one deliberate choice.")]
        [UsesTestTechnique(
            TestTechnique.ExpertJudgment,
            what: "The accidental duplicate selection.",
            why: "It should be combined with the first declaration instead.")]
        public void Duplicate_techniques_are_not_silently_accepted()
        {
        }
    }
}
