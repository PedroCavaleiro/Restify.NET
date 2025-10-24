using System.Net.Http.Headers;
using System.Text.Json;
using SharpExtended;

namespace RestifyNet.Interfaces;

public interface IEasyRest {
    /// <summary>
    /// Performs a GET request to the specified endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint where the request should be performed</param>
    /// <param name="jsonSerializerOptions">The JSON serializer options for the response</param>
    /// <param name="authHeader">
    /// The authentication header to send (if not provided nor configured in the RestifyNet instance it will not send any)
    /// </param>
    /// <param name="headers">Extra headers to send with the request</param>
    /// <param name="handler">A custom HTTP client handler</param>
    /// <param name="authorizer">The Authorizer that generates API authentication</param>
    /// <typeparam name="T">The type of the response</typeparam>
    /// <returns>A result with the response</returns>
    public Task<Result<T?>> Get<T>(
        Endpoint endpoint,
        JsonSerializerOptions? jsonSerializerOptions,
        AuthenticationHeaderValue? authHeader,
        Dictionary<string, string>? headers,
        HttpClientHandler? handler,
        RestifyNet.Authorizer.Authorizer ? authorizer = null
    );

    /// <summary>
    /// Performs a Post request to the specified endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint where the request should be performed</param>
    /// <param name="body">THe body to send</param>
    /// <param name="jsonSerializerOptions">The JSON serializer options for the response</param>
    /// <param name="authHeader">
    /// The authentication header to send (if not provided nor configured in the RestifyNet instance it will not send any)
    /// </param>
    /// <param name="headers">Extra headers to send with the request</param>
    /// <param name="handler">A custom HTTP client handler</param>
    /// <param name="authorizer">The Authorizer that generates API authentication</param>
    /// <typeparam name="T">The type of the response</typeparam>
    /// <typeparam name="TB">The type of the body</typeparam>
    /// <returns>A result with the response</returns>
    public Task<Result<T?>> Post<T, TB>(
        Endpoint endpoint,
        TB? body,
        JsonSerializerOptions? jsonSerializerOptions,
        AuthenticationHeaderValue? authHeader,
        Dictionary<string, string>? headers,
        HttpClientHandler? handler,
        RestifyNet.Authorizer.Authorizer ? authorizer = null
    );

    /// <summary>
    /// Performs a Delete request to the specified endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint where the request should be performed</param>
    /// <param name="jsonSerializerOptions">The JSON serializer options for the response</param>
    /// <param name="authHeader">
    /// The authentication header to send (if not provided nor configured in the RestifyNet instance it will not send any)
    /// </param>
    /// <param name="headers">Extra headers to send with the request</param>
    /// <param name="handler">A custom HTTP client handler</param>
    /// <param name="authorizer">The Authorizer that generates API authentication</param>
    /// <typeparam name="T">The type of the response</typeparam>
    /// <returns>A result with the response</returns>
    public Task<Result<T?>> Delete<T>(
        Endpoint endpoint,
        JsonSerializerOptions? jsonSerializerOptions,
        AuthenticationHeaderValue? authHeader,
        Dictionary<string, string>? headers,
        HttpClientHandler? handler,
        RestifyNet.Authorizer.Authorizer ? authorizer = null
    );

    /// <summary>
    /// Performs a Put request to the specified endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint where the request should be performed</param>
    /// <param name="body">THe body to send</param>
    /// <param name="jsonSerializerOptions">The JSON serializer options for the response</param>
    /// <param name="authHeader">
    /// The authentication header to send (if not provided nor configured in the RestifyNet instance it will not send any)
    /// </param>
    /// <param name="headers">Extra headers to send with the request</param>
    /// <param name="handler">A custom HTTP client handler</param>
    /// <param name="authorizer">The Authorizer that generates API authentication</param>
    /// <typeparam name="T">The type of the response</typeparam>
    /// <typeparam name="TB">The type of the body</typeparam>
    /// <returns>A result with the response</returns>
    public Task<Result<T?>> Put<T, TB>(
        Endpoint endpoint,
        TB? body,
        JsonSerializerOptions? jsonSerializerOptions,
        AuthenticationHeaderValue? authHeader,
        Dictionary<string, string>? headers,
        HttpClientHandler? handler,
        RestifyNet.Authorizer.Authorizer ? authorizer = null
    );

    /// <summary>
    /// Performs a Patch request to the specified endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint where the request should be performed</param>
    /// <param name="body">THe body to send</param>
    /// <param name="jsonSerializerOptions">The JSON serializer options for the response</param>
    /// <param name="authHeader">
    /// The authentication header to send (if not provided nor configured in the RestifyNet instance it will not send any)
    /// </param>
    /// <param name="headers">Extra headers to send with the request</param>
    /// <param name="handler">A custom HTTP client handler</param>
    /// <param name="authorizer">The Authorizer that generates API authentication</param>
    /// <typeparam name="T">The type of the response</typeparam>
    /// <typeparam name="TB">The type of the body</typeparam>
    /// <returns>A result with the response</returns>
    public Task<Result<T?>> Patch<T, TB>(
        Endpoint endpoint,
        TB? body,
        JsonSerializerOptions? jsonSerializerOptions,
        AuthenticationHeaderValue? authHeader,
        Dictionary<string, string>? headers,
        HttpClientHandler? handler,
        RestifyNet.Authorizer.Authorizer ? authorizer = null
    );

    /// <summary>
    /// Configures the authentication header to be used as default
    /// </summary>
    /// <param name="scheme">The scheme of the authentication header</param>
    /// <param name="parameter">The value of the authentication header</param>
    public void SetAuthenticationHeader(string scheme, string parameter);

    /// <summary>
    /// Configures the authentication header to be used as default
    /// </summary>
    /// <param name="value">The authentication header</param>
    public void SetAuthenticationHeader(AuthenticationHeaderValue value);

    /// <summary>
    /// Configures the authentication header to be used as default
    /// This will use the scheme as Bearer
    /// </summary>
    /// <param name="value">The bearer token</param>
    public void SetAuthenticationHeader(string value);

    /// <summary>
    /// Clears the default authentication header
    /// </summary>
    public void ClearAuthenticationHeader();

    /// <summary>
    /// Sets a default custom HTTP client handler to be used in requests
    /// </summary>
    /// <param name="handler"></param>
    public void SetCustomHttpClientHandler(HttpClientHandler handler);

    /// <summary>
    /// Clears the default custom HTTP client handler
    /// </summary>
    public void ClearCustomHttpClientHandler();

    /// <summary>
    /// Sets a default custom JSON serializer options to be used in requests
    /// </summary>
    /// <param name="options">The new default JsonSerializerOptions</param>
    public void SetCustomJsonSerializerOptions(JsonSerializerOptions options);

    /// <summary>
    /// Clears the default custom JSON serializer options
    /// </summary>
    public void ClearCustomJsonSerializerOptions();

    /// <summary>
    /// Contains the default headers to be sent with each request
    /// </summary>
    public Dictionary<string, string> DefaultHeaders { get; set; }

    /// <summary>
    /// Uses an Authorizer to generate the authentication headers for each request
    /// https://github.com/PedroCavaleiro/api-app-authentication
    /// </summary>
    public void UseAuthorizer(RestifyNet.Authorizer.Authorizer authorizer);

    /// <summary>
    /// Clears the Authorizer being used
    /// </summary>
    public void ClearAuthorizer();


}