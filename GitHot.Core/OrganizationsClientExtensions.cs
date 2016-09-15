using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Octokit;
using Octokit.Internal;

namespace GitHot.Core
{
    class OrganizationsClientExtensions
    {
        public static async Task<int> GetOrganizationCommitCount(this OrganizationsClient client, Organization org, TimeSpan span, IGitHubClient github)
        {
            List<User> members = new List<User>();
            SimpleJsonSerializer serializer = new SimpleJsonSerializer();

            DateTime to = DateTime.Now;
            DateTime from = to.Add(-span);

            int lastPage = GithubApiHelpers.GetLastPage($"https://api.github.com/orgs/{org.Login}/public_members");

            for (int page = 0; page < lastPage; page++)
            {
                Repository[] repos = (await github.Search.SearchRepo(new SearchRepositoriesRequest($"user:{org.Login}")
                {
                    Updated = DateRange.Between(from, to),
                    Page = page,
                    SortField = RepoSearchSort.Updated
                })).Items.ToArray();
            }
        }
    }
}

