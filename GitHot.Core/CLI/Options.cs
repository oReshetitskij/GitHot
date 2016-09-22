using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace GitHot.Core.CLI
{
    class Options
    {
        [VerbOption("stats", HelpText = "Get statistics for repository")]
        public StatisticsOptions Stats { get; set; }

        [VerbOption("repos", HelpText = "Get top repositories by criteria")]
        public TopRepositoriesOptions Repos { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("GitHot - discover the most trending repositories right now!"),
                MaximumDisplayWidth = 80
            };

            help.AddPreOptionsLine("Usage: githot <command> [<args>]");
            help.AddOptions(this);

            return help;
        }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            return HelpText.AutoBuild(this, verb);
        }
    }
}
