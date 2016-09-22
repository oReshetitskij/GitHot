using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GitHot.Core
{
    public static class RepositoriesClientExtensions
    {
        public static async Task<int[]> GetCommitCount(this IRepositoriesClient client, Repository repo, TimeSpan span)
        {
            DateTime to = DateTime.Now;
            DateTime from = to.Add(-span);

            IDictionary<DateTime, int> commits = (await client.Commit.GetAll(repo.Owner.Login, repo.Name, new CommitRequest()
            {
                Since = from,
                Until = to
            })).GroupBy(x => x.Commit.Committer.Date.LocalDateTime.Date)
                .ToDictionary(pair => pair.Key, pair => pair.Count());

            // Sort by days, as Dictionary order in undefined
            var commitsByDay = new List<KeyValuePair<DateTime, int>>();

            for (int i = 1; i <= span.Days; i++)
            {
                DateTime date = from.AddDays(i).Date;
                int commitsCount = commits.ContainsKey(date) ? commits[date] : 0;
                commitsByDay.Add(new KeyValuePair<DateTime, int>(date, commitsCount));
            }

            commitsByDay.Sort((x, y) => -x.Key.CompareTo(y.Key));

            return commitsByDay.Select(x => x.Value).ToArray();
        }

        public static async Task<int[]> GetContributorsCount(this IRepositoriesClient client, Repository repo, TimeSpan span)
        {
            DateTime to = DateTime.Now;
            DateTime from = to.Add(-span);

            IDictionary<DateTime, GitHubCommit[]> commits = (await client.Commit.GetAll(repo.Owner.Login, repo.Name, new CommitRequest()
            {
                Since = from,
                Until = to
            })).GroupBy(x => x.Commit.Committer.Date.LocalDateTime.Date)
                .ToDictionary(pair => pair.Key, pair => pair.ToArray());

            // Sort by days, as Dictionary order in undefined
            var contributorsByDay = new List<KeyValuePair<DateTime, int>>();

            for (int i = 1; i <= span.Days; i++)
            {
                DateTime date = from.AddDays(i).Date;

                int contributorsCount = 0;
                if (commits.ContainsKey(date))
                {
                    contributorsCount = (commits[date])
                        .Select(commit => commit.Author?.Login)
                        .Where(login => login != null)
                        .Distinct()
                        .Count();
                }

                contributorsByDay.Add(new KeyValuePair<DateTime, int>(date, contributorsCount));
            }

            contributorsByDay.Sort((x, y) => x.Key.CompareTo(y.Key));

            return contributorsByDay.Select(x => x.Value).ToArray();
        }
    }
}