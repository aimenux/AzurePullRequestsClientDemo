using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Lib.Models;

public class AzurePullRequests
{
    [JsonProperty("count")]
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonProperty("value")]
    [JsonPropertyName("value")]
    public List<AzurePullRequest>? Items { get; set; }
}