using System;
using System.Net;
using System.Text.RegularExpressions;

namespace GitHot.Core
{
    internal class GithubApiHelpers
    {
        public static int GetLastPage(string request)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://api.github.com/orgs/public_members");
            req.Headers.Add("User-Agent", "GitHot");
            req.Headers.Add("Authorization", $"token {Configuration.Instance.Token}");

            HttpWebResponse response = (HttpWebResponse)req.GetResponse();
            var link = response.Headers["Link"];

            Regex re = new Regex(@"https://api.github.com/organizations/\d+\/public_members\?page=(\d+)>; rel=""last""",
                RegexOptions.Compiled);

            MatchCollection matches = re.Matches(link);
            Match match = matches[matches.Count - 1];
            GroupCollection groups = match.Groups;

            return Convert.ToInt32(groups[0].Value);
        }
    }
}
