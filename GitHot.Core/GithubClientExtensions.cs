using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;


namespace GitHot.Core
{
    public static class GithubClientExtensions
    {
        public static async Task<Dictionary<RepositoryCriteria, int[]>> GetTrendingStats(this IGitHubClient client,
            Repository repo, TimeSpan span)
        {
            Task<int[]> commits = client.Repository.GetCommitCount(repo, span);
            Task<int[]> contributors = client.Repository.GetContributorsCount(repo, span);
            Task<int[]> stars = Task<int[]>.Factory.StartNew(() => client.Activity.Starring.GetStarCount(repo, span));

            return new Dictionary<RepositoryCriteria, int[]>
                {
                    { RepositoryCriteria.Commits, await commits },
                    { RepositoryCriteria.Contributors, await contributors },
                    { RepositoryCriteria.Stargazers, await stars }
                };
        }

        public static async Task<Tuple<RepositoryCriteria, int[]>> GetTrendingStats(this IGitHubClient client,
            Repository repo, TimeSpan span, RepositoryCriteria criteria)
        {
            switch (criteria)
            {
                case RepositoryCriteria.Commits:
                    return new Tuple<RepositoryCriteria, int[]>(criteria, await client.Repository.GetCommitCount(repo, span));
                case RepositoryCriteria.Contributors:
                    return new Tuple<RepositoryCriteria, int[]>(criteria, await client.Repository.GetContributorsCount(repo, span));
                case RepositoryCriteria.Stargazers:
                    return new Tuple<RepositoryCriteria, int[]>(criteria, await Task.FromResult(client.Activity.Starring.GetStarCount(repo, span)));
                default:
                    throw new ArgumentException("No method found for given criteria", nameof(criteria));
            }
        }

        public static async Task<Dictionary<Repository, int>> GetTopRepositoriesByCommits(this IGitHubClient client, int weeks, int count)
        {
            StatisticsClient statsClient = new StatisticsClient(new ApiConnection(client.Connection));
            List<KeyValuePair<Repository, int>> topRepositories = new List<KeyValuePair<Repository, int>>();

            for (int page = 1; page <= Configuration.Instance.PageCount; page++)
            {
                Console.WriteLine($"Page {page}");

                var searchResult = (await client.Search.SearchRepo(new SearchRepositoriesRequest
                {
                    Order = SortDirection.Descending,
                    SortField = RepoSearchSort.Updated,
                    PerPage = Configuration.Instance.ItemsPerPage,
                    Page = page,
                    Stars = Range.GreaterThan(10),
                })).Items;

                Dictionary<Repository, Task<CommitActivity>> pageRepos = searchResult.ToDictionary(repo => repo,
                        repo => statsClient.GetCommitActivity(repo.Owner.Login, repo.Name));

                foreach (var result in pageRepos)
                {
                    topRepositories.Add(new KeyValuePair<Repository, int>(
                        result.Key,
                        (await result.Value).Activity
                            .Skip(52 - weeks)
                            .Select(week => week.Total)
                            .Sum()
                    ));
                }

                topRepositories.Sort((x, y) => -x.Value.CompareTo(y.Value));
                topRepositories = topRepositories.Take(count).ToList();
            }

            return topRepositories.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public static async Task<Dictionary<Repository, int>> GetTopRepositoriesByContributors(this IGitHubClient client, int weeks, int count)
        {
            StatisticsClient statsClient = new StatisticsClient(new ApiConnection(client.Connection));
            List<KeyValuePair<Repository, int>> topRepositories = new List<KeyValuePair<Repository, int>>();

            DateTime to = DateTime.Now;
            DateTime from = to.AddDays(-weeks * 7);

            for (int page = 1; page <= Configuration.Instance.PageCount; page++)
            {
                Console.WriteLine($"Page {page}");

                var searchResult = (await client.Search.SearchRepo(new SearchRepositoriesRequest
                {
                    Order = SortDirection.Descending,
                    SortField = RepoSearchSort.Updated,
                    PerPage = Configuration.Instance.ItemsPerPage,
                    Page = page,
                    Stars = Range.GreaterThan(10),
                })).Items;

                Dictionary<Repository, Task<IReadOnlyList<Contributor>>> pageRepos = searchResult.ToDictionary(repo => repo,
                        repo => statsClient.GetContributors(repo.Owner.Login, repo.Name));

                foreach (var result in pageRepos)
                {
                    var contributors =
                        (await result.Value).Where(c => c.Weeks.Any(w => w.Week.DateTime.Date > from &&
                                                                         w.Week.DateTime.Date < to &&
                                                                         w.Commits > 0)).ToArray();

                    int contributorsBySpan = contributors.Count();

                    topRepositories.Add(new KeyValuePair<Repository, int>(result.Key, contributorsBySpan));
                }

                topRepositories.Sort((x, y) => -x.Value.CompareTo(y.Value));
                topRepositories = topRepositories.Take(count).ToList();
            }

            return topRepositories.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public static async Task<Dictionary<Repository, int>> GetTopRepositoriesByStargazers(this IGitHubClient client, int weeks, int count)
        {
            StatisticsClient statsClient = new StatisticsClient(new ApiConnection(client.Connection));
            List<KeyValuePair<Repository, int>> topRepositories = new List<KeyValuePair<Repository, int>>();

            for (int page = 1; page <= Configuration.Instance.PageCount; page++)
            {
                Console.WriteLine($"Page {page}");

                var searchResult = (await client.Search.SearchRepo(new SearchRepositoriesRequest
                {
                    Order = SortDirection.Descending,
                    SortField = RepoSearchSort.Updated,
                    PerPage = Configuration.Instance.ItemsPerPage,
                    Page = page,
                    Stars = Range.GreaterThan(10)
                })).Items;

                Dictionary<Repository, Task<int[]>> pageRepos = searchResult.ToDictionary(repo => repo,
                        repo => Task<int[]>.Factory.StartNew(() =>
                            client.Activity.Starring.GetStarCount(repo, TimeSpan.FromDays(weeks * 7))
                            ));

                foreach (var result in pageRepos)
                {
                    topRepositories.Add(new KeyValuePair<Repository, int>(result.Key, (await result.Value).Sum()));
                }

                topRepositories.Sort((x, y) => -x.Value.CompareTo(y.Value));
                topRepositories = topRepositories.Take(count).ToList();
            }

            return topRepositories.ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}