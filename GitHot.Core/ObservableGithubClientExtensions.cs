using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using Octokit.Reactive;

namespace GitHot.Core
{
    public static class ObservableGithubClientExtensions
    {
        public static IObservable<Tuple<RepositoryCriteria, int[]>> GetTrendingStats(this ObservableGitHubClient client, Repository repo, TimeSpan span)
        {
            return Observable.Create<Tuple<RepositoryCriteria, int[]>>(async (observer) =>
            {
                IObservable<int[]> commits = client.Repository.GetCommitCount(repo, span);

                IObservable<int[]> contributors = client.Repository.GetContributorsCount(repo, span);

                ObservableStarredClient s = new ObservableStarredClient(new GitHubClient(client.Connection));

                IObservable<int[]> stars = s.GetStarCount(repo, span);

                observer.OnNext(new Tuple<RepositoryCriteria, int[]>(RepositoryCriteria.Commits, await commits));
                observer.OnNext(new Tuple<RepositoryCriteria, int[]>(RepositoryCriteria.Contributors, await contributors));
                observer.OnNext(new Tuple<RepositoryCriteria, int[]>(RepositoryCriteria.Stargazers, await stars));

                observer.OnCompleted();
            });
        }
    }
}

