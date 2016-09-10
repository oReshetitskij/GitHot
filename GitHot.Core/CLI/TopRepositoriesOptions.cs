using CommandLine;
using CommandLine.Text;

namespace GitHot.Core.CLI
{
    class TopRepositoriesOptions
    {
        [Option('c', "count", Required = true, HelpText = "Number of repositories to show", DefaultValue = 50)]
        public int? Count { get; set; }

        [Option('o', "output", HelpText = "Filepath for saving output")]
        public string Output { get; set; }

        [Option("stargazers", MutuallyExclusiveSet = "stargazers", HelpText = "Get top repositories by stargazers")]
        public bool Stargazers { get; set; }

        [Option("commits", MutuallyExclusiveSet = "commits", HelpText = "Get top repositories by commits")]
        public bool Commits { get; set; }

        [Option("contributors", MutuallyExclusiveSet = "contributors", HelpText = "Get top repositories by contributors")]
        public bool Contributors { get; set; }
    }
}
