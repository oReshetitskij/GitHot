using System;
using System.Diagnostics;
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

            //Repository repo = client.Repository.Get("apple", "swift").Wait();

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
                }, onCompleted: () => { sw.Stop(); Console.WriteLine($"Finished at: {sw.Elapsed}"); },
                   onError: (ex) =>
                   {
                       Console.WriteLine("Program terminated due to exception:");
                       Console.WriteLine(ex);
                   });

            Console.ReadLine();
        }
    }
}

