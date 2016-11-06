using System.Collections.Generic;
using GitHot.Core.POCO;
using GitHot.DAL;
using Nancy;
using Octokit.Internal;

namespace GitHot.Modules.API
{
    public class RepositoryModule : NancyModule
    {
        public RepositoryModule(IRootPathProvider rootPathProvider)
        {
            Get["/{owner}/{repo}/{criteria}/{days:int}", runAsync: true] = async (param, cancelToken) =>
            {
                Dictionary<string, string> statParams = new Dictionary<string, string>
                {
                    { "owner",    param["owner"] },
                    { "repo",     param["repo"] },
                    { "criteria", param["criteria"] },
                    { "days",    param["days"] }
                };

                TrendingRepository data = await new SingleStatisticsRepository().Get(statParams);

                SimpleJsonSerializer serializer = new SimpleJsonSerializer();
                string json;
                Response response;
                if (data != null)
                {
                    json = serializer.Serialize(data);
                    response = json;
                    response.StatusCode = HttpStatusCode.OK;
                }
                else
                {
                    json = "{ 'error': 'Invalid search criterias' }";
                    response = json;
                    response.StatusCode = HttpStatusCode.BadRequest;
                }

                response.ContentType = "application/json";
                return response;
            };
        }
    }
}