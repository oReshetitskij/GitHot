using System;

namespace GitHot.Core.POCO
{
    public class TrendingRepository
    {
        public long Id { get; set; }
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
