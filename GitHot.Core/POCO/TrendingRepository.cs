using System;
using Octokit;

namespace GitHot.Core.POCO
{
    class TrendingRepository
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string OwnerName { get; set; }
        public string OwnerUrl { get; set; }
        public int[] Stars { get; set; }
        public int[] Commits { get; set; }
        public int[] Contributors { get; set; }

        public TimeSpan Span { get; set; }
    }
}
