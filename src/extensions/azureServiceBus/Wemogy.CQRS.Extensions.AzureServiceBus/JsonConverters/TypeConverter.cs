using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Wemogy.CQRS.Extensions.AzureServiceBus.JsonConverters
{
    public class TypeConverter : JsonConverter<Type>
    {
        public override Type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? typeName = reader.GetString();

            if (typeName is null)
            {
                return typeof(object);
            }

            return Type.GetType(typeName) ?? typeof(object);
        }

        public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.AssemblyQualifiedName);
        }
    }
}
