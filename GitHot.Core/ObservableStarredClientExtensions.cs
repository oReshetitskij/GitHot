using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Octokit;
using Octokit.Internal;
using Octokit.Reactive;

namespace GitHot.Core
{
    public static class ObservableStarredClientExtensions
    {
        public static IObservable<int[]> GetStarCount(this IObservableStarredClient client, Repository repo, TimeSpan span)
        {
            return Observable.Create<int[]>(observer =>
            {
                DateTime to = DateTime.Now;
                DateTime from = to.Add(-span);

                const int pageSize = 100;
                int startPage;

                SimpleJsonSerializer serializer = new SimpleJsonSerializer();
                string pattern = "\"starred_at\": \"(.*)\",";
                Regex re = new Regex(pattern, RegexOptions.Compiled);

                for (int i = 1; ; i++)
                {

                    using (WebClient web = new WebClient())
                    {
                        web.Headers.Add("User-agent",
                                "Mozilla / 5.0(Windows NT 6.1; WOW64; rv: 40.0) Gecko / 20100101 Firefox / 40.1");
                        web.Headers.Add("Accept", "application/vnd.github.v3.star+json");
                        web.Headers.Add("Authorization", "token 085009b1c518cc49dff46bf421e6c3ffbf87965c");

                        string json = web.DownloadString(
                            $"https://api.github.com/repos/{repo.FullName}/stargazers?per_page={pageSize}&page={i}");

                        if (json == "[\n\n]\n")
                        {
                            startPage = i - 1;
                            break;
                        }

                        MatchCollection matches = re.Matches(json);

                        Match match = matches[matches.Count - 1];

                        GroupCollection groups = match.Groups;
                        DateTime date = Convert.ToDateTime(groups[1].Value).Date;
                        if (date > from)
                        {
                            startPage = i;
                            break;
                        }
                    }
                }

                List<UserStar> starsArray = new List<UserStar>();

                for (int i = startPage; ; i++)
                {
                    using (WebClient web = new WebClient())
                    {
                        web.Headers.Add("User-agent",
                                "Mozilla / 5.0(Windows NT 6.1; WOW64; rv: 40.0) Gecko / 20100101 Firefox / 40.1");
                        web.Headers.Add("Accept", "application/vnd.github.v3.star+json");
                        web.Headers.Add("Authorization", "token 085009b1c518cc49dff46bf421e6c3ffbf87965c");

                        string json = web.DownloadString(
                            $"https://api.github.com/repos/{repo.FullName}/stargazers?per_page={pageSize}&page={i}");
                        if (json == "[\n\n]\n")
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

                starsByDay.Sort((x, y) => x.Key.CompareTo(y.Key));

                observer.OnNext(starsByDay.Select(c => c.Value).ToArray());
                observer.OnCompleted();

                return Disposable.Empty;
            });
        }  
    }
}
