namespace Showcase.Examples;

public static class Calculator
{
    public static int Add(int left, int right) => left + right;

    public static decimal Divide(decimal dividend, decimal divisor)
    {
        if (divisor == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(divisor), "Divisor must not be zero.");
        }

        return dividend / divisor;
    }
}

