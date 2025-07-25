using MarkAgent.Domain.Common;
using System.Security.Cryptography;
using System.Text;

namespace MarkAgent.Domain.ValueObjects;

public class UserKey : ValueObject
{
    public string Value { get; }

    private UserKey(string value)
    {
        Value = value;
    }

    public static UserKey Create(string? key = null)
    {
        if (!string.IsNullOrWhiteSpace(key))
        {
            if (!IsValidUserKey(key))
                throw new ArgumentException("User key must start with 'sk-' and be at least 10 characters long.", nameof(key));
            
            return new UserKey(key);
        }

        return GenerateNew();
    }

    public static UserKey GenerateNew()
    {
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        
        var keyValue = Convert.ToBase64String(randomBytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "")
            .Substring(0, 32);
        
        return new UserKey($"sk-{keyValue}");
    }

    private static bool IsValidUserKey(string key)
    {
        return key.StartsWith("sk-") && key.Length >= 10;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(UserKey userKey) => userKey.Value;
}