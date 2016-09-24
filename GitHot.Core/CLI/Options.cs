using CommandLine;
using CommandLine.Text;

namespace GitHot.Core.CLI
{
    class Options
    {
        [VerbOption("stats", HelpText = "Get statistics for repository", MutuallyExclusiveSet = "stats")]
        public StatisticsOptions Stats { get; set; }

        [VerbOption("repos", HelpText = "Get top repositories by criteria", MutuallyExclusiveSet = "repos")]
        public TopRepositoriesOptions Repos { get; set; }

        [HelpOption(MutuallyExclusiveSet = "help")]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("GitHot - discover the most trending repositories right now!"),
                MaximumDisplayWidth = 80,
                AdditionalNewLineAfterOption = true
            };

            help.AddPreOptionsLine("Usage: githot <command> [<args>]");
            help.AddOptions(this);

            return help;
        }

        [HelpVerbOption(MutuallyExclusiveSet = "verb help")]
        public string GetUsage(string verb)
        {
            string usageDescription;
            object options;

            if (verb == "stats")
            {
                usageDescription = Stats.GetUsageDescription(verb);
                options = Stats;
            }
            else if (verb == "repos")
            {
                usageDescription = Repos.GetUsageDescription(verb);
                options = Repos;
            }
            else
            {
                return GetUsage();
            }

            HelpText verbHelp = new HelpText
            {
                Heading = usageDescription,
                AdditionalNewLineAfterOption = true,
                MaximumDisplayWidth = 80
            };


            verbHelp.AddOptions(options);
            return verbHelp;
        }
    }
}
