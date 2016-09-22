using CommandLine;
using CommandLine.Text;

namespace GitHot.Core.CLI
{
    public class StatisticsOptions
    {
        [ValueOption(0)]
        public string Repository { get; set; }

        [Option("stargazers", MutuallyExclusiveSet = "stargazers", HelpText = "Get statistics by stargazers")]
        public bool Stargazers { get; set; }

        [Option("commits", MutuallyExclusiveSet = "commits", HelpText = "Get statistics by commits")]
        public bool Commits { get; set; }

        [Option("contributors", MutuallyExclusiveSet = "contributors", HelpText = "Get statistics by contributors")]
        public bool Contributors { get; set; }

        [Option("all", MutuallyExclusiveSet = "all", HelpText = "Get statistics by all provided criterias")]
        public bool All { get; set; }

        [Option('v', "verbose", HelpText = "Provide verbose output")]
        public string Verbose { get; set; }

        [Option('o', "output", HelpText = "Write output to <file>", Required = true)]
        public string Output { get; set; }

        [Option('s', "span", HelpText = "Time bounds for collecting statistics in format dd.hh:mm:ss (<days>.<hours>:<minutes>:<seconds>)",
            DefaultValue = "7.00:00:00")]
        public string Span { get; set; }
    }
}
