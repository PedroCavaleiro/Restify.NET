using System.Text;
using SharpExtended;

// ReSharper disable MemberCanBePrivate.Global

namespace RestifyNet;

public abstract class Endpoint(string? url) {

    private string _url = string.IsNullOrEmpty(url) ? "" : CleanComponent(url);
    private Dictionary<string, string> _query = new();

    public readonly bool CustomUrl = !string.IsNullOrEmpty(url);
    public Uri Url     => Build();
    public Uri Build() {
        if (_query is not { Count: > 0 }) return new Uri(_url);

        var queryString = new StringBuilder("?");
        foreach (var param in _query)
            queryString.Append($"{param.Key}={param.Value}&");

        return new Uri(_url + queryString.ToString()[..^1]);
    }

    #region Version

    /// <summary>
    /// Adds a version to the endpoint url
    /// Example: v1, v2.0, 3, etc.
    /// </summary>
    /// <param name="version">The string to append</param>
    /// <returns>Appends the version to the url internally and returns `this`</returns>
    public Endpoint WithVersion(string version) {
        _url = $"{_url}/{CleanComponent(version)}";
        return this;
    }

    /// <summary>
    /// Adds a version to the endpoint url but by using an Enum, the enum must have the Description attribute
    /// Example: v1, v2.0, 3, etc.
    /// </summary>
    /// <param name="version">The enum with description</param>
    /// <typeparam name="T">The type of the enum</typeparam>
    /// <returns>Appends the version to the url internally and returns `this`</returns>
    public Endpoint WithVersion<T>( T version) where T : Enum =>
        WithVersion(version.GetDescription() ?? version.ToString());

    #endregion

    #region Path

    /// <summary>
    /// Path component to add to the endpoint url
    /// </summary>
    /// <param name="path">The path to the endpoint</param>
    /// <returns>Appends the path to the url internally and returns `this`</returns>
    public Endpoint WithPath(string path) {
        _url = $"{_url}/{CleanComponent(path)}";
        return this;
    }

    /// <summary>
    /// Path component to add to the endpoint url
    /// </summary>
    /// <param name="path">The path to the endpoint</param>
    /// <typeparam name="T">The type of the enum</typeparam>
    /// <returns>Appends the path to the url internally and returns `this`</returns>
    public Endpoint WithPath<T>(T path) where T : Enum =>
        WithPath(path.GetDescription() ?? path.ToString());

    /// <summary>
    /// Path component to add to the endpoint url with parameters to replace
    /// Example: path = "vehicle/{id}/status", parameters = { "id": "123" } => "vehicle/123/status"
    /// </summary>
    /// <param name="path">The path to the endpoint</param>
    /// <param name="parameters">The dictionary with the parameters</param>
    /// <returns>Appends the path to the url internally and returns `this`</returns>
    public Endpoint WithPath(string path, Dictionary<string, string> parameters) {
        var cleanedPath = CleanComponent(path);
        var pathWithParams = parameters.Aggregate(
            cleanedPath,
            (current, param) => current.Replace($"{{{param.Key}}}", param.Value)
        );
        _url = $"{_url}/{pathWithParams}";
        return this;
    }

    /// <summary>
    /// Path component to add to the endpoint url with parameters to replace
    /// Example: path = "vehicle/{id}/status", parameters = { "id": "123" } => "vehicle/123/status"
    /// </summary>
    /// <param name="path">The path to the endpoint</param>
    /// <param name="parameters">The dictionary with the parameters</param>
    /// <returns>Appends the path to the url internally and returns `this`</returns>
    public Endpoint WithPath<T>(T path, Dictionary<string, string> parameters) where T : Enum =>
        WithPath(path.GetDescription() ?? path.ToString(), parameters);

    #endregion

    #region Query

    /// <summary>
    /// Adds the query params, can be added off order
    /// </summary>
    /// <param name="query">A dictionary containing the query parameters</param>
    /// <returns>Appends the path to the url internally and returns `this`</returns>
    public Endpoint WithQuery(Dictionary<string, string> query) {
        _query = query;
        return this;
    }

    #endregion

    /// <summary>
    /// Cleans the component by removing leading and trailing slashes
    /// </summary>
    /// <param name="input">String to clean</param>
    /// <returns>Cleaned string</returns>
    private static string CleanComponent(string input) {
        if (input.StartsWith('/') && input.EndsWith('/'))
            return input[1..^1];
        if (input.StartsWith('/'))
            return input[1..];
        return input.EndsWith('/') ? input[..^1] : input;
    }

}