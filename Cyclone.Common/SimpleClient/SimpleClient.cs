using System.Text;
using System.Text.Json;
using Cyclone.Common.SimpleEntity;
using Cyclone.Common.SimpleResponse;

namespace Cyclone.Common.SimpleClient;

public class SimpleClient(HttpClient http)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    public async Task<Response<Guid?>> GetByIdAsync<T>(Guid id, CancellationToken? ct = null) where T : BaseEntity
    {
        var typeName  = typeof(T).Name.ToCamelCase();
        var fieldName = $"{typeName}ById";
        var queryText = 
            $$"""
              query Get{{typeName}}ById($id: UUID!) {
                          {{fieldName}}(id: $id) {
                              id
                          }
                      }
              """;
        var body = new
        {
            query = queryText,
            operationName = $"Get{typeName}ById",
            variables = new { id }
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, "");
        req.Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");

        using var resp = await http.SendAsync(req);

        var text = await resp.Content.ReadAsStringAsync();

        try
        {
            var env = JsonSerializer.Deserialize<GraphQlResponse<JsonElement>>(text, JsonOptions);

            if (env is not null)
            {
                if (env.Errors is { Length: > 0 })
                {
                    var msg = string.Join("; ", env.Errors.Select(e => e.Message));
                    throw new InvalidOperationException($"GraphQL errors: {msg}");
                }

                if (env.Data.ValueKind == JsonValueKind.Object &&
                    env.Data.TryGetProperty(fieldName, out var node) &&
                    node.ValueKind == JsonValueKind.Object &&
                    node.TryGetProperty("id", out var idProp) &&
                    idProp.ValueKind == JsonValueKind.String &&
                    Guid.TryParse(idProp.GetString(), out var parsed))
                {
                    return parsed;
                }

                if (env.Data.ValueKind != JsonValueKind.Undefined)
                    return $"Сущности с id--{id.ToString()} не существует";
            }
        }
        catch (JsonException)
        {
        }


        if (!resp.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}. Body: {text}");

        throw new InvalidOperationException($"Unexpected GraphQL response: {text}");
    }
}

public record GraphQlError(string Message);
public record DisplayDto(Guid Id);
public record GetByIdData(Guid? GetDisplayById);
public record GraphQlResponse<T>(T? Data, GraphQlError[]? Errors);