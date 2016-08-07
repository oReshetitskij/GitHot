using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GitHot.Modules
{
    public class RepositoryModule : NancyModule
    {
        public RepositoryModule()
        {
            Get["/"] = param => View["Index.html"];

            Get["/{owner}/{repo}/{criteria}/{weeks}"] = param => {
                
                //TODO provide return value
                return null;
            };
        }
    }
}