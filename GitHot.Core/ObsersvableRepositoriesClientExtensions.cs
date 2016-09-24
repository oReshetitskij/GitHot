using Octokit;
using Octokit.Reactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace GitHot.Core
{
    public static class ObsersvableRepositoriesClientExtensions
    {
        public static IObservable<int[]> GetCommitCount(this IObservableRepositoriesClient client, Repository repo, TimeSpan span)
        {
            return Observable.Create<int[]>(async (observer) =>
                {
                    DateTime to = DateTime.Now;
                    DateTime from = to.Add(-span);

                    IDictionary<DateTime, Task<int>> commits = await client.Commit.GetAll(repo.Owner.Login, repo.Name, new CommitRequest
                        {
                        Since = from,
                        Until = to
                    }).GroupBy(x => x.Commit.Committer.Date.LocalDateTime.Date)
                        .ToDictionary(pair => pair.Key, pair => pair.Count().ToTask());

                    // Sort by days, as Dictionary order in undefined
                    var commitsByDay = new List<KeyValuePair<DateTime, int>>();

                    for (int i = 1; i <= span.Days; i++)
                    {
                        DateTime date = from.AddDays(i).Date;
                        int commitsCount = commits.ContainsKey(date) ? await commits[date] : 0;
                        commitsByDay.Add(new KeyValuePair<DateTime, int>(date, commitsCount));
                    }

                    commitsByDay.Sort((x, y) => x.Key.CompareTo(y.Key));

                    observer.OnNext(commitsByDay.Select(c => c.Value).ToArray());
                    observer.OnCompleted();

                    return Disposable.Empty;
                }
            );
        }

        public static IObservable<int[]> GetContributorsCount(this IObservableRepositoriesClient client, Repository repo, TimeSpan span)
        {
            return Observable.Create<int[]>(async (observer) =>
                {
                    DateTime to = DateTime.Now;
                    DateTime from = to.Add(-span);

                    IDictionary<DateTime, Task<GitHubCommit[]>> commits = await client.Commit.GetAll(repo.Owner.Login, repo.Name, new CommitRequest
                        {
                        Since = from,
                        Until = to
                    }).GroupBy(x => x.Commit.Committer.Date.LocalDateTime.Date)
                        .ToDictionary(pair => pair.Key, pair => pair.ToArray().ToTask());

                    // Sort by days, as Dictionary order in undefined
                    var contributorsByDay = new List<KeyValuePair<DateTime, int>>();

                    for (int i = 1; i <= span.Days; i++)
                    {
                        DateTime date = from.AddDays(i).Date;

                        int contributorsCount = 0;
                        if (commits.ContainsKey(date))
                        {
                            contributorsCount = (await commits[date])
                                .Select(commit => commit.Author?.Login)
                                .Where(login => login != null)
                                .Distinct()
                                .Count();
                        }

                        contributorsByDay.Add(new KeyValuePair<DateTime, int>(date, contributorsCount));
                    }

                    contributorsByDay.Sort((x, y) => x.Key.CompareTo(y.Key));

                    observer.OnNext(contributorsByDay.Select(c => c.Value).ToArray());
                    observer.OnCompleted();

                    return Disposable.Empty;
                }
            );
        }
    }
}
