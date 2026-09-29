using System.Text.Json;

namespace Ecommerce.Api.Helpers;

/// <summary>
/// JSON settings used by the non-MVC layers (exception middleware, authentication events) so that
/// every response - including errors - uses the same camelCase contract as the controllers.
/// </summary>
public static class ApiJson
{
    public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
}
