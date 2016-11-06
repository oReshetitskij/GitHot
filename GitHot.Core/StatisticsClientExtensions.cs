using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Octokit;
using Octokit.Internal;

namespace GitHot.Core
{
    public static class StatisticsClientExtensions
    {
        public static async Task<List<WeeklyCommitActivity>> GetCommitActivityRaw(this IStatisticsClient client,
            Repository repo)
        {
            SimpleJsonSerializer serializer = new SimpleJsonSerializer();

            double timeout = Configuration.Instance.ActivityDelay;
            double maxTimeout = Configuration.Instance.ActivityMaxDelay;
            string json;

            for (int i = 1; ; i++)
            {
                using (WebClient web = new WebClient())
                {
                    web.Headers.Add("User-Agent",
                        "Mozilla / 5.0(Windows NT 6.1; WOW64; rv: 40.0) Gecko / 20100101 Firefox / 40.1");
                    web.Headers.Add("Authorization", $"token {Configuration.Instance.Token}");

                    Debug.WriteLine($"Processing {repo.FullName}: timeout {timeout}");
                    json = web.DownloadString($"https://api.github.com/repos/{repo.FullName}/stats/commit_activity");

                    if (web.ResponseHeaders["Status"] == "202 Accepted")
                    {
                        if (i > Configuration.Instance.ActivityRetryCount)
                            throw new TimeoutException($"More than {Configuration.Instance.ActivityRetryCount} tries applied on {repo.FullName}");

                        await Task.Delay(TimeSpan.FromMilliseconds(timeout));
                        timeout = timeout < maxTimeout ? timeout * 2 : timeout;
                    }
                    else if (web.ResponseHeaders["Status"] == "200 OK")
                    {
                        Debug.WriteLine($"Finished processing {repo.FullName} in {i} tries");
                        break;
                    }
                    else
                    {
                        Debug.WriteLine($"[ERROR]: Unknown status on {repo.FullName}");
                        return new List<WeeklyCommitActivity>();
                    }
                }
            }

            return serializer.Deserialize<List<WeeklyCommitActivity>>(json);
        }

        public static async Task<List<Contributor>> GetContributorsRaw(this IStatisticsClient client,
            Repository repo)
        {
            double timeout = Configuration.Instance.ActivityDelay;
            double maxTimeout = Configuration.Instance.ActivityMaxDelay;
            string json;
            try
            {
                for (int i = 1; ; i++)
                {
                    using (WebClient web = new WebClient())
                    {
                        web.Headers.Add("User-Agent",
                            "Mozilla / 5.0(Windows NT 6.1; WOW64; rv: 40.0) Gecko / 20100101 Firefox / 40.1");
                        web.Headers.Add("Authorization", $"token {Configuration.Instance.Token}");

                        Debug.WriteLine($"Processing {repo.FullName}: timeout {timeout}");
                        json = web.DownloadString($"https://api.github.com/repos/{repo.FullName}/stats/contributors");

                        if (web.ResponseHeaders["Status"] == "202 Accepted")
                        {
                            if (i > Configuration.Instance.ActivityRetryCount)
                                throw new TimeoutException($"More than {Configuration.Instance.ActivityRetryCount} tries applied on {repo.FullName}");

                            await Task.Delay(TimeSpan.FromMilliseconds(timeout));
                            timeout = timeout < maxTimeout ? timeout * 2 : timeout;
                        }
                        else if (web.ResponseHeaders["Status"] == "200 OK")
                        {
                            Debug.WriteLine($"Finished processing {repo.FullName} in {i} tries");
                            break;
                        }
                        else
                        {
                            Debug.WriteLine($"[ERROR]: Unknown status on {repo.FullName}");
                            return new List<Contributor>();
                        }
                    }
                }
            }
            catch (WebException)
            { 
                Debug.WriteLine($"WebException caught while processing {repo.FullName}");
                throw;
            }

            try
            {
                SimpleJsonSerializer serializer = new SimpleJsonSerializer();
                return serializer.Deserialize<List<Contributor>>(json);
            }
            catch (Exception)
            {
                Debug.WriteLine($"Exception caught while processing {repo.FullName}");
                throw;
            }
        }
    }
}
