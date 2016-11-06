using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitHot.DAL
{
    interface IGithubStatisticsRepository<T>
    {
      Task<T> Get(Dictionary<string, string> @params);
    }
}