using System.Reflection;
using Goatly.Testing.Metadata;
using Goatly.Testing.Model;

namespace Goatly.Testing.Catalog;

public sealed class TestCatalog
{
    private TestCatalog(
        IReadOnlyList<TestDefinition> tests,
        IReadOnlyList<string> errors)
    {
        Tests = tests;
        Errors = errors;
    }

    public IReadOnlyList<TestDefinition> Tests { get; }
    public IReadOnlyList<string> Errors { get; }
    public bool IsValid => Errors.Count == 0;

    public static TestCatalog Discover(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        return Discover(assembly.GetTypes());
    }

    public static TestCatalog Discover(params Type[] testContainers)
    {
        ArgumentNullException.ThrowIfNull(testContainers);

        var tests = new List<TestDefinition>();
        var errors = new List<string>();

        var methods = testContainers
            .SelectMany(type => type.GetMethods(
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.Static))
            .Where(method => method.GetCustomAttribute<UseCaseAttribute>() is not null);

        foreach (var method in methods)
        {
            TryAddDefinition(method, tests, errors);
        }

        foreach (var duplicate in tests.GroupBy(test => test.Id).Where(group => group.Count() > 1))
        {
            errors.Add($"Duplicate test ID: {duplicate.Key}");
        }

        return new TestCatalog(tests, errors);
    }

    private static void TryAddDefinition(
        MethodInfo method,
        ICollection<TestDefinition> tests,
        ICollection<string> errors)
    {
        var location = $"{method.DeclaringType?.FullName}.{method.Name}";
        var useCase = method.GetCustomAttribute<UseCaseAttribute>()!;
        var testType = method.GetCustomAttribute<TestTypeAttribute>();
        var behavior = ReadBehavior(method, location, errors);
        var coverage = ReadCoverage(method, location, errors);

        if (testType is null)
        {
            errors.Add($"{location} has no test type.");
        }

        if (testType is null || behavior is null || coverage is null)
        {
            return;
        }

        var claims = method
            .GetCustomAttributes<CoversAttribute>()
            .Select(attribute => new TestClaimReference(
                attribute.UseCaseId,
                attribute.ClaimId,
                attribute.Claim))
            .ToArray();

        foreach (var claim in claims.Where(claim =>
                     claim.UseCaseId is not null &&
                     !string.Equals(claim.UseCaseId, useCase.Id, StringComparison.Ordinal)))
        {
            errors.Add(
                $"{location} belongs to {useCase.Id} but covers criterion " +
                $"{claim.UseCaseId}/{claim.Id}.");
        }

        var declaringType = method.DeclaringType
            ?? throw new InvalidOperationException("A test method must have a declaring type.");

        tests.Add(new TestDefinition(
            MakeId(useCase.Id, declaringType, method.Name),
            Humanize(method.Name),
            useCase.Id,
            testType.Type,
            behavior,
            coverage,
            claims,
            declaringType,
            method.Name));
    }

    private static TestBehavior? ReadBehavior(
        MethodInfo method,
        string location,
        ICollection<string> errors)
    {
        var documented = method.GetCustomAttribute<BehaviorAttribute>();
        var pending = method.GetCustomAttribute<BehaviorNeedsVerificationAttribute>();

        if (documented is not null && pending is not null)
        {
            errors.Add($"{location} has two behavior states.");
            return null;
        }

        if (documented is not null)
        {
            return new TestBehavior.GivenWhenThen(
                documented.Given,
                documented.When,
                documented.Then);
        }

        if (pending is not null)
        {
            return new TestBehavior.NeedsVerification(pending.Reason);
        }

        errors.Add($"{location} has no behavior description or verification marker.");
        return null;
    }

    private static TestCoverageStrategy? ReadCoverage(
        MethodInfo method,
        string location,
        ICollection<string> errors)
    {
        var selections = method
            .GetCustomAttributes<UsesTestTechniqueAttribute>()
            .ToArray();
        var pending = method.GetCustomAttribute<TestCoverageNeedsVerificationAttribute>();

        if (selections.Length > 0 && pending is not null)
        {
            errors.Add($"{location} has two coverage-strategy states.");
            return null;
        }

        var duplicateTechniques = selections
            .GroupBy(selection => selection.Technique)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateTechniques.Length > 0)
        {
            errors.Add(
                $"{location} declares duplicate test techniques: " +
                string.Join(", ", duplicateTechniques));
            return null;
        }

        if (selections.Length > 0)
        {
            return new TestCoverageStrategy.HumanVerified(selections
                .Select(selection => new TestTechniqueSelection(
                    selection.Technique,
                    selection.What,
                    selection.Why))
                .ToArray());
        }

        if (pending is not null)
        {
            return new TestCoverageStrategy.NeedsVerification(pending.Reason);
        }

        errors.Add(
            $"{location} has no test-technique selection with what-and-why reasoning " +
            "or coverage-verification marker.");
        return null;
    }

    private static string MakeId(string useCaseId, Type type, string methodName) =>
        $"{useCaseId}:{type.FullName}.{methodName}";

    private static string Humanize(string methodName) =>
        methodName.Replace('_', ' ');
}
