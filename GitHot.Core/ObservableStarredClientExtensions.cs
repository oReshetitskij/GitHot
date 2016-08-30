using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using Octokit.Reactive;

namespace GitHot.Core
{
    public static class ObservableStarredClientExtensions
    {
        public static IObservable<int[]> GetStarCount(this ObservableStarredClient client, Repository repo, TimeSpan span)
        {
            return Observable.Create<int[]>(async (observer) =>
            {
                DateTime to = DateTime.Now;
                DateTime from = to.Add(-span);

                IDictionary<DateTime, Task<int>> stars = await client.GetAllStargazersWithTimestamps(repo.Owner.Login, repo.Name)
                .Select(star => Observable.Start(() => star))
                    .Merge(100)
                    .GroupBy(x => x.StarredAt.LocalDateTime.Date).ToDictionary(pair => pair.Key, pair => pair.Count().ToTask());

                // Sort by days, as Dictionary order in undefined
                var starsByDay = new List<KeyValuePair<DateTime, int>>();

                for (int i = 1; i <= span.Days; i++)
                {
                    DateTime date = from.AddDays(i).Date;
                    int starsCount = stars.ContainsKey(date) ? await stars[date] : 0;
                    starsByDay.Add(new KeyValuePair<DateTime, int>(date, starsCount));
                }

                starsByDay.Sort((x, y) => x.Key.CompareTo(y.Key));

                observer.OnNext(starsByDay.Select(c => c.Value).ToArray());
                observer.OnCompleted();

                return Disposable.Empty;
            }
            );
        }
    }
}
