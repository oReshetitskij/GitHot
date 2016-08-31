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

            Repository repo = client.Repository.Get("PowerShell", "PowerShell").Wait();

            int days = 14;

            Console.WriteLine("*** With page skipping ***");

            Stopwatch sw = Stopwatch.StartNew();

            client.GetTrendingStats(repo, TimeSpan.FromDays(days)).Subscribe(onNext:
                (data) =>
                {
                    string content = string.Join(" ", data.Item2.Select(x => x.ToString()));
                    Console.WriteLine($"{repo.FullName}: {content} | {sw.Elapsed}");
                }, onCompleted: () => { sw.Stop(); });

            Console.ReadLine();
        }
    }
}

