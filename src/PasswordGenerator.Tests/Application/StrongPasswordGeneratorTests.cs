using PasswordGenerator.Application;

namespace PasswordGenerator.Tests.Application;

public class StrongPasswordGeneratorTests
{
    private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
    private const string Digits = "0123456789";
    private const string Specials = "!@#$%^&*()-_=+[]{}";
    private const int Generations = 10_000;

    private readonly StrongPasswordGenerator _generator = new();

    [Fact]
    public void Generate_ReturnsExactly16Characters()
    {
        for (var i = 0; i < Generations; i++)
        {
            var password = _generator.Generate();

            Assert.Equal(16, password.Length);
        }
    }

    [Fact]
    public void Generate_ContainsAtLeastOneOfEachCategory()
    {
        for (var i = 0; i < Generations; i++)
        {
            var password = _generator.Generate();

            Assert.Contains(password, c => Uppercase.Contains(c));
            Assert.Contains(password, c => Lowercase.Contains(c));
            Assert.Contains(password, c => Digits.Contains(c));
            Assert.Contains(password, c => Specials.Contains(c));
        }
    }

    [Fact]
    public void Generate_UsesOnlyAllowedCharacters()
    {
        var allowed = Uppercase + Lowercase + Digits + Specials;

        for (var i = 0; i < Generations; i++)
        {
            var password = _generator.Generate();

            Assert.All(password, c => Assert.Contains(c, allowed));
        }
    }

    [Fact]
    public void Generate_NeverContainsWhitespace()
    {
        for (var i = 0; i < Generations; i++)
        {
            var password = _generator.Generate();

            Assert.DoesNotContain(password, char.IsWhiteSpace);
        }
    }

    [Fact]
    public void Generate_ProducesDifferentPasswords()
    {
        var passwords = new HashSet<string>();

        for (var i = 0; i < Generations; i++)
        {
            passwords.Add(_generator.Generate());
        }

        Assert.Equal(Generations, passwords.Count);
    }
}
