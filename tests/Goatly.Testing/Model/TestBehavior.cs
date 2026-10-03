namespace Goatly.Testing.Model;

public abstract record TestBehavior
{
    private TestBehavior()
    {
    }

    public sealed record GivenWhenThen(string Given, string When, string Then) : TestBehavior;

    public sealed record NeedsVerification(string Reason) : TestBehavior;
}
