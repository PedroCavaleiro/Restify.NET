using System.ComponentModel;

namespace RestifyNet.Authorizer;

/// <summary>
/// Class containing shared methods and enums
/// </summary>
public static class Common {

    /// <summary>
    /// The allowed methods for requests with body
    /// </summary>
    public enum PostMethods {
        /// <summary>
        /// Method POST
        /// </summary>
        [Description("POST")]
        Post,
        /// <summary>
        /// Method PATCH
        /// </summary>
        [Description("PATCH")]
        Patch,
        /// <summary>
        /// Method PUT
        /// </summary>
        [Description("PUT")]
        Put
    }

    /// <summary>
    /// The allowed methods for requests without body
    /// </summary>
    public enum GetMethods {
        /// <summary>
        /// Method GET
        /// </summary>
        [Description("GET")]
        Get,
        /// <summary>
        /// Method HEAD
        /// </summary>
        [Description("HEAD")]
        Head,
        /// <summary>
        /// Method DELETE
        /// </summary>
        [Description("DELETE")]
        Delete
    }

    /// <summary>
    /// Replaces the signature format with the proper signature values
    /// The signature format must be passed as reference to it's done "in place" thus not returning any value
    /// </summary>
    /// <param name="signature">The signature format</param>
    /// <param name="values">The values to replace on the signature</param>
    internal static void ReplacePlaceholders(ref string signature, Dictionary<string, string> values) =>
        signature = values.Aggregate(signature, (current, value) => current.Replace("{" + value.Key + "}", value.Value));
}