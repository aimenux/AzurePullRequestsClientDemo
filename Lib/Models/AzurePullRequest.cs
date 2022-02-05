using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Lib.Models;

public class AzurePullRequest
{
    [JsonProperty("pullRequestId")]
    [JsonPropertyName("pullRequestId")]
    public int Id { get; set; }

    [JsonProperty("title")]
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonProperty("description")]
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonProperty("url")]
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonProperty("status")]
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonProperty("creationDate")]
    [JsonPropertyName("creationDate")]
    public DateTime CreationDate { get; set; }

    [JsonProperty("closedDate")]
    [JsonPropertyName("closedDate")]
    public DateTime ClosedDate { get; set; }
}