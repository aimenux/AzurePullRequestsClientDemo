using Lib.Models;

namespace Lib;

public interface IAzureDevopsClient
{
    Task<ICollection<AzurePullRequest>> GetAzurePullRequestsAsync(AzureDevopsChoice choice, CancellationToken cancellationToken = default);
}