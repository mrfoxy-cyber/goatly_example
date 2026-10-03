using Showcase.Examples;

namespace Showcase.Examples.Tests;

public sealed class CalculatorTests
{
    [Theory]
    [InlineData(2, 3, 5)]
    [InlineData(-2, 2, 0)]
    [InlineData(int.MaxValue, 0, int.MaxValue)]
    [UseCase(typeof(Criteria.UC_CALC_001))]
    [Covers(typeof(Criteria.UC_CALC_001.AC_001))]
    [TestType(TestType.Unit)]
    [Behavior(
        "two integer operands",
        "addition is requested",
        "their arithmetic sum is returned")]
    [UsesTestTechnique(
        TestTechnique.BoundaryValueAnalysis,
        what: "Positive, negative, zero-result, and maximum-value operands.",
        why: "Integer boundaries and sign changes are the meaningful risks for this operation.")]
    public void Add_returns_the_arithmetic_sum(int left, int right, int expected)
    {
        Assert.Equal(expected, Calculator.Add(left, right));
    }

    [Fact]
    [UseCase(typeof(Criteria.UC_CALC_001))]
    [Covers(typeof(Criteria.UC_CALC_001.AC_002))]
    [TestType(TestType.Unit)]
    [Behavior(
        "a division request with a zero divisor",
        "division is attempted",
        "the invalid divisor is rejected")]
    [UsesTestTechnique(
        TestTechnique.EquivalencePartitioning,
        what: "The invalid zero-divisor partition.",
        why: "Every zero divisor has the same rejection behavior.")]
    public void Divide_rejects_a_zero_divisor()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Calculator.Divide(10, 0));
    }
}

