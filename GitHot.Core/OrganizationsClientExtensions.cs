using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            int commitsCount = 0;

            // Take top 10 latest updated orgs repositories
            Repository[] repos = (await github.Search.SearchRepo(new SearchRepositoriesRequest($"user:{org.Login}")
            {
                Updated = DateRange.Between(from, to),
                Page = 1,
                PerPage = 10
            })).Items.ToArray();

            StatisticsClient statClient = new StatisticsClient(new ApiConnection(github.Connection));
            List<Task<List<WeeklyCommitActivity>>> commits = new List<Task<List<WeeklyCommitActivity>>>();

            foreach (var repo in repos)
            {
                commits.Add(statClient.GetCommitActivityRaw(repo));
            }

            Debug.WriteLine($"Started fetching {org.Login}");
            foreach (Task<List<WeeklyCommitActivity>> t in commits)
            {
                try
                {
                    commitsCount += (await t)
                        .Skip(52 - weeks)
                        .Select(week => week.Total)
                        .Sum();
                }
                catch (TimeoutException e)
                {
                    Debug.WriteLine("[ERROR]: " + e.Message);
                }
            }

            Debug.WriteLine($"Finished fetching {org.Login}");

            return commitsCount;
        }

        public static async Task<double> GetOrganizationMemberAverageCommitCount(this IOrganizationsClient client, User org, int weeks, IGitHubClient github)
        {
            int days = weeks * 7;

            DateTime to = DateTime.Now;
            DateTime from = to.Add(-TimeSpan.FromDays(days));

            StatisticsClient statClient = new StatisticsClient(new ApiConnection(github.Connection));

            int totalRepoCommits = 0;
            int totalRepoContributors = 0;

            // Take top 10 
            Repository[] repos = (await github.Search.SearchRepo(new SearchRepositoriesRequest($"user:{org.Login}")
            {
                Updated = DateRange.Between(from, to),
                Page = 1,
                PerPage = 10
            })).Items.ToArray();

            var commitsByRepo = repos.ToDictionary(repo => repo, repo => statClient.GetCommitActivityRaw(repo));
            var contributorsByRepo = repos.ToDictionary(repo => repo, repo => statClient.GetContributorsRaw(repo));

            Debug.WriteLine($"Started fetching {org.Login}");
            foreach (var repoCommits in commitsByRepo)
            {
                try
                {
                    totalRepoCommits += (await repoCommits.Value)
                        .Skip(52 - weeks)
                        .Select(week => week.Total)
                        .Sum();

                    totalRepoContributors += (await contributorsByRepo[repoCommits.Key]).Sum(c => c.Total);
                }
                catch (TimeoutException e)
                {
                    Debug.WriteLine("[ERROR]: " + e.Message);
                }
            }
            Debug.WriteLine($"Finished fetching {org.Login}");

            if (totalRepoContributors == 0)
            {
                return 0;
            }

            return Math.Round((double)totalRepoCommits / totalRepoContributors, 2);
        }
    }
}

