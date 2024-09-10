using System;
using Newtonsoft.Json;

namespace ContentGeneration.Helpers
{
    internal class EnumJsonConverter<T> : JsonConverter<T> where T : struct, IConvertible
    {
        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public sealed override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var valueString = AdaptString((string)reader.Value!);
            foreach (var value in Enum.GetValues(typeof(T)))
            {
                if (valueString == value.ToString().ToLowerInvariant())
                {
                    return (T)value;
                }
            }

            throw new ArgumentOutOfRangeException(valueString);
        }

        protected virtual string AdaptString(string str)
        {
            return str.ToLowerInvariant();
        }
    }
}