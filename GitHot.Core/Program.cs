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
        //TODO: place token and maximum subscribers count for Merge calls in configuration file 
        internal static string Token = "43c9828893e4dc1d354ad7f760eee3c31fac4970";

        static void Main(string[] args)
        {
            InMemoryCredentialStore credentialStore = new InMemoryCredentialStore(new Credentials(Token));
            ObservableGitHubClient client = new ObservableGitHubClient(new ProductHeaderValue("GitHot"), credentialStore);

            Repository repo = client.Repository.Get("apple", "swift").Wait();

            int days = 21;
            int topCount = 50;

            Console.WriteLine("*** Getting top repos by Commits ***");

            Stopwatch sw = Stopwatch.StartNew();

            client.GetTopRepositoriesByCommits(days / 7, topCount).Subscribe(onNext:
                (data) =>
                {
                    foreach (var repoInfo in data)
                    {
                        Console.WriteLine($"{repoInfo.Key.FullName}: {repoInfo.Value} | {sw.Elapsed}");
                    }
                }, onCompleted: () => { sw.Stop(); Console.WriteLine($"Finished at: {sw.Elapsed}"); });

            Console.ReadLine();
        }
    }
}

