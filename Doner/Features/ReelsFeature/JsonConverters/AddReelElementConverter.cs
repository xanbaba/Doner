using System.Text.Json;
using System.Text.Json.Serialization;
using Contracts.V1.Requests;

namespace Doner.Features.ReelsFeature.JsonConverters;

public class AddReelElementConverter : JsonConverter<AddReelElementRequest>
{
    public override AddReelElementRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        JsonElement? typeProperty = null;
        
        foreach (var property in root.EnumerateObject().Where(element => string.Equals(element.Name, nameof(AddReelElementRequest.ElementType), StringComparison.OrdinalIgnoreCase)))
        {
            typeProperty = property.Value;
            break;
        }

        if (typeProperty is null)
        {
            throw new JsonException($"Missing {nameof(AddReelElementRequest.ElementType)} property.");
        }


        var type = typeProperty.Value.GetString();
        if (!Enum.TryParse<ReelElementType>(type, true, out var reelElementType))
        {
            throw new JsonException($"Unknown {nameof(AddReelElementRequest.ElementType)}: {type}");
        }

        AddReelElementRequest? request = reelElementType switch
        {
            ReelElementType.Checkbox => JsonSerializer.Deserialize<AddCheckboxRequest>(root.GetRawText(), options),
            ReelElementType.PlainText => JsonSerializer.Deserialize<AddPlainTextRequest>(root.GetRawText(), options),
            ReelElementType.Dropdown => JsonSerializer.Deserialize<AddDropdownRequest>(root.GetRawText(), options),
            ReelElementType.Picture => JsonSerializer.Deserialize<AddPictureRequest>(root.GetRawText(), options),
            _ => throw new Exception("This can never happen.")
        };

        if (request is null)
        {
            throw new JsonException($"Failed to deserialize {type}.");
        }
            
        return request;
    }

    public override void Write(Utf8JsonWriter writer, AddReelElementRequest value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize<object>(writer, value, options);
    }
}