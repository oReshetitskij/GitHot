using System;
using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace GitHot.Core
{
    public static class OrganizationsClientExtensions
    {
        public static async Task<int> GetOrganizationCommitCount(this IOrganizationsClient client, User org, int weeks, IGitHubClient github)
        {
            DateTime to = DateTime.Now;
            DateTime from = to.Add(-TimeSpan.FromDays(7 * weeks));

            int lastPage = GithubApiHelpers.GetLastPage($"https://api.github.com/search/repositories/q=user:{org.Login}");

            int commitsCount = 0;
            for (int page = lastPage; page >= lastPage - Configuration.Instance.PageCount; page++)
            {
                Repository[] repos = (await github.Search.SearchRepo(new SearchRepositoriesRequest($"user:{org.Login}")
                {
                    Updated = DateRange.Between(from, to),
                    Page = page,
                    PerPage = Configuration.Instance.ItemsPerPage,
                    SortField = RepoSearchSort.Updated
                })).Items.ToArray();

                StatisticsClient statClient = new StatisticsClient(new ApiConnection(github.Connection));
                var commits = repos.Select(repo => statClient.GetCommitActivity(repo.Owner.Login, repo.Name)).ToArray();

                for (int i = 0; i < commits.Length; i++)
                {
                    commitsCount += (await commits[i]).Activity
                            .Skip(52 - weeks)
                            .Select(week => week.Total)
                            .Sum();
                }
            }

            return commitsCount;
        }

        public static async Task<double> GetOrganizationMemberAverageCommitCount(this IOrganizationsClient client, User org, int weeks, IGitHubClient github)
        {
            int days = weeks * 7;

            DateTime to = DateTime.Now;
            DateTime from = to.Add(-TimeSpan.FromDays(days));

            int lastPage = GithubApiHelpers.GetLastPage($"https://api.github.com/search/repositories/q=user:{org.Login}");

            StatisticsClient statClient = new StatisticsClient(new ApiConnection(github.Connection));

            int totalRepoCommits = 0;
            int totalRepoContributors = 0;
            for (int page = lastPage; page >= lastPage - Configuration.Instance.PageCount; page++)
            {
                Repository[] repos = (await github.Search.SearchRepo(new SearchRepositoriesRequest($"user:{org.Login}")
                {
                    Updated = DateRange.Between(from, to),
                    Page = page,
                    PerPage = Configuration.Instance.ItemsPerPage,
                    SortField = RepoSearchSort.Updated
                })).Items.ToArray();

                var commitsByRepo = repos.ToDictionary(repo => repo, repo => statClient.GetCommitActivity(repo.Owner.Login, repo.Name));
                var contributorsByRepo = repos.ToDictionary(repo => repo, repo => github.Repository.GetContributorsCount(repo, -TimeSpan.FromDays(days)));

                foreach (var repoCommits in commitsByRepo)
                {
                    totalRepoCommits += (await repoCommits.Value).Activity
                                                .Skip(52 - weeks)
                                                .Select(week => week.Total)
                                                .Sum();

                    totalRepoContributors += (await contributorsByRepo[repoCommits.Key]).Sum();
                }
            }

            return (double)totalRepoCommits / totalRepoContributors;
        }
    }
}

