using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
                {RepositoryCriteria.Commits, await commits},
                {RepositoryCriteria.Contributors, await contributors},
                {RepositoryCriteria.Stargazers, await stars}
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

        public static async Task<Dictionary<Repository, int>> GetTopRepositoriesByCommits(this IGitHubClient client,
            int weeks, int count)
        {
            StatisticsClient statsClient = new StatisticsClient(new ApiConnection(client.Connection));
            List<KeyValuePair<Repository, int>> topRepositories = new List<KeyValuePair<Repository, int>>();

            for (int page = 1; page <= Configuration.Instance.PageCount; page++)
            {
                Debug.WriteLine($"Page {page}");
                Debug.Indent();

                var searchResult = (await client.Search.SearchRepo(new SearchRepositoriesRequest
                {
                    Order = SortDirection.Descending,
                    SortField = RepoSearchSort.Updated,
                    PerPage = Configuration.Instance.ItemsPerPage,
                    Page = page,
                    Stars = Range.GreaterThan(100),
                })).Items;

                Dictionary<Repository, Task<List<WeeklyCommitActivity>>> pageRepos = searchResult.ToDictionary(repo => repo,
                    repo => statsClient.GetCommitActivityRaw(repo));

                foreach (var result in pageRepos)
                {
                    try
                    {
                        topRepositories.Add(new KeyValuePair<Repository, int>(
                            result.Key,
                            (await result.Value)
                                .Skip(52 - weeks)
                                .Select(week => week.Total)
                                .Sum()
                        ));
                    }
                    catch (TimeoutException)
                    {
                        Debug.WriteLine($"Couldn't get commit activity for {result.Key.FullName}");
                        continue;
                    }

                    Debug.Unindent();
                }

                topRepositories.Sort((x, y) => -x.Value.CompareTo(y.Value));
                topRepositories = topRepositories.Take(count).ToList();
            }

            return topRepositories.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public static async Task<Dictionary<Repository, int>> GetTopRepositoriesByContributors(
            this IGitHubClient client, int weeks, int count)
        {
            StatisticsClient statsClient = new StatisticsClient(new ApiConnection(client.Connection));
            List<KeyValuePair<Repository, int>> topRepositories = new List<KeyValuePair<Repository, int>>();

            DateTime from = DateTime.Now.AddDays(-weeks * 7);

            for (int page = 1; page <= Configuration.Instance.PageCount; page++)
            {
                Debug.WriteLine($"Page {page}");
                Debug.Indent();

                var searchResult = (await client.Search.SearchRepo(new SearchRepositoriesRequest
                {
                    Order = SortDirection.Descending,
                    SortField = RepoSearchSort.Updated,
                    PerPage = Configuration.Instance.ItemsPerPage,
                    Page = page,
                    Stars = Range.GreaterThan(100),
                })).Items;


                var pageRepos = searchResult.ToDictionary(repo => repo,
                                                repo => statsClient.GetContributorsRaw(repo)).ToList();

                for (int i = 0; i < pageRepos.Count; i++)
                {
                    try
                    {
                        var contributors =
                            (await pageRepos[i].Value).Where(c => c.Weeks.Any(w => w.Week.LocalDateTime.Date > from &&
                                                                             w.Commits > 0));

                        int contributorsBySpan = contributors.Count();
                        topRepositories.Add(new KeyValuePair<Repository, int>(pageRepos[i].Key, contributorsBySpan));
                    }
                    catch (TimeoutException)
                    {
                        Debug.WriteLine($"Exception while processing {pageRepos[i].Key.FullName}");
                    }
                }

                Debug.Unindent();
            }

            topRepositories.Sort((x, y) => -x.Value.CompareTo(y.Value));
            topRepositories = topRepositories.Take(count).ToList();

            return topRepositories.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public static async Task<Dictionary<Repository, int>> GetTopRepositoriesByStargazers(this IGitHubClient client,
            int weeks, int count)
        {
            StatisticsClient statsClient = new StatisticsClient(new ApiConnection(client.Connection));
            List<KeyValuePair<Repository, int>> topRepositories = new List<KeyValuePair<Repository, int>>();

            for (int page = 1; page <= Configuration.Instance.PageCount; page++)
            {
                Debug.WriteLine($"Page {page}");
                Debug.Indent();

                var searchResult = (await client.Search.SearchRepo(new SearchRepositoriesRequest
                {
                    Order = SortDirection.Descending,
                    SortField = RepoSearchSort.Updated,
                    PerPage = Configuration.Instance.ItemsPerPage,
                    Page = page,
                    Stars = Range.GreaterThan(100)
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

                Debug.Unindent();
            }

            return topRepositories.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public static async Task<Dictionary<User, int>> GetTopOrganizationsByTotalCommits(
            this IGitHubClient client, int weeks, int count)
        {
            var topOrganizations = new List<KeyValuePair<User, int>>();

            for (int page = 1; page <= Configuration.Instance.PageCount; page++)
            {
                Debug.WriteLine($"Page {page}");
                Debug.Indent();

                IReadOnlyList<User> searchResult = new List<User>();
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        searchResult = (await client.Search.SearchUsers(new SearchUsersRequest("type:org")
                        {
                            Order = SortDirection.Descending,
                            PerPage = Configuration.Instance.ItemsPerPage,
                            Page = page,
                            Repositories = Range.GreaterThan(3)
                        })).Items;
                        break;
                    }
                    catch (RateLimitExceededException e)
                    {
                        if (i > 3)
                        {
                            Debug.WriteLine("Break through ");
                            throw;
                        }
                        await Task.Delay(200);
                    }
                }

                Dictionary<User, int> pageOrgs = new Dictionary<User, int>();

                foreach (User org in searchResult)
                {
                    Debug.WriteLine($"Organization {org.Login}");
                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            pageOrgs.Add(org, await client.Organization.GetOrganizationCommitCount(org, weeks, client));
                            break;
                        }
                        catch (RateLimitExceededException e)
                        {
                            if (i < 3)
                            {
                                await Task.Delay(50);
                                continue;
                            }

                            Debug.WriteLine("Break through ");
                        }
                    }
                    Debug.WriteLine($"Organization {org.Login}");
                }

                foreach (var result in pageOrgs)
                {
                    topOrganizations.Add(new KeyValuePair<User, int>(result.Key, result.Value));
                }

                topOrganizations.Sort((x, y) => -x.Value.CompareTo(y.Value));
                topOrganizations = topOrganizations.Take(count).ToList();

                Debug.Unindent();
            }

            return topOrganizations.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public static async Task<Dictionary<User, double>> GetTopOrganizationsByMemberAverageCommits(
            this IGitHubClient client, int weeks, int count)
        {
            var topOrganizations = new List<KeyValuePair<User, double>>();

            for (int page = 1; page <= Configuration.Instance.PageCount; page++)
            {
                Debug.WriteLine($"Page {page}");
                Debug.Indent();

                IReadOnlyList<User> searchResult = new List<User>();
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        searchResult = (await client.Search.SearchUsers(new SearchUsersRequest("type:org")
                        {
                            Order = SortDirection.Descending,
                            PerPage = Configuration.Instance.ItemsPerPage,
                            Page = page,
                            Repositories = Range.GreaterThan(3)
                        })).Items;
                        break;
                    }
                    catch (RateLimitExceededException e)
                    {
                        if (i > 3)
                        {
                            Debug.WriteLine("Break through ");
                            throw;
                        }
                        await Task.Delay(200);
                    }
                }

                Dictionary<User, double> pageOrgs = new Dictionary<User, double>();

                foreach (User org in searchResult)
                {
                    Debug.WriteLine($"Organization {org.Login}");
                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            pageOrgs.Add(org, await client.Organization.GetOrganizationMemberAverageCommitCount(org, weeks, client));
                            break;
                        }
                        catch (RateLimitExceededException e)
                        {
                            if (i < 3)
                            {
                                await Task.Delay(50);
                                continue;
                            }

                            Debug.WriteLine("Break through ");
                        }
                    }
                    Debug.WriteLine($"Organization {org.Login}");
                }

                foreach (var result in pageOrgs)
                {
                    topOrganizations.Add(new KeyValuePair<User, double>(result.Key, result.Value));
                }

                topOrganizations.Sort((x, y) => -x.Value.CompareTo(y.Value));
                topOrganizations = topOrganizations.Take(count).ToList();

                Debug.Unindent();
            }

            return topOrganizations.ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}
