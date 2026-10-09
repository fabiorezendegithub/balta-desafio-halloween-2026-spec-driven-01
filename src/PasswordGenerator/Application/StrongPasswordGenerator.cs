using System.Security.Cryptography;

namespace PasswordGenerator.Application;

public sealed class StrongPasswordGenerator : IPasswordGenerator
{
    private const int Length = 16;
    private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
    private const string Digits = "0123456789";
    private const string Specials = "!@#$%^&*()-_=+[]{}";

    private static readonly string[] Categories = [Uppercase, Lowercase, Digits, Specials];

    public string Generate()
    {
        var all = string.Concat(Categories);
        var chars = new char[Length];

        for (var i = 0; i < Categories.Length; i++)
        {
            chars[i] = Pick(Categories[i]);
        }

        for (var i = Categories.Length; i < Length; i++)
        {
            chars[i] = Pick(all);
        }

        Shuffle(chars);

        return new string(chars);
    }

    private static char Pick(string set) =>
        set[RandomNumberGenerator.GetInt32(set.Length)];

    private static void Shuffle(char[] chars)
    {
        for (var i = chars.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }
    }
}
