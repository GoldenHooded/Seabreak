using ContentGeneration.Helpers;
using Newtonsoft.Json;

namespace ContentGeneration.Models
{
    public enum Generator
    {
        StabilityTextToImage, StabilityImageToImage, StabilityMasking,
        DallETextToImage, DallEInpainting,
        MeshyTextToMesh, MeshyTextToTexture,
        GaxosTextToImage, GaxosMasking
    }
    
    internal class GeneratorTypeConverter : EnumJsonConverter<Generator>
    {
        public static string ToString(Generator generator)
        {
            return generator switch
            {
                Generator.StabilityTextToImage => "stability-text-to-image",
                Generator.StabilityImageToImage => "stability-image-to-image",
                Generator.StabilityMasking => "stability-masking",
                Generator.DallETextToImage => "dall-e-text-to-image",
                Generator.DallEInpainting => "dall-e-inpainting",
                Generator.MeshyTextToMesh => "meshy-text-to-mesh",
                Generator.MeshyTextToTexture => "meshy-text-to-texture",
                Generator.GaxosTextToImage => "gaxos-text-to-image",
                Generator.GaxosMasking => "gaxos-masking",
                _ => generator.ToString()
            };
        }
        public override void WriteJson(JsonWriter writer, Generator value, JsonSerializer serializer)
        {
            writer.WriteValue(ToString(value));
        }

        protected override string AdaptString(string str)
        {
            if (str == "comfy-text-to-image")
            {
                str = "gaxos-text-to-image";
            }

            if (str == "comfy-masking")
            {
                str = "gaxos-masking";
            }
            return base.AdaptString(str).Replace("-", "");
        }
    }
}