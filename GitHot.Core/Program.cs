using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Octokit;
using Octokit.Internal;
using System.Threading.Tasks;
using CommandLine;
using GitHot.Core.CLI;
using GitHot.Core.POCO;

namespace GitHot.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            args = new[] { "repos", "-o", "contributors.json", "--contributors", "--count", "100", "--weeks", "2" };

            var options = new Options();
            bool result = Parser.Default.ParseArguments(args, options, onVerbCommand:
                (verb, subOption) =>
                {
                    switch (verb)
                    {
                        case "stats":
                            var statsOption = (StatisticsOptions)subOption;
                            Task getStats = GetStatisticsCount(statsOption);
                            Task.WaitAll(getStats);
                            break;
                        case "repos":
                            var topReposOption = (TopRepositoriesOptions)subOption;
                            Task getRepos = GetTopRepositories(topReposOption);
                            Task.WaitAll(getRepos);
                            break;
                        case "orgs":
                            var topOrgsOption = (TopOrganizationsOptions)subOption;
                            Task getOrgs = GetTopOrganizations(topOrgsOption);
                            Task.WaitAll(getOrgs);
                            break;
                    }
                });

            Console.WriteLine($"Parsing successful: {result}");
        }

        public static async Task GetStatisticsCount(StatisticsOptions opt)
        {
            GitHubClient github = new GitHubClient(new ProductHeaderValue("GitHot"))
            {
                Credentials = new Credentials(Configuration.Instance.Token)
            };

            string[] repoPath = opt.Repository.Split('/');
            TimeSpan span;
            try
            {
                span = TimeSpan.Parse(opt.Span);
            }
            catch (FormatException)
            {
                Console.WriteLine("Wrong timespan format");
                Console.WriteLine("See help for more details");
                return;
            }

            Repository repo;
            try
            {
                repo = await github.Repository.Get(repoPath[0], repoPath[1]);
            }
            catch (ApiException e)
            {
                Console.WriteLine($"Error occured while processing request: {e.Message}");
                return;
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Wrong format for repository path.");
                Console.WriteLine("See help for more details");
                return;
            }

            Dictionary<RepositoryCriteria, int[]> stats;
            if (opt.All)
            {
                stats = await github.GetTrendingStats(repo, span);
            }
            else
            {
                RepositoryCriteria selectedCriteria = (RepositoryCriteria)Enum.Parse(typeof(RepositoryCriteria),
                                                                                opt.GetSelectedCriteria());

                var criteriaStats = await github.GetTrendingStats(repo, span, selectedCriteria);

                stats = new Dictionary<RepositoryCriteria, int[]>();
                foreach (var criteria in Enum.GetValues(typeof(RepositoryCriteria)))
                {
                    if ((RepositoryCriteria)criteria == selectedCriteria)
                    {
                        stats.Add(selectedCriteria, criteriaStats.Item2);
                        continue;
                    }

                    stats.Add((RepositoryCriteria)criteria, null);
                }
            }

            TrendingRepository data = new TrendingRepository
            {
                Name = repo.Name,
                OwnerName = repo.Owner.Login,
                Id = repo.Id,
                Url = repo.HtmlUrl,
                OwnerUrl = repo.Owner.HtmlUrl,
                Commits = stats[RepositoryCriteria.Commits],
                Stars = stats[RepositoryCriteria.Stargazers],
                Contributors = stats[RepositoryCriteria.Contributors],
                Span = span
            };

            SimpleJsonSerializer serializer = new SimpleJsonSerializer();
            string json = serializer.Serialize(data);

            try
            {
                FileStream file = File.Create(opt.Output);
                file.Close();

                StreamWriter sw = new StreamWriter(opt.Output);
                sw.Write(json);
                sw.Close();
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid output path");
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine($"Directory not found: {Path.GetFullPath(opt.Output)}");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Not enough privilegies to create file");
            }
        }

        public static async Task GetTopRepositories(TopRepositoriesOptions opt)
        {
            GitHubClient github = new GitHubClient(new ProductHeaderValue("GitHot"))
            {
                Credentials = new Credentials(Configuration.Instance.Token)
            };

            string criteria;
            Dictionary<Repository, int> repos;

            if (opt.Commits)
            {
                repos = await github.GetTopRepositoriesByCommits(opt.Weeks, opt.Count);
                criteria = "commits";
            }
            else if (opt.Contributors)
            {
                repos = await github.GetTopRepositoriesByContributors(opt.Weeks, opt.Count);
                criteria = "contributors";
            }
            else
            {
                repos = await github.GetTopRepositoriesByStargazers(opt.Weeks, opt.Count);
                criteria = "stargazers";
            }

            var data = repos.ToList().Select(pair => new TopRepository
            {
                Value = pair.Value,
                Criteria = criteria,
                Id = pair.Key.Id,
                Name = pair.Key.Name,
                Url = pair.Key.HtmlUrl,
                OwnerName = pair.Key.Owner.Login,
                OwnerUrl = pair.Key.Owner.HtmlUrl,
                Weeks = opt.Weeks
            }).ToList();

            data.Sort((x, y) => -x.Value.CompareTo(y.Value));

            SimpleJsonSerializer serializer = new SimpleJsonSerializer();
            string json = serializer.Serialize(data);

            try
            {
                StreamWriter sw = new StreamWriter(opt.Output, false);
                sw.Write(json);
                sw.Close();
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid output path");
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine($"Directory not found: {Path.GetFullPath(opt.Output)}");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Not enough privilegies to create file");
            }
        }

        private static async Task GetTopOrganizations(TopOrganizationsOptions opt)
        {
            GitHubClient github = new GitHubClient(new ProductHeaderValue("GitHot"))
            {
                Credentials = new Credentials(Configuration.Instance.Token)
            };

            Dictionary<User, double> orgs;

            if (opt.TotalCommits)
            {
                orgs = (await github.GetTopOrganizationsByTotalCommits(opt.Weeks, opt.Count))
                        .ToDictionary(x => x.Key, x => Convert.ToDouble(x.Value));
            }
            else
            {
                orgs = await github.GetTopOrganizationsByMemberAverageCommits(opt.Weeks, opt.Count);
            }


            var data = orgs.ToList().Select(pair => new TopOrganization
            {
                Id = pair.Key.Id,
                Name = pair.Key.Name,
                Url = pair.Key.HtmlUrl,
                Span = TimeSpan.FromDays(opt.Weeks * 7),
                TotalCommits = opt.TotalCommits ? new int?(Convert.ToInt32(pair.Value)) : null,
                MemberAverageCommits = opt.AverageCommits ? new double?(pair.Value) : null
            }).ToList();

            if (opt.TotalCommits)
            {
                data.Sort((x, y) => -x.TotalCommits?.CompareTo(y.TotalCommits ?? 0) ?? 0);
            }
            else
            {
                data.Sort((x, y) => -x.MemberAverageCommits?.CompareTo(y.MemberAverageCommits ?? 0) ?? 0);
            }

            SimpleJsonSerializer serializer = new SimpleJsonSerializer();
            string json = serializer.Serialize(data);

            try
            {
                StreamWriter sw = new StreamWriter(opt.Output, false);
                sw.Write(json);
                sw.Close();
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid output path");
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine($"Directory not found: {Path.GetFullPath(opt.Output)}");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Not enough privilegies to create file");
            }
        }
    }
}

