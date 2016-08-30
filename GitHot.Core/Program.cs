using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using Octokit.Internal;
using Octokit.Reactive;
using System.Reactive.Linq;
using System.IO;
using System.Reactive.Disposables;

namespace GitHot.Core
{
    class Program
    {
        private static string token = "";

        static void Main(string[] args)
        {
            InMemoryCredentialStore credentialStore = new InMemoryCredentialStore(new Credentials(token));
            ObservableGitHubClient client = new ObservableGitHubClient(new ProductHeaderValue("GitHot"), credentialStore);

            Repository repo = client.Repository.Get("rust-lang", "rust").Wait();

            int days = 14;

            Stopwatch sw = Stopwatch.StartNew();

            string[] content;
            Console.WriteLine("**** Multiple awaits ****");

            sw.Restart();
            client.GetTrendingStats(repo, TimeSpan.FromDays(days))
                .Subscribe(onNext: data =>
                {
                    content = data.Item2.Select(c => c.ToString()).ToArray();
                    Console.WriteLine($"{repo.FullName}: {string.Join(" ", content)} {data.Item1} | {days} days | {sw.Elapsed}");
                },
                onCompleted: () => { sw.Stop(); Console.WriteLine($"completed at {sw.Elapsed}"); });

            Console.ReadLine();
        }
    }
}
