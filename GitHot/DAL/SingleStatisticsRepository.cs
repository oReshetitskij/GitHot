using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHot.Core;
using GitHot.Core.POCO;
using Octokit;

namespace GitHot.DAL
{
    class SingleStatisticsRepository : IGithubStatisticsRepository<TrendingRepository>
    {
        public async Task<TrendingRepository> Get(Dictionary<string, string> param)
        {
            GitHubClient github = new GitHubClient(new ProductHeaderValue("GitHot"))
            {
                Credentials = new Credentials(Configuration.Instance.Token)
            };

            Repository repo = await github.Repository.Get(param["owner"], param["repo"]);

            int days = Convert.ToInt32(param["days"]);
            TimeSpan span = TimeSpan.FromDays(days);
            string criteria = param["criteria"];
            RepositoryCriteria selectedCriteria;
            try
            {
                selectedCriteria = (RepositoryCriteria)Enum.Parse(typeof(RepositoryCriteria),
                    criteria, true);
            }
            catch (ArgumentException)
            {
                return null;
            }

            var stats = await github.GetTrendingStats(repo, span, selectedCriteria);
            var allStats = new Dictionary<RepositoryCriteria, int[]>();
            foreach (var crit in Enum.GetValues(typeof(RepositoryCriteria)))
            {
                if ((RepositoryCriteria)crit == selectedCriteria)
                {
                    allStats.Add(selectedCriteria, stats.Item2);
                    continue;
                }

                allStats.Add((RepositoryCriteria)crit, null);
            }
            TrendingRepository data = new TrendingRepository
            {
                Name = repo.Name,
                OwnerName = repo.Owner.Login,
                Id = repo.Id,
                Url = repo.HtmlUrl,
                OwnerUrl = repo.Owner.HtmlUrl,
                Commits = allStats[RepositoryCriteria.Commits],
                Stars = allStats[RepositoryCriteria.Stargazers],
                Contributors = allStats[RepositoryCriteria.Contributors],
                Span = span,
                CreatedAt = DateTime.Now.ToString("yyyy-MM-dd")
            };

            return data;
        }
    }
}