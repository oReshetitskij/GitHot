using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;

namespace GitHot.Bootstrap
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override IRootPathProvider RootPathProvider
        {
            get
            {
                return new AspNetPathProvider();
            }
        }
    }
}