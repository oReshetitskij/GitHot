using System.Web.Hosting;
using Nancy;

namespace GitHot.Bootstrap
{
    public class AspNetRootPathProvider : IRootPathProvider
    {
        public string GetRootPath()
        {
            return HostingEnvironment.MapPath("~/");
        }
    }
}