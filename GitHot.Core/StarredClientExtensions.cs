using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Octokit;
using Octokit.Internal;

namespace GitHot.Core
{
    public static class StarredClientExtensions
    {
        public static int[] GetStarCount(this IStarredClient client, Repository repo, TimeSpan span)
        {
            DateTime to = DateTime.Now;
            DateTime from = to.Add(-span);

            string pattern = "\"starred_at\": \"(.*)\",";
            Regex re = new Regex(pattern, RegexOptions.Compiled);

            int lastPage = repo.StargazersCount / Configuration.Instance.ItemsPerPage;
            const string emptyJson = "[\n\n]\n";

            List<UserStar> starsArray = new List<UserStar>();
            SimpleJsonSerializer serializer = new SimpleJsonSerializer();

            for (int i = lastPage; i > 0; i--)
            {
                using (WebClient web = new WebClient())
                {
                    web.Headers.Add("User-agent",
                            "GitHot");
                    web.Headers.Add("Accept", "application/vnd.github.v3.star+json");
                    web.Headers.Add("Authorization", $"token {Configuration.Instance.Token}");

                    string json = emptyJson;
                    try
                    {
                        json = web.DownloadString(
                            $"https://api.github.com/repos/{repo.FullName}/stargazers?per_page={Configuration.Instance.ItemsPerPage}&page={i}");
                    }
                    catch (WebException e)
                    {
                        // Return empty statistics if faced
                        // 422 (Unprocessable entity) error code
                        var response = e.Response as HttpWebResponse;
                        if (response?.StatusCode == (HttpStatusCode)422)
                        {
                            return new int[span.Days];
                        }

                        throw;
                    }

                    if (json == emptyJson)
                    {
                        break;
                    }

                    MatchCollection matches = re.Matches(json);
                    Match match = matches[matches.Count - 1];
                    GroupCollection groups = match.Groups;
                    DateTime date = Convert.ToDateTime(groups[1].Value).Date;

                    if (date < from)
                    {
                        break;
                    }

                    starsArray.AddRange(serializer.Deserialize<UserStar[]>(json));
                }
            }

            var stars = starsArray.GroupBy(x => x.StarredAt.LocalDateTime.Date)
                .ToDictionary(pair => pair.Key, pair => pair.Count());

            // Sort by days, as Dictionary order in undefined
            var starsByDay = new List<KeyValuePair<DateTime, int>>();

            for (int i = 1; i <= span.Days; i++)
            {
                DateTime date = from.AddDays(i).Date;
                int starsCount = stars.ContainsKey(date) ? stars[date] : 0;
                starsByDay.Add(new KeyValuePair<DateTime, int>(date, starsCount));
            }

            starsByDay.Sort((x, y) => -x.Key.CompareTo(y.Key));

            return starsByDay.Select(c => c.Value).ToArray();
        }
    }
}
