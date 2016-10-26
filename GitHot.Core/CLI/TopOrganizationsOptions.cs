using CommandLine;

namespace GitHot.Core.CLI
{
    class TopOrganizationsOptions
    {
        [Option('c', "count", Required = true, HelpText = "Number of organizations to show", DefaultValue = 50)]
        public int Count { get; set; }

        [Option('w', "weeks", Required = true, HelpText = "Number of weeks for collecting statistics", DefaultValue = 1)]
        public int Weeks { get; set; }

        [Option('o', "output", HelpText = "Filepath for saving output")]
        public string Output { get; set; }

        [Option('t', "total", MutuallyExclusiveSet = "total", HelpText = "Get top organizations by total commits count")]
        public bool TotalCommits { get; set; }

        [Option('a', "avg", MutuallyExclusiveSet = "avg", HelpText = "Get top organizations by average commits count")]
        public bool AverageCommits { get; set; }

        public string GetUsageDescription(string givenName)
        {
            return $"githot {givenName} - discover top organizations by certain criteria.\n" +
                   $"Usage: githot {givenName} -o <path> [-c|--count] [-w|--weeks] [-t|--total|-a|--avg]";
        }
    }
}
