using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
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
                try
                {
                    DateTime to = DateTime.Now;
                    DateTime from = to.Add(-span);

                    string pattern = "\"starred_at\": \"(.*)\",";
                    Regex re = new Regex(pattern, RegexOptions.Compiled);

                    int lastPage = repo.StargazersCount / 100;
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

                            string json = web.DownloadString(
                                $"https://api.github.com/repos/{repo.FullName}/stargazers?per_page={Configuration.Instance.ItemsPerPage}&page={i}");

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

                    var stars = starsArray.GroupBy(x => x.StarredAt.DateTime.Date)
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
                }

                catch (WebException)
                {
                    observer.OnNext(new int[span.Days]);
                    observer.OnCompleted();
                }

                return Disposable.Empty;
            });
        }
    }
}
