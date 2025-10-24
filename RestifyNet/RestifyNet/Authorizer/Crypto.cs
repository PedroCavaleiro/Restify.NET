using System.Security.Cryptography;
using System.Text;

namespace RestifyNet.Authorizer;

internal static class Crypto {

    /// <summary>
    /// Calculates the SHA256 of a string
    /// </summary>
    /// <param name="text">String from which will be generated the hash</param>
    /// <returns></returns>
    internal static string CalculateSha256(string text)
    {
        var hashString = string.Empty;
        var bytes = Encoding.UTF8.GetBytes(text);
        var hash = SHA256.HashData(bytes);

        return hash.Aggregate(hashString, (current, x) => current + $"{x:x2}");
    }

    /// <summary>
    /// Calculates the HMAC-SHA256 of a string
    /// </summary>
    /// <param name="text">The string from which will be calculated the authenticated message</param>
    /// <param name="key">The key to create the authenticated message</param>
    /// <returns></returns>
    internal static string CalculateHmacSha256(string text, string key) {
        var bytes = Encoding.UTF8.GetBytes(key);
        using var hasher = new HMACSHA256(bytes);
        var byteArray = Encoding.UTF8.GetBytes(text);
        using var stream = new MemoryStream(byteArray);
        return hasher.ComputeHash(stream).Aggregate("", (s, e) => s + $"{e:x2}", s => s );
    }

}