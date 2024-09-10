using Newtonsoft.Json;

namespace ContentGeneration.Models.Meshy
{
    public record MeshyTextToMeshParameters
    {
        [JsonProperty("prompt")] public string Prompt;

        [JsonProperty("mode")] string mode => "preview";

        [JsonProperty("negative_prompt", NullValueHandling = NullValueHandling.Ignore)] 
        public string NegativePrompt;

        [JsonProperty("art_style"), JsonConverter(typeof(TextToMeshArtStyleConverter))] 
        public TextToMeshArtStyle ArtStyle = TextToMeshArtStyle.Realistic;
    }
}