using Octokit.Internal;
using System.IO;

namespace GitHot.Core
{
    public class Configuration
    {
        public string Token { get; protected set; }
        public int MaxSubscriptions { get; protected set; }
        public int PageCount { get; protected set; }
        public int ItemsPerPage { get; protected set; }
        public int ActivityDelay { get; protected set; }
        public int ActivityMaxDelay { get; protected set; }
        public int ActivityRetryCount { get; protected set; }

        private static Configuration _instance;

        public static string InstancePath { private get; set; } = ".";

        public static Configuration Instance
        {
            get
            {
                if (_instance == null)
                {
                    var serializer = new SimpleJsonSerializer();

                    string json = File.ReadAllText(Path.Combine(InstancePath, "config.json"));

                    _instance = serializer.Deserialize<Configuration>(json);
                }

                return _instance;
            }
            set { _instance = value; }
        }
    }
}
