using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GitHot.Modules
{
    public class TopOrganizationsModule : NancyModule
    {
        public TopOrganizationsModule()
        {
            Get["/orgs/{criteria}/{weeks}"] = param =>
            {
                string criteria = (string)(param.criteria);

                switch (criteria)
                {
                    case "commits":
                    case "commitsAvg":
                    default:
                        break;
                }

                //TODO provide return value
                return null;
            };
        }
    }
}