using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GitHot.Modules
{
    public class TopRepositoriesModule : NancyModule
    {
        public TopRepositoriesModule()
        {
            Get["/repos/{criteria}/{weeks}"] = param =>
            {
                string criteria = (string)(param.criteria);

                switch (criteria)
                {
                    case "stars":
                    case "contribs":
                    case "commits":

                    default:
                        break;
                }

                //TODO provide return value
                return null;
            };
        }
    }
}