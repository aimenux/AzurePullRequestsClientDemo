namespace Lib.Configuration;

public class Settings
{
    public string ApiVersion { get; set; } = "api-version=6.0";

    public string? RepositoryId { get; set; }

    public string? OrganizationName { get; set; }

    public string? PersonalAccessToken { get; set; }

    public string AzureDevopsUrl => @"https://dev.azure.com";

    public string? PullRequestStatus { get; set; } = "Completed";

    public string PullRequestsUrl => $"https://dev.azure.com/{OrganizationName}/_apis/git/repositories/{RepositoryId}/pullrequests?searchCriteria.status={PullRequestStatus}&{ApiVersion}";
}