using ContentGeneration.Helpers;
using Newtonsoft.Json;

namespace ContentGeneration.Models.Meshy
{
    public enum TextToMeshArtStyle
    {
        Realistic, Cartoon, LowPoly
    }

    internal class TextToMeshArtStyleConverter : EnumJsonConverter<TextToMeshArtStyle>
    {
        public override void WriteJson(JsonWriter writer, TextToMeshArtStyle value, JsonSerializer serializer)
        {
            var str = value switch
            {
                TextToMeshArtStyle.LowPoly => "low-poly",
                _ => value.ToString().ToLowerInvariant(),
            };
            writer.WriteValue(str);
        }

        protected override string AdaptString(string str)
        {
            return base.AdaptString(str).Replace("-", "");
        }
    }
}