using System;

namespace GitHot.Core.POCO
{
    class TopOrganization
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public int? TotalCommits { get; set; }
        public double? MemberAverageCommits { get; set; }
        public TimeSpan Span { get; set; }
    }
}
