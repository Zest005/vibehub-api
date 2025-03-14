using BLL.Abstractions.Utilities;

namespace BLL.Utilities;

internal class GeneratorUtility : IGeneratorUtility
{
    private readonly Random _random;
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";


    public GeneratorUtility()
    {
        _random = new Random();
    }
    
    public string GenerateString(int length)
    {
        return new string(Enumerable.Repeat(Chars, length).Select(s => s[_random.Next(s.Length)]).ToArray());
    }
}