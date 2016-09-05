using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
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
                int pageCount = 5;

                StatisticsClient statsClient = new StatisticsClient(new ApiConnection(client.Connection));

                List<KeyValuePair<Repository, int>> topRepositories = new List<KeyValuePair<Repository, int>>();

                const int pageSize = 100;
                for (int page = 1; page <= pageCount; page++)
                {
                    Console.WriteLine($"Page {page}");

                    var searchResult = (await client.Search.SearchRepo(new SearchRepositoriesRequest
                    {
                        Order = SortDirection.Descending,
                        SortField = RepoSearchSort.Updated,
                        PerPage = pageSize,
                        Page = page,
                        Stars = Range.GreaterThan(10)
                    })).Items;

                    var pageRepos = searchResult.ToDictionary(repo => repo, repo => statsClient.GetCommitActivity(repo.Owner.Login, repo.Name));


                    foreach (var result in pageRepos)
                    {
                        topRepositories.Add(new KeyValuePair<Repository, int>(result.Key, (await result.Value).Activity.Skip(52 - weeks).Select(week => week.Total).Sum()));
                    }
                    
                    topRepositories.Sort((x, y) => y.Value.CompareTo(x.Value));
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
                int pageCount = 1;

                StatisticsClient statsClient = new StatisticsClient(new ApiConnection(client.Connection));

                List<KeyValuePair<Repository, int>> topRepositories = new List<KeyValuePair<Repository, int>>();

                const int pageSize = 100;
                for (int page = 1; page <= pageCount; page++)
                {
                    Console.WriteLine($"Page {page}");

                    var searchResult = (await client.Search.SearchRepo(new SearchRepositoriesRequest
                    {
                        Order = SortDirection.Descending,
                        SortField = RepoSearchSort.Updated,
                        PerPage = pageSize,
                        Page = page,
                        Stars = Range.GreaterThan(10)
                    })).Items;

                    var pageReposByStars = searchResult.ToDictionary(repo => repo, repo => client.Activity.Starring
                                                                                        .GetStarCount(repo, TimeSpan.FromDays(weeks * 7))
                                                                                        .ToTask());

                    foreach (var result in pageReposByStars)
                    {
                        Console.WriteLine("Fetching Stars...");
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
                int pageCount = 1;

                List<KeyValuePair<Repository, int>> topRepositories = new List<KeyValuePair<Repository, int>>();

                const int pageSize = 100;
                for (int page = 1; page <= pageCount; page++)
                {
                    Console.WriteLine($"Page {page}");

                    var searchResult = (await client.Search.SearchRepo(new SearchRepositoriesRequest
                    {
                        Order = SortDirection.Descending,
                        SortField = RepoSearchSort.Updated,
                        PerPage = pageSize,
                        Page = page,
                        Stars = Range.GreaterThan(10)
                    })).Items;
                    var pageRepos = searchResult.ToDictionary(repo => repo, repo => client.Repository
                                                    .GetContributorsCount(repo, TimeSpan.FromDays(weeks * 7))
                                                    .ToTask());

                    foreach (var result in pageRepos)
                    {
                        Console.WriteLine("Fetching Contributors...");
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
