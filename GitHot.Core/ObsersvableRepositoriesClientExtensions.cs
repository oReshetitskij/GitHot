using Octokit;
using Octokit.Reactive;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace GitHot.Core
{
    public static class ObsersvableRepositoriesClientExtensions
    {
        public static IObservable<int[]> GetStarCount(this ObservableGitHubClient observableClient, Repository repo, TimeSpan span)
        {
            DateTime to = DateTime.Now;
            DateTime from = to.Add(-span);

            GitHubClient cl = new GitHubClient(observableClient.Connection);
            ObservableStarredClient s = new ObservableStarredClient(cl);

            return s.GetAllStargazersWithTimestamps(repo.Name, repo.Owner.Name).Select(star => Observable.Start(() =>
            {
                return (star.StarredAt > from && star.StarredAt < to) ? star : null;
            })).Merge(100).GroupBy(x => x.StarredAt.Date).SelectMany(group => group.Count()).ToArray();
        }

        public static IObservable<int[]> GetCommitCount(this ObservableRepositoriesClient client, Repository repo, TimeSpan span)
        {
            DateTime to = DateTime.Now;
            DateTime from = to.Add(-span);

            return client.Commit.GetAll(repo.Owner.Name, repo.Name, new CommitRequest()
            {
                Since = from,
                Until = to
            }).Select(commit => Observable.Start(() => commit))
              .Merge(100)
              .GroupBy(x => x.Commit.Committer.Date.Date).SelectMany(group => group.Count()).ToArray();
        }

        public static IObservable<int[]> GetContributorsCount(this ObservableRepositoriesClient client, Repository repo, TimeSpan span)
        {
            DateTime to = DateTime.Now;
            DateTime from = to.Add(-span);

            return client.Commit.GetAll(repo.Owner.Name, repo.Name, new CommitRequest()
            {
                Since = from,
                Until = to
            }).Select(commit => Observable.Start(() => commit))
              .Merge(100)
              .GroupBy(x => x.Commit.Committer.Name).SelectMany(group => group.Count()).ToArray();
        }
    }
}
