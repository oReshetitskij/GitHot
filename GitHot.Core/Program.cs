using System;
using System.Diagnostics;
using System.Linq;
using Octokit;
using Octokit.Internal;
using Octokit.Reactive;
using System.Reactive.Linq;
using GitHot.Core.CLI;

namespace GitHot.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                Console.WriteLine("Parse completed without errors.");
            }
            else
            {
                Console.WriteLine(options.GetUsage());
            }
        }
    }
}

