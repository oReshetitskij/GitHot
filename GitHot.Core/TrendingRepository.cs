using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using Octokit.Reactive;
using System.Reactive.Linq;
using System.Reactive.Joins;

namespace GitHot.Core
{
    class TrendingRepository
    {
        private TrendingRepository(Repository repo, int stars, int commits, int contributors)
        {
            Id = repo.Id;
            Name = repo.Name;
            Url = repo.HtmlUrl;
            OwnerName = repo.Owner.Login;
            OwnerUrl = repo.Owner.Url;
            StarsCount = stars;
            CommitsCount = commits;
            ContributorsCount = contributors;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string OwnerName { get; set; }
        public string OwnerUrl { get; set; }
        public int StarsCount { get; set; }
        public int CommitsCount { get; set; }
        public int ContributorsCount { get; set; }
    }
}
