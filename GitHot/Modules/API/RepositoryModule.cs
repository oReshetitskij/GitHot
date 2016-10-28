using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using GitHot.Core;
using GitHot.Core.POCO;
using Octokit;
using Nancy;
using Octokit.Internal;

namespace GitHot.Modules.API
{
    public class RepositoryModule : NancyModule
    {
        private string token = "43c9828893e4dc1d354ad7f760eee3c31fac4970";

        public RepositoryModule()
        {
            Get["/"] = param => View["Index.html"];

            Get["/{owner}/{repo}/{criteria}/{weeks:int}", runAsync: true] = async (param, cancelToken) =>
            {
                GitHubClient github = new GitHubClient(new ProductHeaderValue("GitHot"))
                {
                    Credentials = new Credentials(token)
                };

                Repository repo = await github.Repository.Get(param["owner"], param["repo"]);
                TimeSpan weeks = TimeSpan.FromDays(param["weeks"] * 7);

                string criteria = param["criteria"];
                string json;

                RepositoryCriteria selectedCriteria;
                try
                {
                    selectedCriteria = (RepositoryCriteria) Enum.Parse(typeof(RepositoryCriteria),
                        criteria);
                }
                catch (ArgumentException)
                {
                    json = "{ error: 'Invalid criteria' }";
                    var errorResponse = (Response) json;
                    errorResponse.ContentType = "application/json";
                    return errorResponse;
                }

                var stats = await github.GetTrendingStats(repo, weeks, selectedCriteria);
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
                    Span = weeks
                };

                SimpleJsonSerializer serializer = new SimpleJsonSerializer();
                json = serializer.Serialize(data);
                Response response = (Response)json;
                response.ContentType = "application/json";

                return response;
            };
        }
    }
}