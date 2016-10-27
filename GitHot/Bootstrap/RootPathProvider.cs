using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Nancy;

namespace GitHot.Bootstrap
{
    public class RootPathProvider : IRootPathProvider
    {
        public string GetRootPath()
        {
            string dir = Directory.GetCurrentDirectory();
            return Path.GetFullPath(dir);
        }
    }
}