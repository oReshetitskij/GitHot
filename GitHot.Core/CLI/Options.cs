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
                Heading = new HeadingInfo("GitHot"),
                MaximumDisplayWidth = 80
            };

            help.AddPreOptionsLine("githot <command> [<args>]");

            return help;
        }
    }
}
