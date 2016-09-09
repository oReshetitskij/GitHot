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
            ObservableGitHubClient observableClient = new ObservableGitHubClient(new ProductHeaderValue("GitHot"), credentialStore);
            GitHubClient client = new GitHubClient(observableClient.Connection);

            int days = 21;
            int topCount = 50;


            Console.WriteLine("Comparing performance of Task and Observable methods");

            Repository repo = client.Repository.Get("docker", "docker").Result;
            Console.WriteLine($"Collecting stats for {repo.FullName} in {days} days");

            Console.WriteLine("*** Tasks ***");
            Stopwatch sw = Stopwatch.StartNew();

            var taskStats = client.GetTrendingStats(repo, TimeSpan.FromDays(days)).Result;

            foreach (var pair in taskStats)
            {
                Console.WriteLine($"{pair.Key}: {string.Join(" ", pair.Value.Select(x => x.ToString()))}");
            }
            sw.Stop();
            Console.WriteLine($"Finished at {sw.Elapsed}");

            Console.WriteLine("*** Observables ***");
            sw.Restart();
            var observableStats = observableClient.GetTrendingStats(repo, TimeSpan.FromDays(days)).Subscribe(
                onNext: data =>
                {
                    Console.WriteLine($"{data.Item1}: {string.Join(" ", data.Item2.Select(x => x.ToString()))}");
                },
                onCompleted: () =>
                {
                    Console.WriteLine($"Finished at: {sw.Elapsed}");
                },
                onError: ex => { throw ex; });

            Console.ReadLine();
        }
    }
}

