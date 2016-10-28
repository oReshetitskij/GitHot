using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using Nancy;

namespace GitHot.Bootstrap
{
    public class AspNetPathProvider : IRootPathProvider
    {
        public string GetRootPath()
        {
            return HostingEnvironment.MapPath("~/");
        }
    }
}