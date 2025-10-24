using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using RestifyNet.Authorizer;
using RestifyNet.Interfaces;
using SharpExtended;

// ReSharper disable NullableWarningSuppressionIsUsed
#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).

namespace RestifyNet;

/// <summary>
/// An easy-to-use REST client
/// </summary>
/// <param name="baseUrl">The base URL </param>
public class EasyRest(string baseUrl): IEasyRest {

    private AuthenticationHeaderValue? _authHeader;
    private HttpClientHandler _defaultHandler = new();
    private JsonSerializerOptions _jsonSerializerOptions = new();
    private RestifyNet.Authorizer.Authorizer? _authorizer;

    /// <inheritdoc />
    public Dictionary<string, string> DefaultHeaders { get; set; } = new();


    /// <summary>
    /// Performs an HTTP request to the specified endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint to perform the request</param>
    /// <param name="method">The method of the request</param>
    /// <param name="body">Body of the request, can be null</param>
    /// <param name="jsonSerializerOptions">Custom JSON serializer options, it overrides the default options</param>
    /// <param name="authHeader">Custom auth header, when null the default will be used</param>
    /// <param name="headers">Headers specific to this request, will be added to the <see cref="DefaultHeaders"/></param>
    /// <param name="handler">A custom HTTP Client Handler, it overrides the default handler</param>
    /// <param name="authorizer">The Authorizer that generates API authentication</param>
    /// <typeparam name="T">The type of response</typeparam>
    /// <typeparam name="TB">The type of the body</typeparam>
    /// <returns>A <see cref="Result"/> with the response</returns>
    private async Task<Result<T?>> Request<T, TB>(
        Endpoint endpoint,
        HttpMethodType method,
        TB? body,
        JsonSerializerOptions? jsonSerializerOptions,
        AuthenticationHeaderValue? authHeader,
        Dictionary<string, string>? headers,
        HttpClientHandler? handler,
        RestifyNet.Authorizer.Authorizer ? authorizer = null
    ) {

        using var client = new HttpClient(handler ?? _defaultHandler);

        if (!endpoint.CustomUrl)
            client.BaseAddress = new Uri(baseUrl);

        if (authHeader != null)
            client.DefaultRequestHeaders.Authorization = authHeader;
        else if (authHeader != null)
            client.DefaultRequestHeaders.Authorization = _authHeader;

        if (headers != null)
            foreach (var header in headers)
                client.DefaultRequestHeaders.Add(header.Key, header.Value);

        foreach (var header in DefaultHeaders)
            client.DefaultRequestHeaders.Add(header.Key, header.Value);

        if (authorizer != null) {
            switch (method) {
                case HttpMethodType.Get or HttpMethodType.Delete:
                    var getHeaders = authorizer.GenerateHeader(
                        method switch {
                            HttpMethodType.Get => Common.GetMethods.Get,
                            HttpMethodType.Delete  => Common.GetMethods.Delete
                        },
                        "{appid}{method}{timestamp}{nonce}"
                    );
                    foreach (var header in getHeaders)
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    break;
                case HttpMethodType.Post or HttpMethodType.Put or HttpMethodType.Patch:
                    var postHeaders = authorizer.GenerateHeader(
                        body!,
                        method switch {
                            HttpMethodType.Post => Common.PostMethods.Post,
                            HttpMethodType.Put  => Common.PostMethods.Put,
                            _                   => Common.PostMethods.Patch
                        }
                    );
                    foreach (var header in postHeaders)
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    break;
            }
        } else if (_authorizer != null) {
            switch (method) {
                case HttpMethodType.Get or HttpMethodType.Delete:
                    var getHeaders = _authorizer.GenerateHeader(
                        method switch {
                            HttpMethodType.Get    => Common.GetMethods.Get,
                            HttpMethodType.Delete => Common.GetMethods.Delete
                        },
                        "{appid}{method}{timestamp}{nonce}"
                    );
                    foreach (var header in getHeaders)
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    break;
                case HttpMethodType.Post or HttpMethodType.Put or HttpMethodType.Patch:
                    var postHeaders = _authorizer.GenerateHeader(
                        body!,
                        method switch {
                            HttpMethodType.Post => Common.PostMethods.Post,
                            HttpMethodType.Put  => Common.PostMethods.Put,
                            _                   => Common.PostMethods.Patch
                        }
                    );
                    foreach (var header in postHeaders)
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    break;
            }
        }

        var httpMethod = method switch {
            HttpMethodType.Get    => HttpMethod.Get,
            HttpMethodType.Post   => HttpMethod.Post,
            HttpMethodType.Put    => HttpMethod.Put,
            HttpMethodType.Delete => HttpMethod.Delete,
            HttpMethodType.Patch  => HttpMethod.Patch,
            _                     => HttpMethod.Get
        };

        using var request = new HttpRequestMessage(httpMethod, endpoint.Build());

        if (body != null && method is HttpMethodType.Post or HttpMethodType.Put or HttpMethodType.Patch) {
            var json = JsonSerializer.Serialize(body, jsonSerializerOptions ?? _jsonSerializerOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        using var response = await client.SendAsync(request);
        var rawContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode) {
            var error = Result.Fail<T>(rawContent);
            error.AddFail($"Request failed with status code {response.StatusCode}");
            return error;
        }

        T? value;
        if (!string.IsNullOrWhiteSpace(rawContent) && typeof(T) != typeof(string)) {
            try {
                value = JsonSerializer.Deserialize<T?>(rawContent, jsonSerializerOptions ?? _jsonSerializerOptions);
                return Result.Ok(value);
            } catch {
                var error = Result.Fail<T>(rawContent);
                error.AddFail("Failed to deserialize response body");
                return error;
            }
        }

        if (typeof(T) != typeof(string)) return Result.Fail<T>(rawContent);
        value = (T?)(object?)rawContent;
        return Result.Ok(value);


    }

    private enum HttpMethodType {
        Get,
        Post,
        Put,
        Delete,
        Patch
    }

    /// <inheritdoc/>
    public async Task<Result<T?>> Get<T>(
        Endpoint endpoint,
        JsonSerializerOptions? jsonSerializerOptions,
        AuthenticationHeaderValue? authHeader,
        Dictionary<string, string>? headers,
        HttpClientHandler? handler,
        RestifyNet.Authorizer.Authorizer ? authorizer = null
    ) => await Request<T, object>(endpoint, HttpMethodType.Get, null, jsonSerializerOptions, authHeader, headers, handler, authorizer);

    /// <inheritdoc/>
    public async Task<Result<T?>> Post<T, TB>(
        Endpoint endpoint,
        TB? body,
        JsonSerializerOptions? jsonSerializerOptions,
        AuthenticationHeaderValue? authHeader,
        Dictionary<string, string>? headers,
        HttpClientHandler? handler,
        RestifyNet.Authorizer.Authorizer ? authorizer = null
    ) => await Request<T, TB>(endpoint, HttpMethodType.Post, body, jsonSerializerOptions, authHeader, headers, handler, authorizer);

    /// <inheritdoc/>
    public async Task<Result<T?>> Delete<T>(
        Endpoint endpoint,
        JsonSerializerOptions? jsonSerializerOptions,
        AuthenticationHeaderValue? authHeader,
        Dictionary<string, string>? headers,
        HttpClientHandler? handler,
        RestifyNet.Authorizer.Authorizer ? authorizer = null
    ) => await Request<T, object>(endpoint, HttpMethodType.Delete, null, jsonSerializerOptions, authHeader, headers, handler, authorizer);

    /// <inheritdoc/>
    public async Task<Result<T?>> Put<T, TB>(
        Endpoint endpoint,
        TB? body,
        JsonSerializerOptions? jsonSerializerOptions,
        AuthenticationHeaderValue? authHeader,
        Dictionary<string, string>? headers,
        HttpClientHandler? handler,
        RestifyNet.Authorizer.Authorizer ? authorizer = null
    ) => await Request<T, TB>(endpoint, HttpMethodType.Put, body, jsonSerializerOptions, authHeader, headers, handler, authorizer);

    /// <inheritdoc/>
    public async Task<Result<T?>> Patch<T, TB>(
        Endpoint endpoint,
        TB? body,
        JsonSerializerOptions? jsonSerializerOptions,
        AuthenticationHeaderValue? authHeader,
        Dictionary<string, string>? headers,
        HttpClientHandler? handler,
        RestifyNet.Authorizer.Authorizer ? authorizer = null
    ) => await Request<T, TB>(endpoint, HttpMethodType.Patch, body, jsonSerializerOptions, authHeader, headers, handler, authorizer);

    /// <inheritdoc/>
    public void SetAuthenticationHeader(string scheme, string parameter) =>
        _authHeader = new AuthenticationHeaderValue(scheme, parameter);

    /// <inheritdoc/>
    public void SetAuthenticationHeader(AuthenticationHeaderValue value) =>
        _authHeader = value;

    /// <inheritdoc/>
    public void SetAuthenticationHeader(string value) =>
        _authHeader = new AuthenticationHeaderValue("Bearer", value);

    /// <inheritdoc/>
    public void ClearAuthenticationHeader() =>
        _authHeader = null;

    /// <inheritdoc/>
    public void SetCustomHttpClientHandler(HttpClientHandler handler) =>
        _defaultHandler = handler;

    /// <inheritdoc/>
    public void ClearCustomHttpClientHandler() =>
        _defaultHandler = new HttpClientHandler();

    /// <inheritdoc/>
    public void SetCustomJsonSerializerOptions(JsonSerializerOptions options) =>
        _jsonSerializerOptions = options;

    /// <inheritdoc/>
    public void ClearCustomJsonSerializerOptions() =>
        _jsonSerializerOptions = new JsonSerializerOptions();

    /// <inheritdoc/>
    public void UseAuthorizer(RestifyNet.Authorizer.Authorizer authorizer) =>
        _authorizer = authorizer;

    /// <inheritdoc/>
    public void ClearAuthorizer() =>
        _authorizer = null;
}