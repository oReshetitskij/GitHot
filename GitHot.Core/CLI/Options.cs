using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace GitHot.Core.CLI
{
    class Options
    {
        [VerbOption("stats", HelpText = "Get statistics for repository")]
        public StatisticsOptions Stats { get; set; }

        [VerbOption("repos", HelpText = "Get top repositories by criteria")]
        public TopRepositoriesOptions Repos { get; set; }
    }
}
