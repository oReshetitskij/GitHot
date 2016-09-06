using System;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit.Internal;
using System.IO;
using System.Web.Script.Serialization;

namespace GitHot.Core
{
    class Configuration
    {
        public string Token { get; protected set; }
        public string HelperToken { get; protected set; }
        public int MaxSubscriptions { get; protected set; }
        public int PageCount { get; protected set; }
        public int ItemsPerPage { get; protected set; }

        private static Configuration _instance;

        public static Configuration Instance
        {
            get
            {
                if (_instance == null)
                {
                    var serializer = new SimpleJsonSerializer();

                    string json = File.ReadAllText("config.json");

                    _instance = serializer.Deserialize<Configuration>(json);
                }

                return _instance;
            }
            set { _instance = value; }
        }
    }
}
