using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Octokit;
using Octokit.Reactive;

namespace GitHot.Core
{
    public static class ObservableSearchClientExtensions
    {
        //TODO: Use dry to decouple repeated code parts into separate method
        public static IObservable<Dictionary<Repository, int>> GetTopRepositoriesByCommits(
            this IObservableGitHubClient client, int weeks, int count)
        {
            return Observable.Create<Dictionary<Repository, int>>(async (observer) =>
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
                    try
                    {
                        foreach (var result in pageRepos)
                        {
                            topRepositories.Add(new KeyValuePair<Repository, int>(result.Key, (await result.Value).Activity.Skip(52 - weeks).Select(week => week.Total).Sum()));
                        }
                    }
                    catch (RateLimitExceededException)
                    {
                        if (client.Connection.Credentials.GetToken() == Configuration.Instance.Token)
                        {
                            client.Connection.Credentials = new Credentials(Configuration.Instance.HelperToken);
                            page -= 1;
                            continue;
                        }

                        throw;
                    }

                    topRepositories.Sort((x, y) => -x.Value.CompareTo(y.Value));
                    topRepositories = topRepositories.Take(count).ToList();
                }

                observer.OnNext(topRepositories.ToDictionary(pair => pair.Key, pair => pair.Value));
                observer.OnCompleted();

                return Disposable.Empty;
            });
        }

        public static IObservable<Dictionary<Repository, int>> GetTopRepositoriesByStargazers(this IObservableGitHubClient client,
            int weeks, int count)
        {
            return Observable.Create<Dictionary<Repository, int>>(async (observer) =>
            {
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

                    var pageReposByStars = searchResult.ToDictionary(repo => repo, repo => client.Activity.Starring
                                                                                        .GetStarCount(repo, TimeSpan.FromDays(weeks * 7))
                                                                                        .ToTask());

                    foreach (var result in pageReposByStars)
                    {
                        topRepositories.Add(new KeyValuePair<Repository, int>(result.Key, (await result.Value).Sum()));
                    }

                    topRepositories.Sort((x, y) => y.Value.CompareTo(x.Value));
                    topRepositories = topRepositories.Take(count).ToList();
                }

                return Disposable.Empty;
            });
        }

        public static IObservable<Dictionary<Repository, int>> GetTopRepositoriesByContributors(this IObservableGitHubClient client,
            int weeks, int count)
        {
            return Observable.Create<Dictionary<Repository, int>>(async (observer) =>
            {
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
                    var pageRepos = searchResult.ToDictionary(repo => repo, repo => client.Repository
                                                    .GetContributorsCount(repo, TimeSpan.FromDays(weeks * 7))
                                                    .ToTask());

                    foreach (var result in pageRepos)
                    {
                        topRepositories.Add(new KeyValuePair<Repository, int>(result.Key, (await result.Value).Sum()));
                    }

                    topRepositories.Sort((x, y) => y.Value.CompareTo(x.Value));
                    topRepositories = topRepositories.Take(count).ToList();
                }

                return Disposable.Empty;
            });
        }


    }
}
