using System;
using System.Diagnostics;
using System.Linq;
using Octokit;
using Octokit.Internal;
using Octokit.Reactive;
using System.Reactive.Linq;

namespace GitHot.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            InMemoryCredentialStore credentialStore = new InMemoryCredentialStore(new Credentials(Configuration.Instance.Token));
            ObservableGitHubClient client = new ObservableGitHubClient(new ProductHeaderValue("GitHot"), credentialStore);

            int days = 21;
            int topCount = 50;

            Stopwatch sw = Stopwatch.StartNew();

            Console.WriteLine("*** Getting top repos by Stargazers ***");

            client.GetTopRepositoriesByStargazers(days / 7, topCount).Subscribe(onNext:
                (data) =>
                {
                    foreach (var repoInfo in data)
                    {
                        Console.WriteLine($"{repoInfo.Key.FullName}: {repoInfo.Value} | {sw.Elapsed}");
                    }
                }, onCompleted: () => { sw.Stop(); Console.WriteLine($"Finished at: {sw.Elapsed}"); },
                   onError: (ex) =>
                   {
                       throw ex;
                   });

            Console.ReadLine();
        }
    }
}

