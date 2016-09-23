using CommandLine;
using CommandLine.Text;

namespace GitHot.Core.CLI
{
    public class StatisticsOptions
    {
        [ValueOption(0)]
        public string Repository { get; set; }

        [Option("stargazers", MutuallyExclusiveSet = "stargazers", HelpText = "Get statistics only by stargazers")]
        public bool Stargazers { get; set; }

        [Option("commits", MutuallyExclusiveSet = "commits", HelpText = "Get statistics only by commits")]
        public bool Commits { get; set; }

        [Option("contributors", MutuallyExclusiveSet = "contributors", HelpText = "Get statistics only by contributors")]
        public bool Contributors { get; set; }

        [Option('v', "verbose", HelpText = "Provide verbose output")]
        public string Verbose { get; set; }

        [Option('o', "output", HelpText = "Write output to <file>", Required = true)]
        public string Output { get; set; }

        [Option('s', "span", HelpText = "Time bounds for collecting statistics in format dd.hh:mm:ss (<days>.<hours>:<minutes>:<seconds>)",
            DefaultValue = "7.00:00:00")]
        public string Span { get; set; }

        public bool All => !(Commits || Contributors || Stargazers);

        public string GetUsageDescription(string givenName)
        {
            return
                $"githot {givenName} - find out repository statistics in given period of time.\n" +
                $"Usage: githot {givenName} -o <path> [--stargazers|--commits|--contributors] [-v] [-s|--span <timespan>]";
        }

        public string GetSelectedCriteria()
        {
            if (Commits)
            {
                return "Commits";
            }
            if (Contributors)
            {
                return "Contributors";
            }
            if (Stargazers)
            {
                return "Stargazers";
            }

            return null;
        }
    }
}
