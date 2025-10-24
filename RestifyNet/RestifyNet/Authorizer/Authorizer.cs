using System.Text.Json;
using SharpExtended;

// ReSharper disable ClassNeverInstantiated.Global

namespace RestifyNet.Authorizer;

/// <summary>
/// Initializes the class, requires the appId and appKey
/// </summary>
/// <param name="appId">The appId to be authorized</param>
/// <param name="appKey">The appKey to use on the authorization</param>
public class Authorizer(string appId, string appKey) {

    /// <summary>
    /// Generates the headers required for the app authentication with the API for the methods "POST", "PATCH" and "PUT"
    /// </summary>
    /// <typeparam name="T">The type of the body, this is inferred by the parameter</typeparam>
    /// <param name="body">The body of the request, must be a codable struct or a `string` if body is a `string` parameter `isJson` must be set to `false`</param>
    /// <param name="method">The method of the request, optional, defaults to `PostMethods.POST`</param>
    /// <param name="isJson">Sets if the body is a conformable json class or a `string`</param>
    /// <param name="customSig">The signature format that will be used to generate the request signature, optional, defaults to "{appid}{method}{timestamp}{nonce}{bodyhash}"</param>
    /// <returns>The dictionary with the headers required for the authentication</returns>
    public Dictionary<string, string> GenerateHeader<T>(
        T body,
        Common.PostMethods method = Common.PostMethods.Post,
        bool isJson = true,
        string customSig = "{appid}{method}{timestamp}{nonce}{bodyhash}"
    ) {
        var timestamp = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString();
        var nonce = Guid.NewGuid().ToString();
        var signature = customSig;
        var values = new Dictionary<string, string> {
            { "timestamp", timestamp },
            { "nonce", nonce },
            { "appid", appId },
            { "method",  method.GetDescription() ?? "" },
            { "bodyhash", isJson ? Crypto.CalculateSha256(JsonSerializer.Serialize(body)) : Crypto.CalculateSha256(body?.ToString() ?? string.Empty) }
        };

        Common.ReplacePlaceholders(ref signature, values);

        var hmac = Crypto.CalculateHmacSha256(signature, appKey);
        return new Dictionary<string, string> {
            { "X-Req-Timestamp", timestamp },
            { "X-Req-Nonce", nonce },
            { "X-Req-Sig", hmac },
            { "X-App-Id", appId }
        };
    }

    /// <summary>
    /// Generates the headers required for the app authentication with the API for the methods "GET", "HEAD" and "DELETE"
    /// </summary>
    /// <param name="method">The method of the request</param>
    /// <param name="customSig">The signature format that will be used to generate the request signature, optional, defaults to "{appid}{method}{timestamp}{nonce}"</param>
    /// <returns>The dictionary with the headers required for the authentication</returns>
    public Dictionary<string, string> GenerateHeader(
        Common.GetMethods method = Common.GetMethods.Get,
        string customSig = "{appid}{method}{timestamp}{nonce}"
    ) {
        var timestamp = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString();
        var nonce = Guid.NewGuid().ToString();
        var signature = customSig;
        var values = new Dictionary<string, string> {
            { "timestamp", timestamp },
            { "nonce", nonce },
            { "appid", appId },
            { "method",  method.GetDescription() ?? "" }
        };

        Common.ReplacePlaceholders(ref signature, values);

        var hmac = Crypto.CalculateHmacSha256(signature, appKey);
        return new Dictionary<string, string> {
            { "X-Req-Timestamp", timestamp },
            { "X-Req-Nonce", nonce },
            { "X-Req-Sig", hmac },
            { "X-App-Id", appId }
        };
    }

}