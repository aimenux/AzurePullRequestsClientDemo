using System.Net.Http.Headers;
using System.Text;
using Lib.Configuration;
using Lib.Models;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace Lib;

public class AzureDevopsClient : IAzureDevopsClient
{
    private readonly HttpClient _httpClient;
    private readonly GitHttpClient _sdkClient;
    private readonly IOptions<Settings> _options;

    public AzureDevopsClient(HttpClient httpClient, GitHttpClient sdkClient, IOptions<Settings> options)
    {
        _options = options;
        _sdkClient = sdkClient;
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Accept.Add(GetAcceptHeader());
        _httpClient.DefaultRequestHeaders.Authorization = GetAuthorizationHeader();
    }

    public Task<ICollection<AzurePullRequest>> GetAzurePullRequestsAsync(AzureDevopsChoice choice, CancellationToken cancellationToken = default)
    {
        return choice switch
        {
            AzureDevopsChoice.Sdk => GetBySdkAsync(cancellationToken),
            AzureDevopsChoice.Rest => GetByRestAsync(cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, $"Unexpected choice {choice}")
        };
    }

    private async Task<ICollection<AzurePullRequest>> GetBySdkAsync(CancellationToken cancellationToken = default)
    {
        var repositoryId = new Guid(_options.Value.RepositoryId!);
        var pullRequestStatus = GetPullRequestStatus(_options.Value.PullRequestStatus!);
        var searchCriteria = new GitPullRequestSearchCriteria
        {
            RepositoryId = repositoryId,
            Status = pullRequestStatus,
        };
        var repository = await _sdkClient.GetRepositoryAsync(repositoryId, cancellationToken: cancellationToken);
        var projectName = repository.Name;
        var results = await _sdkClient.GetPullRequestsByProjectAsync(projectName, searchCriteria, cancellationToken: cancellationToken);
        var pullRequests = results?.Select(x => new AzurePullRequest
        {
            Url = x.Url,
            Title = x.Title,
            Id = x.PullRequestId,
            Status = x.Status.ToString(),
            CreationDate = x.CreationDate,
            ClosedDate = x.ClosedDate
        }).ToList() ?? new List<AzurePullRequest>();
        return pullRequests;
    }

    public async Task<ICollection<AzurePullRequest>> GetByRestAsync(CancellationToken cancellationToken = default)
    {
        var url = _options.Value.PullRequestsUrl;
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        var pullRequests = await response.Content.ReadAsAsync<AzurePullRequests>(cancellationToken);
        return pullRequests.Items ?? new List<AzurePullRequest>();
    }

    private static MediaTypeWithQualityHeaderValue GetAcceptHeader() => new MediaTypeWithQualityHeaderValue("application/json");

    private AuthenticationHeaderValue GetAuthorizationHeader()
    {
        var pat = _options.Value.PersonalAccessToken;
        var bytes = Encoding.ASCII.GetBytes($":{pat}");
        var base64 = Convert.ToBase64String(bytes);
        return new AuthenticationHeaderValue("Basic", base64);
    }

    private static PullRequestStatus GetPullRequestStatus(string value)
    {
        return (PullRequestStatus)Enum.Parse(typeof(PullRequestStatus), value, true);
    }
}